using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneBarView : MonoBehaviour
    {
        private const string ContentName = "ui_zone_bar_content";
        private const string CurrentFrameName = "ui_image_zone_current_frame_value";

        [SerializeField] private ZoneTheme _theme;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _currentFrameImage;
        [SerializeField] private ZoneCellView[] _cells;
        [SerializeField, Min(1f)] private float _cellWidth = 84f;
        [SerializeField, Min(0f)] private float _shiftDuration = 0.4f;
        [SerializeField] private PunchSettings _framePunch = new PunchSettings(0.12f, 0.25f);

        private ZoneRules _rules;
        private int _shownZone;
        private Tween _shiftTween;
        private Tween _framePunchTween;

        private int CenterIndex => _cells.Length / 2;

        public void Initialize(ZoneRules rules)
        {
            _rules = rules;
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

            ApplyFrameStyle(zone);
            ColorCells(_shownZone, zone, _shiftDuration);

            _shiftTween = DOTween
                .To(() => _content.anchoredPosition.x, SetContentOffset, -_cellWidth, _shiftDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Refresh(zone);
                    PunchFrame();
                });
        }

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

                var color = _theme.GetNumberColor(_rules.GetZoneType(cellZone), cellZone == currentZone, cellZone < currentZone);
                _cells[i].Show(cellZone, color, duration);
            }
        }

        private void ApplyFrameStyle(int zone)
        {
            var style = _theme.GetStyle(_rules.GetZoneType(zone));
            _currentFrameImage.sprite = style.CurrentFrameSprite;
            _currentFrameImage.color = style.CurrentFrameColor;
        }

        private void PunchFrame()
        {
            _framePunchTween?.Kill(true);
            _framePunchTween = _framePunch.PunchScale(_currentFrameImage.rectTransform);
        }

        private void SetContentOffset(float x)
        {
            _content.anchoredPosition = new Vector2(x, 0f);
        }

        private void OnDisable()
        {
            _shiftTween?.Kill(true);
            _framePunchTween?.Kill(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _content = UiReferenceFinder.Find<RectTransform>(this, ContentName);
            _currentFrameImage = UiReferenceFinder.Find<Image>(this, CurrentFrameName);
            _cells = GetComponentsInChildren<ZoneCellView>(true);
        }
#endif
    }
}
