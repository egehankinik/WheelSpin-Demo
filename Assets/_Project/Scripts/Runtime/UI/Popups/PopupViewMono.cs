using DG.Tweening;
using UnityEngine;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Popups
{
    public abstract class PopupViewMono : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RectTransform _content;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField, Min(0f)] private float _duration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _closeDurationRatio = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _hiddenScale = 0.8f;
        [SerializeField] private Ease _openScaleEase = Ease.OutBack;
        [SerializeField] private Ease _closeScaleEase = Ease.InBack;
        [SerializeField] private Ease _fadeEase = Ease.OutQuad;

        private Sequence _sequence;

        #endregion

        #region Properties

        protected abstract string ContentName { get; }

        #endregion

        #region Unity Methods

        protected virtual void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion

        #region Public Methods

        public void Close()
        {
            _sequence?.Kill();
            _contentGroup.interactable = false;
            var closeDuration = _duration * _closeDurationRatio;
            _sequence = DOTween.Sequence()
                .Join(FadeTo(0f, closeDuration))
                .Join(_content.DOScale(_hiddenScale, closeDuration).SetEase(_closeScaleEase))
                .OnComplete(HandleCloseCompleted)
                .SetLink(gameObject);
        }

        #endregion

        #region Protected Methods

        protected void Open()
        {
            _sequence?.Kill();
            gameObject.SetActive(true);
            _contentGroup.alpha = 0f;
            _contentGroup.interactable = true;
            _content.localScale = Vector3.one * _hiddenScale;
            _sequence = DOTween.Sequence()
                .Join(FadeTo(1f, _duration))
                .Join(_content.DOScale(1f, _duration).SetEase(_openScaleEase))
                .SetLink(gameObject);
        }

        #endregion

        #region Private Methods

        private Tween FadeTo(float alpha, float duration)
        {
            return DOTween
                .To(GetContentAlpha, SetContentAlpha, alpha, duration)
                .SetEase(_fadeEase);
        }

        private float GetContentAlpha()
        {
            return _contentGroup.alpha;
        }

        private void SetContentAlpha(float alpha)
        {
            _contentGroup.alpha = alpha;
        }

        private void HandleCloseCompleted()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _content = UiReferenceFinder.Find<RectTransform>(this, ContentName);
            _contentGroup = UiReferenceFinder.Find<CanvasGroup>(this, ContentName);
        }
#endif

        #endregion
    }
}
