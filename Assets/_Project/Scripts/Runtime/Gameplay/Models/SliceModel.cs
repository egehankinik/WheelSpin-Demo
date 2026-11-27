using System;

namespace WheelOfFortune.Gameplay
{
    public readonly struct SliceModel
    {
        public SliceModel(RewardModel reward, int baseAmount)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

            if (baseAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAmount));
            }

            Reward = reward;
            BaseAmount = baseAmount;
            IsBomb = false;
        }

        private SliceModel(bool isBomb)
        {
            Reward = null;
            BaseAmount = 0;
            IsBomb = isBomb;
        }

        public RewardModel Reward { get; }
        public int BaseAmount { get; }
        public bool IsBomb { get; }

        public static SliceModel Bomb()
        {
            return new SliceModel(true);
        }
    }
}
