using System;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public sealed class EconomyRules
    {
        public EconomyRules(int reviveCost, RewardDefinition currencyReward)
        {
            if (reviveCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reviveCost));
            }

            ReviveCost = reviveCost;
            CurrencyReward = currencyReward;
        }

        public int ReviveCost { get; }
        public RewardDefinition CurrencyReward { get; }
    }
}
