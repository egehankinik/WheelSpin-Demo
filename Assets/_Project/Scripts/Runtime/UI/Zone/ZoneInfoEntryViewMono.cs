using TMPro;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneInfoEntryViewMono : MonoBehaviour
    {
        #region Fields

        private const string VALUE_SUFFIX = "_value";

        [SerializeField] private ZoneTypeDefinition _zoneType;
        [SerializeField] private TMP_Text _zoneText;

        #endregion

        #region Properties

        public ZoneTypeDefinition ZoneType
        {
            get { return _zoneType; }
        }

        #endregion

        #region Public Methods

        public void Show(int zone)
        {
            _zoneText.text = zone.ToString();
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _zoneText = UiReferenceFinder.FindWithSuffix<TMP_Text>(this, VALUE_SUFFIX);
            if (!_zoneType)
            {
                Debug.LogError($"{name}: zone type is not assigned.", this);
            }
        }
#endif

        #endregion
    }
}
