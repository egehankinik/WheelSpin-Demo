using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelSliceViewMono : MonoBehaviour
    {
        #region Fields

        private const string ICON_IMAGE_NAME = "ui_image_wheel_slice_icon_value";
        private const string AMOUNT_TEXT_NAME = "ui_text_wheel_slice_amount_value";

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private PunchSettings _highlightPunch = new PunchSettings(0.35f, 0.4f);

        private Tween _highlightTween;

        #endregion

        #region Properties

        public RectTransform IconRect
        {
            get { return _iconImage.rectTransform; }
        }

        #endregion

        #region Unity Methods

        private void OnDisable()
        {
            _highlightTween?.Kill(true);
        }

        #endregion

        #region Public Methods

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

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void PlayHighlight()
        {
            _highlightTween?.Kill(true);
            _highlightTween = _highlightPunch.PunchScale(IconRect);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectTransform = (RectTransform)transform;
            _iconImage = UiReferenceFinder.Find<Image>(this, ICON_IMAGE_NAME);
            _amountText = UiReferenceFinder.Find<TMP_Text>(this, AMOUNT_TEXT_NAME);
        }
#endif

        #endregion
    }
}
