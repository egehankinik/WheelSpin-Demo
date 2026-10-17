using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Popups
{
    public sealed class CollectedPopupView : PopupView
    {
        private const string RestartButtonName = "ui_button_collected_restart";
        private const string BalanceTextName = "ui_text_collected_balance_value";

        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _balanceText;

        public event Action RestartClicked;

        protected override string ContentName => "ui_popup_collected_content";

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        public void Show(int balance)
        {
            _balanceText.text = AmountFormatter.Format(balance);
            Open();
        }

        private void HandleRestartClicked() => RestartClicked?.Invoke();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _restartButton = UiReferenceFinder.Find<Button>(this, RestartButtonName);
            _balanceText = UiReferenceFinder.Find<TMP_Text>(this, BalanceTextName);
        }
#endif
    }
}
