using DG.Tweening;
using TMPro;
using UnityEngine;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneCellView : MonoBehaviour
    {
        private const string NumberTextName = "ui_text_zone_number_value";

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _numberText;

        private int _zone;
        private Tween _colorTween;

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
                _numberText.color = color;
                return;
            }

            _colorTween = DOTween.To(() => _numberText.color, value => _numberText.color = value, color, duration);
        }

        public void Hide()
        {
            _colorTween?.Kill();
            _numberText.enabled = false;
        }

        private void OnDisable()
        {
            _colorTween?.Kill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectTransform = (RectTransform)transform;
            _numberText = UiReferenceFinder.Find<TMP_Text>(this, NumberTextName);
        }
#endif
    }
}
