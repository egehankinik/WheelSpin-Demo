using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardRowViewMono : MonoBehaviour
    {
        #region Fields

        private const string ANIMATION_ROOT_NAME = "ui_reward_row_animation";
        private const string ICON_IMAGE_NAME = "ui_image_reward_row_icon_value";
        private const string AMOUNT_TEXT_NAME = "ui_text_reward_row_amount_value";

        [SerializeField] private RectTransform _animationRoot;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Ease _countEase = Ease.OutCubic;
        [SerializeField] private PunchSettings _amountPunch = new PunchSettings(0.2f, 0.3f);

        private int _displayedAmount;
        private Tween _countTween;
        private Tween _punchTween;

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
            KillTweens();
        }

        #endregion

        #region Public Methods

        public void Bind(Sprite icon, int amount)
        {
            KillTweens();
            _iconImage.sprite = icon;
            SetDisplayedAmount(amount);
        }

        public void AnimateTo(int amount, float duration)
        {
            _countTween?.Kill();
            _countTween = DOTween
                .To(GetDisplayedAmount, SetDisplayedAmount, amount, duration)
                .SetEase(_countEase)
                .SetLink(gameObject);

            _punchTween?.Kill(true);
            _punchTween = _amountPunch.PunchScale(_animationRoot);
        }

        #endregion

        #region Private Methods

        private int GetDisplayedAmount()
        {
            return _displayedAmount;
        }

        private void SetDisplayedAmount(int amount)
        {
            _displayedAmount = amount;
            _amountText.text = AmountFormatter.Format(amount);
        }

        private void KillTweens()
        {
            _countTween?.Kill();
            _punchTween?.Kill(true);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _animationRoot = UiReferenceFinder.Find<RectTransform>(this, ANIMATION_ROOT_NAME);
            _iconImage = UiReferenceFinder.Find<Image>(this, ICON_IMAGE_NAME);
            _amountText = UiReferenceFinder.Find<TMP_Text>(this, AMOUNT_TEXT_NAME);
        }
#endif

        #endregion
    }
}
