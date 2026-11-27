using System;
using System.Collections.Generic;

namespace WheelOfFortune.Gameplay
{
    public sealed class ZoneTypeModel
    {
        #region Fields

        private readonly List<WheelTierModel> _wheelTiers;

        #endregion

        public ZoneTypeModel(int interval, bool isSafe, IEnumerable<WheelTierModel> wheelTiers)
        {
            if (interval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            if (wheelTiers == null)
            {
                throw new ArgumentNullException(nameof(wheelTiers));
            }

            Interval = interval;
            IsSafe = isSafe;
            _wheelTiers = new List<WheelTierModel>(wheelTiers);
        }

        #region Properties

        public int Interval { get; }
        public bool IsSafe { get; }

        public bool IsDefault
        {
            get { return Interval == 0; }
        }

        #endregion

        #region Public Methods

        public bool Matches(int zone)
        {
            return !IsDefault && zone % Interval == 0;
        }

        public WheelModel GetWheel(int zone)
        {
            WheelModel wheel = null;
            var bestFromZone = int.MinValue;
            foreach (var tier in _wheelTiers)
            {
                if (tier.FromZone <= zone && tier.FromZone > bestFromZone)
                {
                    bestFromZone = tier.FromZone;
                    wheel = tier.Wheel;
                }
            }

            return wheel;
        }

        #endregion
    }
}
