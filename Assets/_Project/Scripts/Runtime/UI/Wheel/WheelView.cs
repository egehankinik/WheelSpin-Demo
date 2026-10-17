using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelView : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_spin";
        private const string AnimationRootName = "ui_wheel_animation";
        private const string RotationRootName = "ui_wheel_rotation";
        private const string BaseImageName = "ui_image_wheel_base_value";
        private const string IndicatorImageName = "ui_image_wheel_indicator_value";
        private const string ShineImageName = "ui_image_wheel_shine_value";
        private const string TitleTextName = "ui_text_wheel_title_value";
        private const string SubtitleTextName = "ui_text_wheel_subtitle_value";

        [SerializeField] private Button _spinButton;
        [SerializeField] private RectTransform _animationRoot;
        [SerializeField] private RectTransform _rotationRoot;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private WheelIndicatorFlapper _indicatorFlapper;
        [SerializeField] private Image _shineImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private WheelSliceView[] _sliceViews;
        [SerializeField] private Sprite _bombIcon;
        [SerializeField, Range(0f, 0.5f)] private float _sliceRadiusRatio = 0.293f;
        [SerializeField, Min(0.1f)] private float _spinDuration = 4f;
        [SerializeField, Min(1)] private int _fullRotations = 5;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;
        [SerializeField, Range(0f, 0.45f)] private float _landingJitter = 0.3f;
        [SerializeField, Min(0f)] private float _bombShakeDuration = 0.5f;
        [SerializeField, Min(0f)] private float _bombShakeStrength = 25f;
        [SerializeField, Min(1)] private int _bombShakeVibrato = 20;
        [SerializeField] private PunchSettings _zoneChangePunch = new PunchSettings(0.06f, 0.35f);

        private float _sliceAngle;
        private int _lastPegIndex;
        private Tween _spinTween;
        private Tween _transitionTween;

        public event Action SpinClicked;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _spinTween?.Kill();
            _transitionTween?.Kill(true);
        }

        public void Bind(WheelDefinition wheel, Func<int, int> amountForSlice, bool animate)
        {
            _baseImage.sprite = wheel.WheelSprite;
            _indicatorImage.sprite = wheel.IndicatorSprite;
            _shineImage.color = wheel.GlowColor;
            _titleText.text = wheel.Title;
            _subtitleText.text = wheel.Subtitle;
            _sliceAngle = wheel.SliceAngle;

            if (wheel.SliceCount > _sliceViews.Length)
            {
                Debug.LogWarning($"{wheel.name} has {wheel.SliceCount} slices but the view has {_sliceViews.Length}.", this);
            }

            var radius = _rotationRoot.rect.width * _sliceRadiusRatio;
            for (var i = 0; i < _sliceViews.Length; i++)
            {
                var sliceView = _sliceViews[i];
                var hasSlice = i < wheel.SliceCount;
                sliceView.SetVisible(hasSlice);
                if (!hasSlice)
                {
                    continue;
                }

                var slice = wheel.Slices[i];
                var icon = slice.IsBomb ? _bombIcon : slice.Reward.Icon;
                var label = slice.IsBomb ? string.Empty : AmountFormatter.FormatMultiplier(amountForSlice(i));
                sliceView.Bind(icon, label);
                sliceView.Place(radius, i * _sliceAngle);
            }

            if (animate)
            {
                _transitionTween?.Kill(true);
                _transitionTween = _zoneChangePunch.PunchScale(_animationRoot);
            }
        }

        public void Spin(int sliceIndex, Action onStopped)
        {
            _spinTween?.Kill();
            _lastPegIndex = GetPegIndex();

            var jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter) * _sliceAngle;
            var targetAngle = sliceIndex * _sliceAngle + jitter - 360f * _fullRotations;

            _spinTween = _rotationRoot
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .OnUpdate(UpdateIndicatorContact)
                .OnComplete(() =>
                {
                    UpdateIndicatorContact();
                    _sliceViews[sliceIndex].PlayHighlight();
                    onStopped?.Invoke();
                });
        }

        public void PlayBombHit(Action onComplete)
        {
            _transitionTween?.Kill(true);
            _transitionTween = _animationRoot
                .DOShakePosition(_bombShakeDuration, _bombShakeStrength, _bombShakeVibrato)
                .OnComplete(() => onComplete?.Invoke());
        }

        public RectTransform GetSliceIconRect(int sliceIndex) => _sliceViews[sliceIndex].IconRect;

        public void SetSpinInteractable(bool interactable) => _spinButton.interactable = interactable;

        private int GetPegIndex()
        {
            return Mathf.FloorToInt(Mathf.Repeat(_sliceAngle * 0.5f - _rotationRoot.localEulerAngles.z, 360f) / _sliceAngle);
        }

        private float GetPegOffset()
        {
            return Mathf.Repeat(_sliceAngle - _rotationRoot.localEulerAngles.z, _sliceAngle) - _sliceAngle * 0.5f;
        }

        private void UpdateIndicatorContact()
        {
            var pegIndex = GetPegIndex();
            _indicatorFlapper.SetPegOffset(GetPegOffset(), pegIndex != _lastPegIndex);
            _lastPegIndex = pegIndex;
        }

        private void HandleSpinClicked() => SpinClicked?.Invoke();

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spinButton = UiReferenceFinder.Find<Button>(this, SpinButtonName);
            _animationRoot = UiReferenceFinder.Find<RectTransform>(this, AnimationRootName);
            _rotationRoot = UiReferenceFinder.Find<RectTransform>(this, RotationRootName);
            _baseImage = UiReferenceFinder.Find<Image>(this, BaseImageName);
            _indicatorImage = UiReferenceFinder.Find<Image>(this, IndicatorImageName);
            _indicatorFlapper = UiReferenceFinder.Find<WheelIndicatorFlapper>(this, IndicatorImageName);
            _shineImage = UiReferenceFinder.Find<Image>(this, ShineImageName);
            _titleText = UiReferenceFinder.Find<TMP_Text>(this, TitleTextName);
            _subtitleText = UiReferenceFinder.Find<TMP_Text>(this, SubtitleTextName);
            _sliceViews = GetComponentsInChildren<WheelSliceView>(true);
        }
#endif
    }
}
