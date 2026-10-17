using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardRowView : MonoBehaviour
    {
        private const string AnimationRootName = "ui_reward_row_animation";
        private const string IconImageName = "ui_image_reward_row_icon_value";
        private const string AmountTextName = "ui_text_reward_row_amount_value";

        [SerializeField] private RectTransform _animationRoot;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private PunchSettings _amountPunch = new PunchSettings(0.2f, 0.3f);

        private int _displayedAmount;
        private Tween _countTween;
        private Tween _punchTween;

        public RectTransform IconRect => _iconImage.rectTransform;

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
                .To(() => _displayedAmount, SetDisplayedAmount, amount, duration)
                .SetEase(Ease.OutCubic);

            _punchTween?.Kill(true);
            _punchTween = _amountPunch.PunchScale(_animationRoot);
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

        private void OnDisable()
        {
            KillTweens();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _animationRoot = UiReferenceFinder.Find<RectTransform>(this, AnimationRootName);
            _iconImage = UiReferenceFinder.Find<Image>(this, IconImageName);
            _amountText = UiReferenceFinder.Find<TMP_Text>(this, AmountTextName);
        }
#endif
    }
}
