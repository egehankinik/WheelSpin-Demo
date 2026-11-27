using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "wheel_", menuName = "WheelOfFortune/Wheel Definition")]
    public sealed class WheelDefinition : ScriptableObject
    {
        #region Fields

        [SerializeField] private string _title;
        [SerializeField] private string _subtitle;
        [SerializeField] private Sprite _wheelSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [SerializeField] private Color _glowColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private List<WheelSlice> _slices = new List<WheelSlice>();

        #endregion

        #region Properties

        public string Title
        {
            get { return _title; }
        }

        public string Subtitle
        {
            get { return _subtitle; }
        }

        public Sprite WheelSprite
        {
            get { return _wheelSprite; }
        }

        public Sprite IndicatorSprite
        {
            get { return _indicatorSprite; }
        }

        public Color GlowColor
        {
            get { return _glowColor; }
        }

        public IReadOnlyList<WheelSlice> Slices
        {
            get { return _slices; }
        }

        public int SliceCount
        {
            get { return _slices.Count; }
        }

        public float SliceAngle
        {
            get
            {
                if (SliceCount == 0)
                {
                    return 0f;
                }

                return 360f / SliceCount;
            }
        }

        public bool HasBomb
        {
            get
            {
                foreach (var slice in _slices)
                {
                    if (slice.IsBomb)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_slices.Count == 0)
            {
                Debug.LogError($"{name}: wheel has no slices.", this);
            }

            for (var i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                if (!slice.IsBomb && !slice.Reward)
                {
                    Debug.LogError($"{name}: slice {i} has no reward assigned.", this);
                }
            }
        }
#endif

        #endregion
    }
}
