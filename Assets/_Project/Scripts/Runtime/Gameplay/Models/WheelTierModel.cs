using System;

namespace WheelOfFortune.Gameplay
{
    public readonly struct WheelTierModel
    {
        public WheelTierModel(int fromZone, WheelModel wheel)
        {
            if (fromZone < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(fromZone));
            }

            FromZone = fromZone;
            Wheel = wheel ?? throw new ArgumentNullException(nameof(wheel));
        }

        public int FromZone { get; }
        public WheelModel Wheel { get; }
    }
}
