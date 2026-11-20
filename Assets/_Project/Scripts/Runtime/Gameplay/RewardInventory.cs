using System;
using System.Collections.Generic;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Gameplay
{
    public sealed class RewardInventory
    {
        private readonly Dictionary<RewardDefinition, int> _amounts = new Dictionary<RewardDefinition, int>();
        private readonly IEventBus _events;

        public RewardInventory(IEventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public bool IsEmpty => _amounts.Count == 0;

        public int GetAmount(RewardDefinition reward)
        {
            return reward != null && _amounts.TryGetValue(reward, out var amount) ? amount : 0;
        }

        public void Add(RewardDefinition reward, int amount)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var total = GetAmount(reward) + amount;
            _amounts[reward] = total;
            _events.Publish(new RewardAmountChangedEvent(reward, total));
        }

        public void Clear()
        {
            _amounts.Clear();
            _events.Publish(new RewardsClearedEvent());
        }
    }
}
