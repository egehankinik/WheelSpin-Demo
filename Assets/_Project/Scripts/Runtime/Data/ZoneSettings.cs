using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_settings", menuName = "WheelOfFortune/Zone Settings")]
    public sealed class ZoneSettings : ScriptableObject
    {
        [SerializeField] private List<ZoneTypeDefinition> _zoneTypes = new List<ZoneTypeDefinition>();
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.1f;

        public IReadOnlyList<ZoneTypeDefinition> ZoneTypes => _zoneTypes;

        public ZoneRules CreateRules() => new ZoneRules(_zoneTypes, _rewardGrowthPerZone);

        private void OnValidate()
        {
            var defaultCount = 0;
            foreach (var zoneType in _zoneTypes)
            {
                if (zoneType == null)
                {
                    Debug.LogWarning($"{name}: zone type list has an empty entry.", this);
                }
                else if (zoneType.IsDefault)
                {
                    defaultCount++;
                }
            }

            if (defaultCount != 1)
            {
                Debug.LogWarning($"{name}: exactly one zone type must have interval 0, found {defaultCount}.", this);
            }
        }
    }
}
