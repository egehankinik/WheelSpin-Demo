using System;
using System.Collections.Generic;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Gameplay
{
    public sealed class RewardInventory
    {
        #region Fields

        private readonly Dictionary<RewardModel, int> _amounts = new Dictionary<RewardModel, int>();
        private readonly IEventBus _events;

        #endregion

        public RewardInventory(IEventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        #region Properties

        public bool IsEmpty
        {
            get { return _amounts.Count == 0; }
        }

        public IReadOnlyDictionary<RewardModel, int> Amounts
        {
            get { return _amounts; }
        }

        #endregion

        #region Public Methods

        public int GetAmount(RewardModel reward)
        {
            if (reward != null && _amounts.TryGetValue(reward, out var amount))
            {
                return amount;
            }

            return 0;
        }

        public void Add(RewardModel reward, int amount)
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

        #endregion
    }
}
