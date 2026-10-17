using DG.Tweening;
using UnityEngine;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Popups
{
    public abstract class PopupView : MonoBehaviour
    {
        private const float HiddenScale = 0.8f;
        private const float CloseDurationRatio = 0.7f;

        [SerializeField] private RectTransform _content;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField, Min(0f)] private float _duration = 0.3f;

        private Sequence _sequence;

        protected abstract string ContentName { get; }

        public void Close()
        {
            _sequence?.Kill();
            _contentGroup.interactable = false;
            _sequence = DOTween.Sequence()
                .Join(FadeTo(0f, _duration * CloseDurationRatio))
                .Join(_content.DOScale(HiddenScale, _duration * CloseDurationRatio).SetEase(Ease.InBack))
                .OnComplete(() => gameObject.SetActive(false));
        }

        protected void Open()
        {
            _sequence?.Kill();
            gameObject.SetActive(true);
            _contentGroup.alpha = 0f;
            _contentGroup.interactable = true;
            _content.localScale = Vector3.one * HiddenScale;
            _sequence = DOTween.Sequence()
                .Join(FadeTo(1f, _duration))
                .Join(_content.DOScale(1f, _duration).SetEase(Ease.OutBack));
        }

        private Tween FadeTo(float alpha, float duration)
        {
            return DOTween.To(() => _contentGroup.alpha, value => _contentGroup.alpha = value, alpha, duration);
        }

        protected virtual void OnDisable()
        {
            _sequence?.Kill();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _content = UiReferenceFinder.Find<RectTransform>(this, ContentName);
            _contentGroup = UiReferenceFinder.Find<CanvasGroup>(this, ContentName);
        }
#endif
    }
}
