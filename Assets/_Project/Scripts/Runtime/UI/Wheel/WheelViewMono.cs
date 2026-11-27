using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelViewMono : MonoBehaviour
    {
        #region Fields

        private const string SPIN_BUTTON_NAME = "ui_button_spin";
        private const string ANIMATION_ROOT_NAME = "ui_wheel_animation";
        private const string ROTATION_ROOT_NAME = "ui_wheel_rotation";
        private const string BASE_IMAGE_NAME = "ui_image_wheel_base_value";
        private const string INDICATOR_IMAGE_NAME = "ui_image_wheel_indicator_value";
        private const string SHINE_IMAGE_NAME = "ui_image_wheel_shine_value";
        private const string TITLE_TEXT_NAME = "ui_text_wheel_title_value";
        private const string SUBTITLE_TEXT_NAME = "ui_text_wheel_subtitle_value";
        private const float FULL_TURN_DEGREES = 360f;

        [SerializeField] private Button _spinButton;
        [SerializeField] private RectTransform _animationRoot;
        [SerializeField] private RectTransform _rotationRoot;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private WheelIndicatorFlapperMono _indicatorFlapper;
        [SerializeField] private Image _shineImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private WheelSliceViewMono[] _sliceViews;
        [SerializeField] private Sprite _bombIcon;
        [SerializeField, Range(0f, 0.5f)] private float _sliceRadiusRatio = 0.293f;
        [SerializeField, Min(0.1f)] private float _spinDuration = 4f;
        [SerializeField, Min(1)] private int _fullRotations = 5;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;
        [SerializeField, Range(0f, 0.45f)] private float _landingJitter = 0.3f;
        [SerializeField, Min(0f)] private float _bombShakeDuration = 0.5f;
        [SerializeField, Min(0f)] private float _bombShakeStrength = 25f;
        [SerializeField, Min(1)] private int _bombShakeVibrato = 20;
        [SerializeField, Range(0f, 180f)] private float _bombShakeRandomness = 90f;
        [SerializeField] private PunchSettings _zoneChangePunch = new PunchSettings(0.06f, 0.35f);

        private IEventBus _events;
        private float _sliceAngle;
        private int _lastPegIndex;
        private int _targetSliceIndex;
        private Action _onSpinStopped;
        private Action _onBombHitCompleted;
        private Tween _spinTween;
        private Tween _bombTween;
        private Tween _transitionTween;

        #endregion

        #region Properties

        public int SliceCapacity
        {
            get { return _sliceViews.Length; }
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _transitionTween?.Kill(true);

            var isInterrupted = _spinTween.IsActive() || _bombTween.IsActive();
            _spinTween?.Kill();
            _bombTween?.Kill();
            _onSpinStopped = null;
            _onBombHitCompleted = null;
            if (isInterrupted)
            {
                _events?.Publish(new SpinAnimationInterruptedEvent());
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(IEventBus events)
        {
            _events = events;
        }

        public bool CanDisplay(WheelDefinition wheel)
        {
            return wheel.SliceCount > 0 && wheel.SliceCount <= SliceCapacity;
        }

        public void Bind(WheelDefinition wheel, Func<int, int> amountForSlice, bool isAnimated)
        {
            if (!CanDisplay(wheel))
            {
                throw new InvalidOperationException($"{wheel.name} has {wheel.SliceCount} slices but {name} can show 1 to {SliceCapacity}.");
            }

            _baseImage.sprite = wheel.WheelSprite;
            _indicatorImage.sprite = wheel.IndicatorSprite;
            _shineImage.color = wheel.GlowColor;
            _titleText.text = wheel.Title;
            _subtitleText.text = wheel.Subtitle;
            _sliceAngle = wheel.SliceAngle;

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
                if (slice.IsBomb)
                {
                    sliceView.Bind(_bombIcon, string.Empty);
                }
                else
                {
                    sliceView.Bind(slice.Reward.Icon, AmountFormatter.FormatMultiplier(amountForSlice(i)));
                }

                sliceView.Place(radius, i * _sliceAngle);
            }

            if (isAnimated)
            {
                _transitionTween?.Kill(true);
                _transitionTween = _zoneChangePunch.PunchScale(_animationRoot);
            }
        }

        public void Spin(int sliceIndex, Action onStopped)
        {
            _spinTween?.Kill();
            _targetSliceIndex = sliceIndex;
            _onSpinStopped = onStopped;
            _lastPegIndex = GetPegIndex();

            var jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter) * _sliceAngle;
            var targetAngle = sliceIndex * _sliceAngle + jitter - FULL_TURN_DEGREES * _fullRotations;

            _spinTween = _rotationRoot
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .OnUpdate(UpdateIndicatorContact)
                .OnComplete(HandleSpinCompleted)
                .SetLink(gameObject);
        }

        public void PlayBombHit(Action onComplete)
        {
            _transitionTween?.Kill(true);
            _bombTween?.Kill();
            _onBombHitCompleted = onComplete;
            _bombTween = _animationRoot
                .DOShakePosition(_bombShakeDuration, _bombShakeStrength, _bombShakeVibrato, _bombShakeRandomness)
                .OnComplete(HandleBombHitCompleted)
                .SetLink(gameObject);
        }

        public RectTransform GetSliceIconRect(int sliceIndex)
        {
            return _sliceViews[sliceIndex].IconRect;
        }

        public void SetSpinInteractable(bool isInteractable)
        {
            _spinButton.interactable = isInteractable;
        }

        #endregion

        #region Private Methods

        private int GetPegIndex()
        {
            var angle = Mathf.Repeat(_sliceAngle * 0.5f - _rotationRoot.localEulerAngles.z, FULL_TURN_DEGREES);
            return Mathf.FloorToInt(angle / _sliceAngle);
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

        private void HandleSpinCompleted()
        {
            UpdateIndicatorContact();
            _sliceViews[_targetSliceIndex].PlayHighlight();
            var onStopped = _onSpinStopped;
            _onSpinStopped = null;
            onStopped?.Invoke();
        }

        private void HandleBombHitCompleted()
        {
            var onComplete = _onBombHitCompleted;
            _onBombHitCompleted = null;
            onComplete?.Invoke();
        }

        private void HandleSpinClicked()
        {
            _events.Publish(new SpinRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spinButton = UiReferenceFinder.Find<Button>(this, SPIN_BUTTON_NAME);
            _animationRoot = UiReferenceFinder.Find<RectTransform>(this, ANIMATION_ROOT_NAME);
            _rotationRoot = UiReferenceFinder.Find<RectTransform>(this, ROTATION_ROOT_NAME);
            _baseImage = UiReferenceFinder.Find<Image>(this, BASE_IMAGE_NAME);
            _indicatorImage = UiReferenceFinder.Find<Image>(this, INDICATOR_IMAGE_NAME);
            _indicatorFlapper = UiReferenceFinder.Find<WheelIndicatorFlapperMono>(this, INDICATOR_IMAGE_NAME);
            _shineImage = UiReferenceFinder.Find<Image>(this, SHINE_IMAGE_NAME);
            _titleText = UiReferenceFinder.Find<TMP_Text>(this, TITLE_TEXT_NAME);
            _subtitleText = UiReferenceFinder.Find<TMP_Text>(this, SUBTITLE_TEXT_NAME);
            _sliceViews = GetComponentsInChildren<WheelSliceViewMono>(true);
        }
#endif

        #endregion
    }
}
