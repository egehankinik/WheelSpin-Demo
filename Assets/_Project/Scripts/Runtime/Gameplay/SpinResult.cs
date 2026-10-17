using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public readonly struct SpinResult
    {
        public SpinResult(int sliceIndex, WheelSlice slice, int amount)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
            Amount = amount;
        }

        public int SliceIndex { get; }
        public WheelSlice Slice { get; }
        public int Amount { get; }
        public bool IsBomb => Slice.IsBomb;
        public RewardDefinition Reward => Slice.Reward;
    }
}
