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

        public RewardDefinition Reward
        {
            get { return _reward; }
        }

        public int BaseAmount
        {
            get { return _baseAmount; }
        }

        public bool IsBomb
        {
            get { return _isBomb; }
        }
    }
}
