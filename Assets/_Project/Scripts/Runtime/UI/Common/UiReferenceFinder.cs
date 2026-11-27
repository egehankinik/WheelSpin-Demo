using System;
using UnityEngine;

namespace WheelOfFortune.UI.Common
{
    public static class UiReferenceFinder
    {
        #region Public Methods

        public static T Find<T>(Component root, string childName) where T : Component
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName && child.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            Debug.LogError($"{root.name}: child '{childName}' with {typeof(T).Name} was not found.", root);
            return null;
        }

        public static T FindWithSuffix<T>(Component root, string nameSuffix) where T : Component
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.EndsWith(nameSuffix, StringComparison.Ordinal) && child.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            Debug.LogError($"{root.name}: no child ending with '{nameSuffix}' with {typeof(T).Name} was found.", root);
            return null;
        }

        #endregion
    }
}
