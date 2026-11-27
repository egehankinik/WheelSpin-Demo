using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core.Events;
using WheelOfFortune.UI.Common;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.UI.Popups
{
    public sealed class BombPopupViewMono : PopupViewMono
    {
        #region Fields

        private const string CONTENT_NAME = "ui_popup_bomb_content";
        private const string GIVE_UP_BUTTON_NAME = "ui_button_bomb_give_up";
        private const string REVIVE_BUTTON_NAME = "ui_button_bomb_revive";
        private const string REVIVE_COST_TEXT_NAME = "ui_text_bomb_revive_cost_value";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;

        private IEventBus _events;

        #endregion

        #region Properties

        protected override string ContentName
        {
            get { return CONTENT_NAME; }
        }

        #endregion

        #region Unity Methods

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

        #endregion

        #region Public Methods

        public void Initialize(IEventBus events)
        {
            _events = events;
        }

        public void Show(int reviveCost, bool isReviveAffordable)
        {
            _reviveCostText.text = AmountFormatter.Format(reviveCost);
            _reviveButton.interactable = isReviveAffordable;
            Open();
        }

        #endregion

        #region Private Methods

        private void HandleGiveUpClicked()
        {
            _events.Publish(new GiveUpRequestedEvent());
        }

        private void HandleReviveClicked()
        {
            _events.Publish(new ReviveRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _giveUpButton = UiReferenceFinder.Find<Button>(this, GIVE_UP_BUTTON_NAME);
            _reviveButton = UiReferenceFinder.Find<Button>(this, REVIVE_BUTTON_NAME);
            _reviveCostText = UiReferenceFinder.Find<TMP_Text>(this, REVIVE_COST_TEXT_NAME);
        }
#endif

        #endregion
    }
}
