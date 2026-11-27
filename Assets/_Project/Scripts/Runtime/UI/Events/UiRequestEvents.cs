using WheelOfFortune.Core.Events;

namespace WheelOfFortune.UI.Events
{
    public readonly struct SpinRequestedEvent : IGameEvent
    {
    }

    public readonly struct LeaveRequestedEvent : IGameEvent
    {
    }

    public readonly struct ReviveRequestedEvent : IGameEvent
    {
    }

    public readonly struct GiveUpRequestedEvent : IGameEvent
    {
    }

    public readonly struct RestartRequestedEvent : IGameEvent
    {
    }

    public readonly struct SpinAnimationInterruptedEvent : IGameEvent
    {
    }
}
