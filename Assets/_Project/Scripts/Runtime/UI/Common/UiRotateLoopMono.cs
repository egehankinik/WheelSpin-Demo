using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.UI.Common
{
    public sealed class UiRotateLoopMono : MonoBehaviour
    {
        #region Fields

        private const float FULL_TURN_DEGREES = 360f;

        [SerializeField] private float _degreesPerSecond = -20f;
        [SerializeField] private Ease _ease = Ease.Linear;

        private Tween _tween;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (Mathf.Approximately(_degreesPerSecond, 0f))
            {
                return;
            }

            var duration = FULL_TURN_DEGREES / Mathf.Abs(_degreesPerSecond);
            var fullTurn = new Vector3(0f, 0f, FULL_TURN_DEGREES * Mathf.Sign(_degreesPerSecond));
            _tween = transform
                .DOLocalRotate(fullTurn, duration, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Incremental)
                .SetLink(gameObject);
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }

        #endregion
    }
}
