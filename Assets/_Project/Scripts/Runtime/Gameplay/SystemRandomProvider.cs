using System;

namespace WheelOfFortune.Gameplay
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider()
        {
            _random = new Random();
        }

        public SystemRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
