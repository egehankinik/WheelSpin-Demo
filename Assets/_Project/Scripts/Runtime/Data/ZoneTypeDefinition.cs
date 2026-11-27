using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_type_", menuName = "WheelOfFortune/Zone Type Definition")]
    public sealed class ZoneTypeDefinition : ScriptableObject
    {
        #region Fields

        [SerializeField, Min(0), Tooltip("Every Nth zone is this type. 0 marks the default type used when no other type matches.")]
        private int _interval;
        [SerializeField, Tooltip("Safe zones have no bomb and let the player leave with the rewards.")]
        private bool _isSafe;
        [SerializeField] private List<WheelTier> _wheelTiers = new List<WheelTier>();

        #endregion

        #region Properties

        public int Interval
        {
            get { return _interval; }
        }

        public bool IsSafe
        {
            get { return _isSafe; }
        }

        public bool IsDefault
        {
            get { return _interval == 0; }
        }

        public IReadOnlyList<WheelTier> WheelTiers
        {
            get { return _wheelTiers; }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            var hasTierFromZoneOne = false;
            foreach (var tier in _wheelTiers)
            {
                if (tier.FromZone == 1)
                {
                    hasTierFromZoneOne = true;
                }

                if (!tier.Wheel)
                {
                    Debug.LogError($"{name}: wheel tier from zone {tier.FromZone} has no wheel.", this);
                }
                else if (_isSafe && tier.Wheel.HasBomb)
                {
                    Debug.LogError($"{name}: safe zone wheel {tier.Wheel.name} must not contain a bomb.", this);
                }
                else if (!_isSafe && !tier.Wheel.HasBomb)
                {
                    Debug.LogError($"{name}: risky zone wheel {tier.Wheel.name} has no bomb.", this);
                }
            }

            if (!hasTierFromZoneOne)
            {
                Debug.LogError($"{name}: no wheel tier starts at zone 1.", this);
            }
        }
#endif

        #endregion
    }
}
