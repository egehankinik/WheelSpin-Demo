using System;
using System.Collections.Generic;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    internal sealed class FixedRandomProvider : IRandomProvider
    {
        public int NextValue { get; set; }

        public int Range(int minInclusive, int maxExclusive)
        {
            return NextValue;
        }
    }

    internal sealed class FakeWallet : IWallet
    {
        private readonly Dictionary<RewardModel, int> _balances = new Dictionary<RewardModel, int>();

        public void SetBalance(RewardModel currency, int balance)
        {
            _balances[currency] = balance;
        }

        public int GetBalance(RewardModel currency)
        {
            if (_balances.TryGetValue(currency, out var balance))
            {
                return balance;
            }

            return currency.StartingBalance;
        }

        public void Add(RewardModel currency, int amount)
        {
            if (!currency.IsCurrency)
            {
                throw new ArgumentException($"{currency.Id} is not a currency.", nameof(currency));
            }

            _balances[currency] = GetBalance(currency) + amount;
        }

        public bool TrySpend(RewardModel currency, int amount)
        {
            var balance = GetBalance(currency);
            if (balance < amount)
            {
                return false;
            }

            _balances[currency] = balance - amount;
            return true;
        }
    }

    internal static class TestModels
    {
        public static RewardModel Item(string id)
        {
            return new RewardModel(id, false, 0);
        }

        public static RewardModel Currency(string id, int startingBalance = 0)
        {
            return new RewardModel(id, true, startingBalance);
        }

        public static WheelModel Wheel(params SliceModel[] slices)
        {
            return new WheelModel(slices);
        }

        public static ZoneTypeModel ZoneType(int interval, bool isSafe, params WheelTierModel[] tiers)
        {
            return new ZoneTypeModel(interval, isSafe, tiers);
        }
    }
}
