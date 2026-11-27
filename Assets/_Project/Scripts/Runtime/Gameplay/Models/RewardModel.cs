using System;

namespace WheelOfFortune.Gameplay
{
    public sealed class RewardModel
    {
        public RewardModel(string id, bool isCurrency, int startingBalance)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Reward id is required.", nameof(id));
            }

            if (startingBalance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingBalance));
            }

            Id = id;
            IsCurrency = isCurrency;
            StartingBalance = startingBalance;
        }

        public string Id { get; }
        public bool IsCurrency { get; }
        public int StartingBalance { get; }
    }
}
