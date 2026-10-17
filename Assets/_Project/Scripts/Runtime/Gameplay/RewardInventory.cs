using System;
using System.Collections.Generic;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public sealed class RewardInventory
    {
        private readonly Dictionary<RewardDefinition, int> _amounts = new Dictionary<RewardDefinition, int>();

        public event Action<RewardDefinition, int> RewardChanged;
        public event Action Cleared;

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
            RewardChanged?.Invoke(reward, total);
        }

        public void Clear()
        {
            _amounts.Clear();
            Cleared?.Invoke();
        }
    }
}
