using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public struct WheelTier
    {
        [SerializeField, Min(1)] private int _fromZone;
        [SerializeField] private WheelDefinition _wheel;

        public int FromZone
        {
            get { return _fromZone; }
        }

        public WheelDefinition Wheel
        {
            get { return _wheel; }
        }
    }
}
