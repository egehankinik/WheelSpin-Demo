using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        private const string IconImageName = "ui_image_wheel_slice_icon_value";
        private const string AmountTextName = "ui_text_wheel_slice_amount_value";

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private PunchSettings _highlightPunch = new PunchSettings(0.35f, 0.4f);

        private Tween _highlightTween;

        public RectTransform IconRect => _iconImage.rectTransform;

        public void Bind(Sprite icon, string amountLabel)
        {
            _iconImage.sprite = icon;
            _amountText.text = amountLabel;
        }

        public void Place(float radius, float angle)
        {
            var radians = angle * Mathf.Deg2Rad;
            _rectTransform.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void PlayHighlight()
        {
            _highlightTween?.Kill(true);
            _highlightTween = _highlightPunch.PunchScale(IconRect);
        }

        private void OnDisable()
        {
            _highlightTween?.Kill(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectTransform = (RectTransform)transform;
            _iconImage = UiReferenceFinder.Find<Image>(this, IconImageName);
            _amountText = UiReferenceFinder.Find<TMP_Text>(this, AmountTextName);
        }
#endif
    }
}
