using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneBarViewMono : MonoBehaviour
    {
        #region Fields

        private const string CONTENT_NAME = "ui_zone_bar_content";
        private const string CURRENT_FRAME_NAME = "ui_image_zone_current_frame_value";

        [SerializeField] private ZoneTheme _theme;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _currentFrameImage;
        [SerializeField] private ZoneCellViewMono[] _cells;
        [SerializeField, Min(1f)] private float _cellWidth = 84f;
        [SerializeField, Min(0f)] private float _shiftDuration = 0.4f;
        [SerializeField] private Ease _shiftEase = Ease.OutCubic;
        [SerializeField] private PunchSettings _framePunch = new PunchSettings(0.12f, 0.25f);

        private Func<int, ZoneTypeDefinition> _zoneTypeAt;
        private int _shownZone;
        private int _targetZone;
        private Tween _shiftTween;
        private Tween _framePunchTween;

        #endregion

        #region Properties

        private int CenterIndex
        {
            get { return _cells.Length / 2; }
        }

        #endregion

        #region Unity Methods

        private void OnDisable()
        {
            _shiftTween?.Kill(true);
            _framePunchTween?.Kill(true);
        }

        #endregion

        #region Public Methods

        public void Initialize(Func<int, ZoneTypeDefinition> zoneTypeAt)
        {
            _zoneTypeAt = zoneTypeAt;
            for (var i = 0; i < _cells.Length; i++)
            {
                _cells[i].SetPosition((i - CenterIndex) * _cellWidth);
            }
        }

        public void ShowZone(int zone)
        {
            _shiftTween?.Kill(true);

            var isNextZone = _shownZone > 0 && zone == _shownZone + 1;
            if (!isNextZone)
            {
                Refresh(zone);
                return;
            }

            _targetZone = zone;
            ApplyFrameStyle(zone);
            ColorCells(_shownZone, zone, _shiftDuration);

            _shiftTween = DOTween
                .To(GetContentOffset, SetContentOffset, -_cellWidth, _shiftDuration)
                .SetEase(_shiftEase)
                .OnComplete(HandleShiftCompleted)
                .SetLink(gameObject);
        }

        #endregion

        #region Private Methods

        private void Refresh(int zone)
        {
            _shownZone = zone;
            SetContentOffset(0f);
            ColorCells(zone, zone, 0f);
            ApplyFrameStyle(zone);
        }

        private void ColorCells(int firstCellBaseZone, int currentZone, float duration)
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                var cellZone = firstCellBaseZone + i - CenterIndex;
                if (cellZone < 1)
                {
                    _cells[i].Hide();
                    continue;
                }

                var color = _theme.GetNumberColor(_zoneTypeAt(cellZone), cellZone == currentZone, cellZone < currentZone);
                _cells[i].Show(cellZone, color, duration);
            }
        }

        private void ApplyFrameStyle(int zone)
        {
            var style = _theme.GetStyle(_zoneTypeAt(zone));
            _currentFrameImage.sprite = style.CurrentFrameSprite;
            _currentFrameImage.color = style.CurrentFrameColor;
        }

        private void HandleShiftCompleted()
        {
            Refresh(_targetZone);
            _framePunchTween?.Kill(true);
            _framePunchTween = _framePunch.PunchScale(_currentFrameImage.rectTransform);
        }

        private float GetContentOffset()
        {
            return _content.anchoredPosition.x;
        }

        private void SetContentOffset(float x)
        {
            _content.anchoredPosition = new Vector2(x, 0f);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _content = UiReferenceFinder.Find<RectTransform>(this, CONTENT_NAME);
            _currentFrameImage = UiReferenceFinder.Find<Image>(this, CURRENT_FRAME_NAME);
            _cells = GetComponentsInChildren<ZoneCellViewMono>(true);
        }
#endif

        #endregion
    }
}
