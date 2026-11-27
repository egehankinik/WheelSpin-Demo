using System;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Gameplay
{
    public sealed class GameSession
    {
        #region Fields

        private readonly ZoneRules _rules;
        private readonly EconomyRules _economy;
        private readonly IRandomProvider _random;
        private readonly IWallet _wallet;
        private readonly IEventBus _events;
        private SpinResult _pendingResult;

        #endregion

        public GameSession(ZoneRules rules, EconomyRules economy, IRandomProvider random, IWallet wallet, IEventBus events)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            Inventory = new RewardInventory(_events);
        }

        #region Properties

        public RewardInventory Inventory { get; }
        public int CurrentZone { get; private set; }
        public SessionState State { get; private set; }

        public ZoneTypeModel CurrentZoneType
        {
            get { return _rules.GetZoneType(CurrentZone); }
        }

        public WheelModel CurrentWheel
        {
            get { return _rules.GetWheel(CurrentZone); }
        }

        public bool IsRunStartAllowed
        {
            get { return State == SessionState.NotStarted || State == SessionState.Collected; }
        }

        public bool IsSpinAllowed
        {
            get { return State == SessionState.Idle; }
        }

        public bool IsLeaveAllowed
        {
            get { return State == SessionState.Idle && _rules.CanLeaveFrom(CurrentZoneType); }
        }

        public bool IsReviveAllowed
        {
            get { return State == SessionState.Lost && _wallet.GetBalance(_economy.ReviveCurrency) >= _economy.ReviveCost; }
        }

        #endregion

        #region Public Methods

        public int GetSliceAmount(int sliceIndex)
        {
            var slice = CurrentWheel.Slices[sliceIndex];
            if (slice.IsBomb)
            {
                return 0;
            }

            return _rules.GetScaledAmount(slice.BaseAmount, CurrentZone);
        }

        public bool TryStartRun()
        {
            if (!IsRunStartAllowed)
            {
                return false;
            }

            BeginRun();
            return true;
        }

        public bool TryBeginSpin(out SpinResult result)
        {
            result = default;
            if (!IsSpinAllowed)
            {
                return false;
            }

            var wheel = CurrentWheel;
            var index = _random.Range(0, wheel.SliceCount);
            _pendingResult = new SpinResult(index, wheel.Slices[index], GetSliceAmount(index));
            result = _pendingResult;
            SetState(SessionState.Spinning);
            return true;
        }

        public void CompleteSpin()
        {
            if (State != SessionState.Spinning)
            {
                return;
            }

            if (_pendingResult.IsBomb)
            {
                SetState(SessionState.Lost);
                _events.Publish(new BombHitEvent(_economy.ReviveCost, IsReviveAllowed));
                return;
            }

            Inventory.Add(_pendingResult.Reward, _pendingResult.Amount);
            SetZone(CurrentZone + 1);
            SetState(SessionState.Idle);
        }

        public bool TryLeave()
        {
            if (!IsLeaveAllowed)
            {
                return false;
            }

            foreach (var collected in Inventory.Amounts)
            {
                if (collected.Key.IsCurrency)
                {
                    _wallet.Add(collected.Key, collected.Value);
                }
            }

            SetState(SessionState.Collected);
            _events.Publish(new RewardsCollectedEvent(_wallet.GetBalance(_economy.ReviveCurrency)));
            return true;
        }

        public bool TryRevive()
        {
            if (!IsReviveAllowed || !_wallet.TrySpend(_economy.ReviveCurrency, _economy.ReviveCost))
            {
                return false;
            }

            SetZone(CurrentZone + 1);
            SetState(SessionState.Idle);
            return true;
        }

        public bool TryGiveUp()
        {
            if (State != SessionState.Lost)
            {
                return false;
            }

            BeginRun();
            return true;
        }

        #endregion

        #region Private Methods

        private void BeginRun()
        {
            Inventory.Clear();
            SetZone(1);
            SetState(SessionState.Idle);
        }

        private void SetZone(int zone)
        {
            CurrentZone = zone;
            _events.Publish(new ZoneChangedEvent(CurrentZone, CurrentZoneType));
        }

        private void SetState(SessionState state)
        {
            State = state;
            _events.Publish(new SessionStateChangedEvent(State));
        }

        #endregion
    }
}
