using System;
using System.Collections.Generic;

namespace WheelOfFortune.Gameplay
{
    public sealed class ZoneRules
    {
        #region Fields

        private readonly List<ZoneTypeModel> _zoneTypes;
        private readonly ZoneTypeModel _defaultZoneType;
        private readonly float _rewardGrowthPerZone;
        private readonly int _searchRange;

        #endregion

        public ZoneRules(IEnumerable<ZoneTypeModel> zoneTypes, float rewardGrowthPerZone)
        {
            if (zoneTypes == null)
            {
                throw new ArgumentNullException(nameof(zoneTypes));
            }

            if (rewardGrowthPerZone < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardGrowthPerZone));
            }

            _zoneTypes = new List<ZoneTypeModel>();
            _searchRange = 1;
            foreach (var zoneType in zoneTypes)
            {
                if (zoneType == null)
                {
                    continue;
                }

                if (zoneType.IsDefault)
                {
                    if (_defaultZoneType != null)
                    {
                        throw new ArgumentException("Only one zone type can be the default.", nameof(zoneTypes));
                    }

                    _defaultZoneType = zoneType;
                    continue;
                }

                _zoneTypes.Add(zoneType);
                _searchRange *= zoneType.Interval;
            }

            if (_defaultZoneType == null)
            {
                throw new ArgumentException("A default zone type with interval 0 is required.", nameof(zoneTypes));
            }

            _rewardGrowthPerZone = rewardGrowthPerZone;
        }

        #region Public Methods

        public bool CanLeaveFrom(ZoneTypeModel zoneType)
        {
            return zoneType.IsSafe;
        }

        public ZoneTypeModel GetZoneType(int zone)
        {
            if (zone < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(zone));
            }

            var result = _defaultZoneType;
            foreach (var zoneType in _zoneTypes)
            {
                if (zoneType.Matches(zone) && zoneType.Interval > result.Interval)
                {
                    result = zoneType;
                }
            }

            return result;
        }

        public WheelModel GetWheel(int zone)
        {
            return GetZoneType(zone).GetWheel(zone);
        }

        public int GetNextZoneOfType(int fromZone, ZoneTypeModel zoneType)
        {
            var start = Math.Max(1, fromZone);
            for (var zone = start; zone <= start + _searchRange; zone++)
            {
                if (GetZoneType(zone) == zoneType)
                {
                    return zone;
                }
            }

            return -1;
        }

        public int GetScaledAmount(int baseAmount, int zone)
        {
            var multiplier = 1f + _rewardGrowthPerZone * (zone - 1);
            return (int)Math.Round(baseAmount * multiplier, MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}
