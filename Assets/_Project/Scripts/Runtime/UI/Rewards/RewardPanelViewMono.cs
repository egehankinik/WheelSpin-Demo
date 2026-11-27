using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardPanelViewMono : MonoBehaviour
    {
        #region Fields

        private const string EXIT_BUTTON_NAME = "ui_button_exit";
        private const string ROW_CONTAINER_NAME = "ui_reward_panel_content";

        private static readonly Vector2 TOP_CENTER = new Vector2(0.5f, 1f);

        [SerializeField] private Button _exitButton;
        [SerializeField] private RectTransform _rowContainer;
        [SerializeField] private RewardRowViewMono _rowPrefab;
        [SerializeField, Min(0f)] private float _countDuration = 0.5f;
        [SerializeField, Min(0f)] private float _rowSpacing = 12f;
        [SerializeField, Min(0f)] private float _topPadding = 10f;

        private readonly Dictionary<RewardDefinition, RewardRowViewMono> _rows = new Dictionary<RewardDefinition, RewardRowViewMono>();
        private readonly List<RewardRowViewMono> _orderedRows = new List<RewardRowViewMono>();
        private readonly Stack<RewardRowViewMono> _pool = new Stack<RewardRowViewMono>();

        private IEventBus _events;

        #endregion

        #region Properties

        private float RowHeight
        {
            get { return ((RectTransform)_rowPrefab.transform).rect.height; }
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            _exitButton.onClick.AddListener(HandleExitClicked);
        }

        private void OnDisable()
        {
            _exitButton.onClick.RemoveListener(HandleExitClicked);
        }

        #endregion

        #region Public Methods

        public void Initialize(IEventBus events)
        {
            _events = events;
        }

        public void SetExitInteractable(bool isInteractable)
        {
            _exitButton.interactable = isInteractable;
        }

        public RectTransform GetIconTarget(RewardDefinition reward)
        {
            return GetOrCreateRow(reward, 0).IconRect;
        }

        public void SetAmount(RewardDefinition reward, int amount)
        {
            if (_rows.TryGetValue(reward, out var row))
            {
                row.AnimateTo(amount, _countDuration);
                return;
            }

            GetOrCreateRow(reward, amount);
        }

        public void Clear()
        {
            foreach (var row in _orderedRows)
            {
                row.gameObject.SetActive(false);
                _pool.Push(row);
            }

            _rows.Clear();
            _orderedRows.Clear();
            ResizeContainer();
        }

        #endregion

        #region Private Methods

        private RewardRowViewMono GetOrCreateRow(RewardDefinition reward, int amount)
        {
            if (_rows.TryGetValue(reward, out var row))
            {
                return row;
            }

            row = _pool.Count > 0 ? _pool.Pop() : CreateRow();
            row.Bind(reward.Icon, amount);
            PlaceRow(row, _orderedRows.Count);
            row.gameObject.SetActive(true);

            _rows.Add(reward, row);
            _orderedRows.Add(row);
            ResizeContainer();
            return row;
        }

        private RewardRowViewMono CreateRow()
        {
            var row = Instantiate(_rowPrefab, _rowContainer);
            row.name = _rowPrefab.name;
            row.gameObject.SetActive(false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = TOP_CENTER;
            rect.anchorMax = TOP_CENTER;
            rect.pivot = TOP_CENTER;
            return row;
        }

        private void PlaceRow(RewardRowViewMono row, int index)
        {
            var rect = (RectTransform)row.transform;
            rect.anchoredPosition = new Vector2(0f, -(_topPadding + index * (RowHeight + _rowSpacing)));
        }

        private void ResizeContainer()
        {
            var height = _topPadding * 2f + _orderedRows.Count * (RowHeight + _rowSpacing);
            _rowContainer.sizeDelta = new Vector2(_rowContainer.sizeDelta.x, height);
        }

        private void HandleExitClicked()
        {
            _events.Publish(new LeaveRequestedEvent());
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _exitButton = UiReferenceFinder.Find<Button>(this, EXIT_BUTTON_NAME);
            _rowContainer = UiReferenceFinder.Find<RectTransform>(this, ROW_CONTAINER_NAME);
        }
#endif

        #endregion
    }
}
