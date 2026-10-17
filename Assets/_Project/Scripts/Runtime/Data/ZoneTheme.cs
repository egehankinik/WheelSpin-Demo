using System;
using UnityEngine;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_theme", menuName = "WheelOfFortune/Zone Theme")]
    public sealed class ZoneTheme : ScriptableObject
    {
        [Serializable]
        public struct ZoneStyle
        {
            [SerializeField] private Sprite _currentFrameSprite;
            [SerializeField] private Color _currentFrameColor;
            [SerializeField] private Color _currentNumberColor;
            [SerializeField] private Color _numberColor;

            public Sprite CurrentFrameSprite => _currentFrameSprite;
            public Color CurrentFrameColor => _currentFrameColor;
            public Color CurrentNumberColor => _currentNumberColor;
            public Color NumberColor => _numberColor;
        }

        [SerializeField] private ZoneStyle _normal;
        [SerializeField] private ZoneStyle _safe;
        [SerializeField] private ZoneStyle _super;
        [SerializeField, Range(0f, 1f)] private float _passedZoneAlpha = 0.35f;

        public ZoneStyle GetStyle(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Super:
                    return _super;
                case ZoneType.Safe:
                    return _safe;
                default:
                    return _normal;
            }
        }

        public Color GetNumberColor(ZoneType zoneType, bool isCurrent, bool isPassed)
        {
            var style = GetStyle(zoneType);
            if (isCurrent)
            {
                return style.CurrentNumberColor;
            }

            var color = style.NumberColor;
            if (isPassed)
            {
                color.a *= _passedZoneAlpha;
            }

            return color;
        }
    }
}
