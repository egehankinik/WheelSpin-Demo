using DG.Tweening;
using TMPro;
using UnityEngine;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneCellViewMono : MonoBehaviour
    {
        #region Fields

        private const string NUMBER_TEXT_NAME = "ui_text_zone_number_value";

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _numberText;
        [SerializeField] private Ease _colorEase = Ease.OutQuad;

        private int _zone;
        private Tween _colorTween;

        #endregion

        #region Unity Methods

        private void OnDisable()
        {
            _colorTween?.Kill();
        }

        #endregion

        #region Public Methods

        public void SetPosition(float x)
        {
            _rectTransform.anchoredPosition = new Vector2(x, 0f);
        }

        public void Show(int zone, Color color, float duration)
        {
            _colorTween?.Kill();
            if (zone != _zone || !_numberText.enabled)
            {
                _zone = zone;
                _numberText.text = zone.ToString();
                _numberText.enabled = true;
            }

            if (duration <= 0f)
            {
                SetNumberColor(color);
                return;
            }

            _colorTween = DOTween
                .To(GetNumberColor, SetNumberColor, color, duration)
                .SetEase(_colorEase)
                .SetLink(gameObject);
        }

        public void Hide()
        {
            _colorTween?.Kill();
            _numberText.enabled = false;
        }

        #endregion

        #region Private Methods

        private Color GetNumberColor()
        {
            return _numberText.color;
        }

        private void SetNumberColor(Color color)
        {
            _numberText.color = color;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectTransform = (RectTransform)transform;
            _numberText = UiReferenceFinder.Find<TMP_Text>(this, NUMBER_TEXT_NAME);
        }
#endif

        #endregion
    }
}
