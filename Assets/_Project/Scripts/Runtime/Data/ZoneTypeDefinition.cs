using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_type_", menuName = "WheelOfFortune/Zone Type Definition")]
    public sealed class ZoneTypeDefinition : ScriptableObject
    {
        [SerializeField, Min(0), Tooltip("Every Nth zone is this type. 0 marks the default type used when no other type matches.")]
        private int _interval;
        [SerializeField, Tooltip("Safe zones have no bomb and let the player leave with the rewards.")]
        private bool _isSafe;
        [SerializeField] private List<WheelTier> _wheelTiers = new List<WheelTier>();

        public int Interval => _interval;
        public bool IsSafe => _isSafe;
        public bool IsDefault => _interval == 0;
        public IReadOnlyList<WheelTier> WheelTiers => _wheelTiers;

        public bool Matches(int zone) => !IsDefault && zone % _interval == 0;

        public WheelDefinition GetWheel(int zone)
        {
            WheelDefinition wheel = null;
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

        internal void Initialize(int interval, bool isSafe, IEnumerable<WheelTier> wheelTiers)
        {
            _interval = interval;
            _isSafe = isSafe;
            _wheelTiers = new List<WheelTier>(wheelTiers);
        }

        private void OnValidate()
        {
            if (_wheelTiers.Count == 0)
            {
                Debug.LogWarning($"{name}: zone type has no wheel tiers.", this);
                return;
            }

            if (GetWheel(1) == null)
            {
                Debug.LogWarning($"{name}: no wheel tier starts at zone 1.", this);
            }

            foreach (var tier in _wheelTiers)
            {
                if (tier.Wheel == null)
                {
                    Debug.LogWarning($"{name}: wheel tier from zone {tier.FromZone} has no wheel.", this);
                }
                else if (_isSafe && tier.Wheel.HasBomb)
                {
                    Debug.LogWarning($"{name}: safe zone wheel {tier.Wheel.name} must not contain a bomb.", this);
                }
                else if (!_isSafe && !tier.Wheel.HasBomb)
                {
                    Debug.LogWarning($"{name}: risky zone wheel {tier.Wheel.name} has no bomb.", this);
                }
            }
        }
    }
}
