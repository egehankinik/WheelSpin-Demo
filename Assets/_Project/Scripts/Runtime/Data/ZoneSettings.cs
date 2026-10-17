using UnityEngine;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_settings", menuName = "WheelOfFortune/Zone Settings")]
    public sealed class ZoneSettings : ScriptableObject, IWheelProvider
    {
        [SerializeField, Min(1)] private int _safeZoneInterval = 5;
        [SerializeField, Min(1)] private int _superZoneInterval = 30;
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.1f;
        [SerializeField] private bool _allowLeavingInNormalZones = true;
        [SerializeField] private WheelDefinition _normalWheel;
        [SerializeField] private WheelDefinition _safeWheel;
        [SerializeField] private WheelDefinition _superWheel;

        public ZoneRules CreateRules() => new ZoneRules(_safeZoneInterval, _superZoneInterval, _rewardGrowthPerZone, _allowLeavingInNormalZones);

        public WheelDefinition GetWheel(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Super:
                    return _superWheel;
                case ZoneType.Safe:
                    return _safeWheel;
                default:
                    return _normalWheel;
            }
        }

        private void OnValidate()
        {
            if (_safeWheel != null && _safeWheel.HasBomb)
            {
                Debug.LogWarning($"{name}: safe wheel must not contain a bomb.", this);
            }

            if (_superWheel != null && _superWheel.HasBomb)
            {
                Debug.LogWarning($"{name}: super wheel must not contain a bomb.", this);
            }
        }
    }
}
