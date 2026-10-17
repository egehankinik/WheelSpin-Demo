using UnityEngine;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelIndicatorFlapper : MonoBehaviour
    {
        private const float RestThreshold = 0.05f;
        private const float MaxStep = 1f / 30f;

        [SerializeField] private RectTransform _pivot;
        [SerializeField, Range(0f, 60f)] private float _maxDeflection = 30f;
        [SerializeField, Range(0f, 45f)] private float _contactStartAngle = 12f;
        [SerializeField, Range(0f, 45f)] private float _releaseAngle = 3f;
        [SerializeField, Min(0f)] private float _stiffness = 900f;
        [SerializeField, Min(0f)] private float _damping = 16f;
        [SerializeField, Range(0f, 1f)] private float _overshootLimit = 0.35f;

        private float _angle;
        private float _angularVelocity;
        private float _pushAngle;
        private bool _inContact;

        public void SetPegOffset(float pegOffset, bool pegPassedTip)
        {
            _inContact = pegOffset >= -_contactStartAngle && pegOffset <= _releaseAngle;
            _pushAngle = _inContact ? _maxDeflection * Mathf.InverseLerp(-_contactStartAngle, _releaseAngle, pegOffset) : 0f;

            if (pegPassedTip && !_inContact)
            {
                PushTo(_maxDeflection);
            }
            else if (_inContact)
            {
                PushTo(_pushAngle);
            }

            enabled = true;
            Apply();
        }

        private void PushTo(float angle)
        {
            if (_angle >= angle)
            {
                return;
            }

            _angle = angle;
            _angularVelocity = 0f;
        }

        private void Update()
        {
            var deltaTime = Mathf.Min(Time.deltaTime, MaxStep);
            var acceleration = -_stiffness * _angle - _damping * _angularVelocity;
            _angularVelocity += acceleration * deltaTime;
            _angle += _angularVelocity * deltaTime;

            if (_inContact)
            {
                PushTo(_pushAngle);
            }

            _angle = Mathf.Clamp(_angle, -_maxDeflection * _overshootLimit, _maxDeflection);

            if (!_inContact && Mathf.Abs(_angle) < RestThreshold && Mathf.Abs(_angularVelocity) < RestThreshold)
            {
                _angle = 0f;
                _angularVelocity = 0f;
                Apply();
                enabled = false;
                return;
            }

            Apply();
        }

        private void Apply()
        {
            _pivot.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _pivot = (RectTransform)transform;
        }
#endif
    }
}
