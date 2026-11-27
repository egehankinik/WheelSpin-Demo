using System;

namespace WheelOfFortune.Gameplay
{
    public sealed class EconomyRules
    {
        public EconomyRules(RewardModel reviveCurrency, int reviveCost)
        {
            if (reviveCurrency == null)
            {
                throw new ArgumentNullException(nameof(reviveCurrency));
            }

            if (!reviveCurrency.IsCurrency)
            {
                throw new ArgumentException($"{reviveCurrency.Id} is not a currency.", nameof(reviveCurrency));
            }

            if (reviveCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reviveCost));
            }

            ReviveCurrency = reviveCurrency;
            ReviveCost = reviveCost;
        }

        public RewardModel ReviveCurrency { get; }
        public int ReviveCost { get; }
    }
}
