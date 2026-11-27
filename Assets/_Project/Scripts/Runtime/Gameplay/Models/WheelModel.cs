using System;
using System.Collections.Generic;

namespace WheelOfFortune.Gameplay
{
    public sealed class WheelModel
    {
        #region Fields

        private readonly List<SliceModel> _slices;

        #endregion

        public WheelModel(IEnumerable<SliceModel> slices)
        {
            if (slices == null)
            {
                throw new ArgumentNullException(nameof(slices));
            }

            _slices = new List<SliceModel>(slices);
            if (_slices.Count == 0)
            {
                throw new ArgumentException("A wheel needs at least one slice.", nameof(slices));
            }

            foreach (var slice in _slices)
            {
                if (slice.IsBomb)
                {
                    HasBomb = true;
                    break;
                }
            }
        }

        #region Properties

        public IReadOnlyList<SliceModel> Slices
        {
            get { return _slices; }
        }

        public int SliceCount
        {
            get { return _slices.Count; }
        }

        public bool HasBomb { get; }

        #endregion
    }
}
