using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Popups
{
    public sealed class BombPopupView : PopupView
    {
        private const string GiveUpButtonName = "ui_button_bomb_give_up";
        private const string ReviveButtonName = "ui_button_bomb_revive";
        private const string ReviveCostTextName = "ui_text_bomb_revive_cost_value";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;

        public event Action GiveUpClicked;
        public event Action ReviveClicked;

        protected override string ContentName => "ui_popup_bomb_content";

        private void OnEnable()
        {
            _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            _reviveButton.onClick.AddListener(HandleReviveClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            _reviveButton.onClick.RemoveListener(HandleReviveClicked);
        }

        public void Show(int reviveCost, bool canRevive)
        {
            _reviveCostText.text = AmountFormatter.Format(reviveCost);
            _reviveButton.interactable = canRevive;
            Open();
        }

        private void HandleGiveUpClicked() => GiveUpClicked?.Invoke();

        private void HandleReviveClicked() => ReviveClicked?.Invoke();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _giveUpButton = UiReferenceFinder.Find<Button>(this, GiveUpButtonName);
            _reviveButton = UiReferenceFinder.Find<Button>(this, ReviveButtonName);
            _reviveCostText = UiReferenceFinder.Find<TMP_Text>(this, ReviveCostTextName);
        }
#endif
    }
}
