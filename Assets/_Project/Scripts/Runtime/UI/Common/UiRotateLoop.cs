using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.UI.Common
{
    public sealed class UiRotateLoop : MonoBehaviour
    {
        [SerializeField] private float _degreesPerSecond = -20f;

        private Tween _tween;

        private void OnEnable()
        {
            if (Mathf.Approximately(_degreesPerSecond, 0f))
            {
                return;
            }

            var duration = 360f / Mathf.Abs(_degreesPerSecond);
            var fullTurn = new Vector3(0f, 0f, 360f * Mathf.Sign(_degreesPerSecond));
            _tween = transform
                .DOLocalRotate(fullTurn, duration, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }
    }
}
