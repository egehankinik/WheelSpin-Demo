using UnityEngine;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelIndicatorFlapperMono : MonoBehaviour
    {
        #region Fields

        private const float REST_THRESHOLD = 0.05f;
        private const float MAX_STEP = 1f / 30f;

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
        private bool _isInContact;

        #endregion

        #region Unity Methods

        private void Update()
        {
            var deltaTime = Mathf.Min(Time.deltaTime, MAX_STEP);
            var acceleration = -_stiffness * _angle - _damping * _angularVelocity;
            _angularVelocity += acceleration * deltaTime;
            _angle += _angularVelocity * deltaTime;

            if (_isInContact)
            {
                PushTo(_pushAngle);
            }

            _angle = Mathf.Clamp(_angle, -_maxDeflection * _overshootLimit, _maxDeflection);

            var isAtRest = Mathf.Abs(_angle) < REST_THRESHOLD && Mathf.Abs(_angularVelocity) < REST_THRESHOLD;
            if (!_isInContact && isAtRest)
            {
                _angle = 0f;
                _angularVelocity = 0f;
                Apply();
                enabled = false;
                return;
            }

            Apply();
        }

        #endregion

        #region Public Methods

        public void SetPegOffset(float pegOffset, bool hasPegPassedTip)
        {
            _isInContact = pegOffset >= -_contactStartAngle && pegOffset <= _releaseAngle;
            _pushAngle = 0f;
            if (_isInContact)
            {
                _pushAngle = _maxDeflection * Mathf.InverseLerp(-_contactStartAngle, _releaseAngle, pegOffset);
            }

            if (hasPegPassedTip && !_isInContact)
            {
                PushTo(_maxDeflection);
            }
            else if (_isInContact)
            {
                PushTo(_pushAngle);
            }

            enabled = true;
            Apply();
        }

        #endregion

        #region Private Methods

        private void PushTo(float angle)
        {
            if (_angle >= angle)
            {
                return;
            }

            _angle = angle;
            _angularVelocity = 0f;
        }

        private void Apply()
        {
            _pivot.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _pivot = (RectTransform)transform;
        }
#endif

        #endregion
    }
}
