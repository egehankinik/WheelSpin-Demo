using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_settings", menuName = "WheelOfFortune/Zone Settings")]
    public sealed class ZoneSettings : ScriptableObject
    {
        #region Fields

        [SerializeField] private List<ZoneTypeDefinition> _zoneTypes = new List<ZoneTypeDefinition>();
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.1f;

        #endregion

        #region Properties

        public IReadOnlyList<ZoneTypeDefinition> ZoneTypes
        {
            get { return _zoneTypes; }
        }

        public float RewardGrowthPerZone
        {
            get { return _rewardGrowthPerZone; }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            var defaultCount = 0;
            foreach (var zoneType in _zoneTypes)
            {
                if (!zoneType)
                {
                    Debug.LogError($"{name}: zone type list has an empty entry.", this);
                }
                else if (zoneType.IsDefault)
                {
                    defaultCount++;
                }
            }

            if (defaultCount != 1)
            {
                Debug.LogError($"{name}: exactly one zone type must have interval 0, found {defaultCount}.", this);
            }
        }
#endif

        #endregion
    }
}
