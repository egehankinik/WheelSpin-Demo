using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public interface IWheelProvider
    {
        WheelDefinition GetWheel(ZoneType zoneType);
    }
}
