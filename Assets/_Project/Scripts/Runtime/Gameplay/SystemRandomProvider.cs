using System;

namespace WheelOfFortune.Gameplay
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        #region Fields

        private readonly Random _random;

        #endregion

        public SystemRandomProvider()
        {
            _random = new Random();
        }

        public SystemRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        #region Public Methods

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        #endregion
    }
}
