using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardPanelView : MonoBehaviour
    {
        private const string ExitButtonName = "ui_button_exit";
        private const string RowContainerName = "ui_reward_panel_content";

        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);

        [SerializeField] private Button _exitButton;
        [SerializeField] private RectTransform _rowContainer;
        [SerializeField] private RewardRowView _rowPrefab;
        [SerializeField, Min(0f)] private float _countDuration = 0.5f;
        [SerializeField, Min(0f)] private float _rowSpacing = 12f;
        [SerializeField, Min(0f)] private float _topPadding = 10f;

        private readonly Dictionary<RewardDefinition, RewardRowView> _rows = new Dictionary<RewardDefinition, RewardRowView>();
        private readonly List<RewardRowView> _orderedRows = new List<RewardRowView>();
        private readonly Stack<RewardRowView> _pool = new Stack<RewardRowView>();

        public event Action ExitClicked;

        private void OnEnable()
        {
            _exitButton.onClick.AddListener(HandleExitClicked);
        }

        private void OnDisable()
        {
            _exitButton.onClick.RemoveListener(HandleExitClicked);
        }

        public void SetExitInteractable(bool interactable) => _exitButton.interactable = interactable;

        public RectTransform GetIconTarget(RewardDefinition reward) => GetOrCreateRow(reward, 0).IconRect;

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

        private RewardRowView GetOrCreateRow(RewardDefinition reward, int amount)
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

        private RewardRowView CreateRow()
        {
            var row = Instantiate(_rowPrefab, _rowContainer);
            row.gameObject.SetActive(false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = TopCenter;
            rect.anchorMax = TopCenter;
            rect.pivot = TopCenter;
            return row;
        }

        private void PlaceRow(RewardRowView row, int index)
        {
            var rect = (RectTransform)row.transform;
            rect.anchoredPosition = new Vector2(0f, -(_topPadding + index * (RowHeight + _rowSpacing)));
        }

        private void ResizeContainer()
        {
            var height = _topPadding * 2f + _orderedRows.Count * (RowHeight + _rowSpacing);
            _rowContainer.sizeDelta = new Vector2(_rowContainer.sizeDelta.x, height);
        }

        private float RowHeight => ((RectTransform)_rowPrefab.transform).rect.height;

        private void HandleExitClicked() => ExitClicked?.Invoke();

#if UNITY_EDITOR
        private void OnValidate()
        {
            _exitButton = UiReferenceFinder.Find<Button>(this, ExitButtonName);
            _rowContainer = UiReferenceFinder.Find<RectTransform>(this, RowContainerName);
        }
#endif
    }
}
