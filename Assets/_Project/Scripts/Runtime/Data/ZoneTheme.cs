using System;
using System.Collections.Generic;
using UnityEngine;

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

        [Serializable]
        public struct ZoneStyleEntry
        {
            [SerializeField] private ZoneTypeDefinition _zoneType;
            [SerializeField] private ZoneStyle _style;

            public ZoneTypeDefinition ZoneType => _zoneType;
            public ZoneStyle Style => _style;
        }

        [SerializeField] private ZoneStyle _defaultStyle;
        [SerializeField] private List<ZoneStyleEntry> _styles = new List<ZoneStyleEntry>();
        [SerializeField, Range(0f, 1f)] private float _passedZoneAlpha = 0.35f;

        public ZoneStyle GetStyle(ZoneTypeDefinition zoneType)
        {
            foreach (var entry in _styles)
            {
                if (entry.ZoneType == zoneType)
                {
                    return entry.Style;
                }
            }

            return _defaultStyle;
        }

        public Color GetNumberColor(ZoneTypeDefinition zoneType, bool isCurrent, bool isPassed)
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
