using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay.Events
{
    public readonly struct ZoneChangedEvent : IGameEvent
    {
        public ZoneChangedEvent(int zone, ZoneTypeDefinition zoneType)
        {
            Zone = zone;
            ZoneType = zoneType;
        }

        public int Zone { get; }
        public ZoneTypeDefinition ZoneType { get; }
    }

    public readonly struct SessionStateChangedEvent : IGameEvent
    {
        public SessionStateChangedEvent(SessionState state)
        {
            State = state;
        }

        public SessionState State { get; }
    }

    public readonly struct BombHitEvent : IGameEvent
    {
        public BombHitEvent(int reviveCost, bool canAffordRevive)
        {
            ReviveCost = reviveCost;
            CanAffordRevive = canAffordRevive;
        }

        public int ReviveCost { get; }
        public bool CanAffordRevive { get; }
    }

    public readonly struct RewardsCollectedEvent : IGameEvent
    {
        public RewardsCollectedEvent(int depositedCurrency, int balance)
        {
            DepositedCurrency = depositedCurrency;
            Balance = balance;
        }

        public int DepositedCurrency { get; }
        public int Balance { get; }
    }

    public readonly struct RewardAmountChangedEvent : IGameEvent
    {
        public RewardAmountChangedEvent(RewardDefinition reward, int total)
        {
            Reward = reward;
            Total = total;
        }

        public RewardDefinition Reward { get; }
        public int Total { get; }
    }

    public readonly struct RewardsClearedEvent : IGameEvent
    {
    }
}
