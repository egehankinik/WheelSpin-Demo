using System;

namespace WheelOfFortune.Gameplay
{
    public sealed class ZoneRules
    {
        private readonly int _safeZoneInterval;
        private readonly int _superZoneInterval;
        private readonly float _rewardGrowthPerZone;
        private readonly bool _allowLeavingInNormalZones;

        public ZoneRules(int safeZoneInterval, int superZoneInterval, float rewardGrowthPerZone, bool allowLeavingInNormalZones = false)
        {
            if (safeZoneInterval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(safeZoneInterval));
            }

            if (superZoneInterval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(superZoneInterval));
            }

            if (rewardGrowthPerZone < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardGrowthPerZone));
            }

            _safeZoneInterval = safeZoneInterval;
            _superZoneInterval = superZoneInterval;
            _rewardGrowthPerZone = rewardGrowthPerZone;
            _allowLeavingInNormalZones = allowLeavingInNormalZones;
        }

        public bool CanLeaveFrom(ZoneType zoneType)
        {
            return _allowLeavingInNormalZones || zoneType != ZoneType.Normal;
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(zone));
            }

            if (zone % _superZoneInterval == 0)
            {
                return ZoneType.Super;
            }

            if (zone % _safeZoneInterval == 0)
            {
                return ZoneType.Safe;
            }

            return ZoneType.Normal;
        }

        public int GetNextZoneOfType(int fromZone, ZoneType zoneType)
        {
            var searchLimit = fromZone + _safeZoneInterval * _superZoneInterval;
            for (var zone = Math.Max(1, fromZone); zone <= searchLimit; zone++)
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
    }
}
