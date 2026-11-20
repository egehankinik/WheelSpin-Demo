using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public struct WheelTier
    {
        [SerializeField, Min(1)] private int _fromZone;
        [SerializeField] private WheelDefinition _wheel;

        public WheelTier(int fromZone, WheelDefinition wheel)
        {
            _fromZone = fromZone;
            _wheel = wheel;
        }

        public int FromZone => _fromZone;
        public WheelDefinition Wheel => _wheel;
    }
}
