using TMPro;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneInfoEntryView : MonoBehaviour
    {
        private const string ValueSuffix = "_value";

        [SerializeField] private ZoneTypeDefinition _zoneType;
        [SerializeField] private TMP_Text _zoneText;

        public ZoneTypeDefinition ZoneType => _zoneType;

        public void Show(int zone)
        {
            _zoneText.text = zone.ToString();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _zoneText = UiReferenceFinder.FindWithSuffix<TMP_Text>(this, ValueSuffix);
        }
#endif
    }
}
