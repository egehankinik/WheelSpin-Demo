using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core.Events;
using WheelOfFortune.UI.Common;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.UI.Popups
{
    public sealed class CollectedPopupViewMono : PopupViewMono
    {
        #region Fields

        private const string CONTENT_NAME = "ui_popup_collected_content";
        private const string RESTART_BUTTON_NAME = "ui_button_collected_restart";
        private const string BALANCE_TEXT_NAME = "ui_text_collected_balance_value";

        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _balanceText;

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
            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        #endregion

        #region Public Methods

        public void Initialize(IEventBus events)
        {
            _events = events;
        }

        public void Show(int balance)
        {
            _balanceText.text = AmountFormatter.Format(balance);
            Open();
        }

        #endregion

        #region Private Methods

        private void HandleRestartClicked()
        {
            _events.Publish(new RestartRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _restartButton = UiReferenceFinder.Find<Button>(this, RESTART_BUTTON_NAME);
            _balanceText = UiReferenceFinder.Find<TMP_Text>(this, BALANCE_TEXT_NAME);
        }
#endif

        #endregion
    }
}
