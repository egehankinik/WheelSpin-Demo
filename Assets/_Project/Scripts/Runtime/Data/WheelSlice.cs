using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public struct WheelSlice
    {
        [SerializeField] private RewardDefinition _reward;
        [SerializeField, Min(1)] private int _baseAmount;
        [SerializeField] private bool _isBomb;

        public WheelSlice(RewardDefinition reward, int baseAmount, bool isBomb = false)
        {
            _reward = reward;
            _baseAmount = baseAmount;
            _isBomb = isBomb;
        }

        public static WheelSlice Bomb() => new WheelSlice(null, 0, true);

        public RewardDefinition Reward => _reward;
        public int BaseAmount => _baseAmount;
        public bool IsBomb => _isBomb;
    }
}
