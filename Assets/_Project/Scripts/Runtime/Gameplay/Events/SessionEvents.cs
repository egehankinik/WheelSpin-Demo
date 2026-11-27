using WheelOfFortune.Core.Events;

namespace WheelOfFortune.Gameplay.Events
{
    public readonly struct ZoneChangedEvent : IGameEvent
    {
        public ZoneChangedEvent(int zone, ZoneTypeModel zoneType)
        {
            Zone = zone;
            ZoneType = zoneType;
        }

        public int Zone { get; }
        public ZoneTypeModel ZoneType { get; }
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
        public BombHitEvent(int reviveCost, bool isReviveAffordable)
        {
            ReviveCost = reviveCost;
            IsReviveAffordable = isReviveAffordable;
        }

        public int ReviveCost { get; }
        public bool IsReviveAffordable { get; }
    }

    public readonly struct RewardsCollectedEvent : IGameEvent
    {
        public RewardsCollectedEvent(int reviveCurrencyBalance)
        {
            ReviveCurrencyBalance = reviveCurrencyBalance;
        }

        public int ReviveCurrencyBalance { get; }
    }

    public readonly struct RewardAmountChangedEvent : IGameEvent
    {
        public RewardAmountChangedEvent(RewardModel reward, int total)
        {
            Reward = reward;
            Total = total;
        }

        public RewardModel Reward { get; }
        public int Total { get; }
    }

    public readonly struct RewardsClearedEvent : IGameEvent
    {
    }
}
