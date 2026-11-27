namespace WheelOfFortune.Gameplay
{
    public readonly struct SpinResult
    {
        public SpinResult(int sliceIndex, SliceModel slice, int amount)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
            Amount = amount;
        }

        public int SliceIndex { get; }
        public SliceModel Slice { get; }
        public int Amount { get; }

        public bool IsBomb
        {
            get { return Slice.IsBomb; }
        }

        public RewardModel Reward
        {
            get { return Slice.Reward; }
        }
    }
}
