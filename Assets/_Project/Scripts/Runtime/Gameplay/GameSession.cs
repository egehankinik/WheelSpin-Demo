using System;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Gameplay
{
    public sealed class GameSession
    {
        private readonly ZoneRules _rules;
        private readonly EconomyRules _economy;
        private readonly IRandomProvider _random;
        private readonly IWallet _wallet;
        private readonly IEventBus _events;
        private SpinResult _pendingResult;

        public GameSession(ZoneRules rules, EconomyRules economy, IRandomProvider random, IWallet wallet, IEventBus events)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            Inventory = new RewardInventory(_events);
        }

        public RewardInventory Inventory { get; }
        public int CurrentZone { get; private set; }
        public SessionState State { get; private set; }
        public ZoneTypeDefinition CurrentZoneType => _rules.GetZoneType(CurrentZone);
        public WheelDefinition CurrentWheel => _rules.GetWheel(CurrentZone);
        public bool CanSpin => State == SessionState.Idle;
        public bool CanLeave => State == SessionState.Idle && _rules.CanLeaveFrom(CurrentZoneType);
        public bool CanRevive => State == SessionState.Lost && _wallet.Balance >= _economy.ReviveCost;

        public int GetSliceAmount(int sliceIndex)
        {
            var slice = CurrentWheel.Slices[sliceIndex];
            return slice.IsBomb ? 0 : _rules.GetScaledAmount(slice.BaseAmount, CurrentZone);
        }

        public void StartRun()
        {
            Inventory.Clear();
            SetZone(1);
            SetState(SessionState.Idle);
        }

        public bool TryBeginSpin(out SpinResult result)
        {
            result = default;
            var wheel = CurrentWheel;
            if (!CanSpin || wheel == null || wheel.SliceCount == 0)
            {
                return false;
            }

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
                _events.Publish(new BombHitEvent(_economy.ReviveCost, CanRevive));
                return;
            }

            Inventory.Add(_pendingResult.Reward, _pendingResult.Amount);
            SetZone(CurrentZone + 1);
            SetState(SessionState.Idle);
        }

        public bool TryLeave()
        {
            if (!CanLeave)
            {
                return false;
            }

            var earnedCurrency = Inventory.GetAmount(_economy.CurrencyReward);
            if (earnedCurrency > 0)
            {
                _wallet.Add(earnedCurrency);
            }

            SetState(SessionState.Collected);
            _events.Publish(new RewardsCollectedEvent(earnedCurrency, _wallet.Balance));
            return true;
        }

        public bool TryRevive()
        {
            if (!CanRevive || !_wallet.TrySpend(_economy.ReviveCost))
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

            StartRun();
            return true;
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
    }
}
