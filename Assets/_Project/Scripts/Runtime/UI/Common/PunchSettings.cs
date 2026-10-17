using System;
using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.UI.Common
{
    [Serializable]
    public struct PunchSettings
    {
        [SerializeField] private float _strength;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Min(0)] private int _vibrato;
        [SerializeField, Range(0f, 1f)] private float _elasticity;

        public PunchSettings(float strength, float duration, int vibrato = 6, float elasticity = 0.5f)
        {
            _strength = strength;
            _duration = duration;
            _vibrato = vibrato;
            _elasticity = elasticity;
        }

        public Tween PunchScale(Transform target)
        {
            return target.DOPunchScale(Vector3.one * _strength, _duration, _vibrato, _elasticity);
        }
    }
}
