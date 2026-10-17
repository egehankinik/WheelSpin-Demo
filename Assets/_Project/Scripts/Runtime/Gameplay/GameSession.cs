using System;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public sealed class GameSession
    {
        private readonly ZoneRules _rules;
        private readonly IWheelProvider _wheels;
        private readonly IRandomProvider _random;
        private SpinResult _pendingResult;

        public GameSession(ZoneRules rules, IWheelProvider wheels, IRandomProvider random)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _wheels = wheels ?? throw new ArgumentNullException(nameof(wheels));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Inventory = new RewardInventory();
        }

        public event Action<int> ZoneChanged;
        public event Action BombHit;
        public event Action RewardsCollected;
        public event Action StateChanged;

        public RewardInventory Inventory { get; }
        public int CurrentZone { get; private set; }
        public SessionState State { get; private set; }
        public ZoneType CurrentZoneType => _rules.GetZoneType(CurrentZone);
        public WheelDefinition CurrentWheel => _wheels.GetWheel(CurrentZoneType);
        public bool CanSpin => State == SessionState.Idle;
        public bool CanLeave => State == SessionState.Idle && _rules.CanLeaveFrom(CurrentZoneType);
        public bool CanRevive => State == SessionState.Lost;

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
                BombHit?.Invoke();
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

            SetState(SessionState.Collected);
            RewardsCollected?.Invoke();
            return true;
        }

        public bool TryRevive()
        {
            if (!CanRevive)
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
            ZoneChanged?.Invoke(CurrentZone);
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke();
        }
    }
}
