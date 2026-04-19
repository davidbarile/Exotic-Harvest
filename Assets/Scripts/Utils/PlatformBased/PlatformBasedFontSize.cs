using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class PlatformBasedFontSize : PlatformBasedModifierBase
{
    [Serializable]
    private struct PlatformFontData
    {
        public PlatformFlags platform;
        public int fontSize;
        
        [Space]
        public bool enableAutoSizing;
        public int fontSizeMin;
        public int fontSizeMax;

        [Space]
        public bool overrideSpacing;

        [Space]
        public bool overrideAlignment;
        public TextAlignmentOptions alignment;
        
        public float characterSpacing;
        public float wordSpacing;
        public float lineSpacing;

        [Space]
        public bool overrideWrapAndOverflow;
        public TextWrappingModes textWrappingMode;
        public TextOverflowModes overflowMode;
    }

    [Tooltip("Do not duplicate platform flags in elements")]
    [SerializeField] private PlatformFontData[] _platformFontData;

    protected override void CheckForNullOrDuplicates()
    {
        if (_platformFontData == null)
            return;

        var seenPlatforms = new HashSet<PlatformFlags>();
        for (var i = 0; i < _platformFontData.Length; i++)
        {
            if (!seenPlatforms.Add(_platformFontData[i].platform))
                Debug.LogError($"Duplicate platform flag '{_platformFontData[i].platform}' found at element {i} in {name}", gameObject);

            if (_platformFontData[i].platform == PlatformFlags.None)
                Debug.LogError($"Platform flag is not set for element {i} in {name}", gameObject);
        }
    }

    protected override bool Execute()
    {
        if (!base.Execute())
            return false;

        var textMeshPro = GetComponent<TMP_Text>();

        foreach (var platformFontData in _platformFontData)
        {
            if (PlatformManager.IN.Matches(platformFontData.platform))
            {
                textMeshPro.fontSize = platformFontData.fontSize;

                textMeshPro.enableAutoSizing = platformFontData.enableAutoSizing;
                textMeshPro.fontSizeMin = platformFontData.fontSizeMin;
                textMeshPro.fontSizeMax = platformFontData.fontSizeMax;

                if (platformFontData.overrideSpacing)
                {
                    textMeshPro.characterSpacing = platformFontData.characterSpacing;
                    textMeshPro.wordSpacing = platformFontData.wordSpacing;
                    textMeshPro.lineSpacing = platformFontData.lineSpacing;
                }

                if (platformFontData.overrideAlignment)
                    textMeshPro.alignment = platformFontData.alignment;

                if (platformFontData.overrideWrapAndOverflow)
                {
                    textMeshPro.textWrappingMode = platformFontData.textWrappingMode;
                    textMeshPro.overflowMode = platformFontData.overflowMode;
                }

                break;
            }
        }

        return true;
    }
}
