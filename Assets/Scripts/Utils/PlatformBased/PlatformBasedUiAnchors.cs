using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PlatformBasedUiAnchors : PlatformBasedModifierBase
{
    [Serializable]
    private struct PlatformBasedAnchor
    {
        public PlatformFlags platform;

        [Header("Reference RectTransform (copies all anchoring settings from this)")]
        public RectTransform referenceRectTransform;

        [Header("Anchoring Overrides (ignored if Reference RectTransform is set)")]
        public bool overrideAnchors;
        public Vector2 anchorMin;
        public Vector2 anchorMax;

        [Space]
        public bool overridePivot;
        public Vector2 pivot;

        [Space]
        public bool overrideAnchoredPosition;
        public Vector2 anchoredPosition;

        [Space]
        public bool overrideSizeDelta;
        public Vector2 sizeDelta;

        [Space]
        public bool overrideLocalPosition;
        public Vector3 localPosition;

        [Space]
        public bool overrideRotation;
        public Vector3 eulerAngles;

        [Space]
        public bool overrideScale;
        public Vector3 localScale;
    }

    [Tooltip("Do not duplicate platform flags in elements")]
    [SerializeField] private PlatformBasedAnchor[] _platformBasedAnchors;

    protected override void CheckForNullOrDuplicates()
    {
        if (_platformBasedAnchors == null)
            return;

        var seenPlatforms = new HashSet<PlatformFlags>();
        for (var i = 0; i < _platformBasedAnchors.Length; i++)
        {
            if (!seenPlatforms.Add(_platformBasedAnchors[i].platform))
                Debug.LogError($"Duplicate platform flag '{_platformBasedAnchors[i].platform}' found at element {i} in {name}", gameObject);

            if (_platformBasedAnchors[i].platform == PlatformFlags.None)
                Debug.LogError($"Platform flag is not set for element {i} in {name}", gameObject);
        }
    }
    
    protected override bool Execute()
    {
        if (!base.Execute())
            return false;

        var rectTransform = GetComponent<RectTransform>();

        foreach (var platformBasedUiAnchor in _platformBasedAnchors)
        {
            if (PlatformManager.IN.Matches(platformBasedUiAnchor.platform))
            {
                if (platformBasedUiAnchor.referenceRectTransform != null)
                {
                    RectTransformExtensions.CopyRectTransform(platformBasedUiAnchor.referenceRectTransform, rectTransform);
                    break;
                }

                if (platformBasedUiAnchor.overrideAnchors)
                {
                    rectTransform.anchorMin = platformBasedUiAnchor.anchorMin;
                    rectTransform.anchorMax = platformBasedUiAnchor.anchorMax;
                }

                if (platformBasedUiAnchor.overridePivot)
                    rectTransform.pivot = platformBasedUiAnchor.pivot;

                if (platformBasedUiAnchor.overrideAnchoredPosition)
                    rectTransform.anchoredPosition = platformBasedUiAnchor.anchoredPosition;

                if (platformBasedUiAnchor.overrideSizeDelta)
                    rectTransform.sizeDelta = platformBasedUiAnchor.sizeDelta;

                if (platformBasedUiAnchor.overrideLocalPosition)
                    rectTransform.localPosition = platformBasedUiAnchor.localPosition;

                if (platformBasedUiAnchor.overrideRotation)
                    rectTransform.localEulerAngles = platformBasedUiAnchor.eulerAngles;

                if (platformBasedUiAnchor.overrideScale)
                    rectTransform.localScale = platformBasedUiAnchor.localScale;

                break;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        return true;
    }
}