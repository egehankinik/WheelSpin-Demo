using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    internal sealed class FixedRandomProvider : IRandomProvider
    {
        public int NextValue { get; set; }

        public int Range(int minInclusive, int maxExclusive) => NextValue;
    }

    internal sealed class FakeWallet : IWallet
    {
        public FakeWallet(int balance)
        {
            Balance = balance;
        }

        public int Balance { get; private set; }

        public void Add(int amount) => Balance += amount;

        public bool TrySpend(int amount)
        {
            if (Balance < amount)
            {
                return false;
            }

            Balance -= amount;
            return true;
        }
    }

    internal sealed class TestAssets
    {
        private readonly List<Object> _created = new List<Object>();

        public RewardDefinition Reward()
        {
            var reward = Track(ScriptableObject.CreateInstance<RewardDefinition>());
            reward.Initialize(null);
            return reward;
        }

        public WheelDefinition Wheel(params WheelSlice[] slices)
        {
            var wheel = Track(ScriptableObject.CreateInstance<WheelDefinition>());
            wheel.Initialize(slices);
            return wheel;
        }

        public ZoneTypeDefinition ZoneType(int interval, bool isSafe, params WheelTier[] tiers)
        {
            var zoneType = Track(ScriptableObject.CreateInstance<ZoneTypeDefinition>());
            zoneType.Initialize(interval, isSafe, tiers);
            return zoneType;
        }

        public void DestroyAll()
        {
            foreach (var asset in _created)
            {
                Object.DestroyImmediate(asset);
            }

            _created.Clear();
        }

        private T Track<T>(T asset) where T : Object
        {
            _created.Add(asset);
            return asset;
        }
    }
}
