using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class OnFocusModifierBase : MonoBehaviour
{
#region Struct
    [Serializable]
    private struct OnFocusAnchor
    {
        public bool CopyCurrentRectTransformProperties;

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

    [SerializeField] private OnFocusAnchor focusedAnchor, unFocusedAnchor;

    private RectTransform rectTransform;

    protected virtual void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
        ScreenManager.OnGameFocusChanged += OnGameFocusChanged;
    }

    protected virtual void OnDestroy()
    {
        ScreenManager.OnGameFocusChanged -= OnGameFocusChanged;
    }

    protected virtual void OnGameFocusChanged(bool hasFocus)
    {
        var focusAnchor = hasFocus ? this.focusedAnchor : this.unFocusedAnchor;

        if (focusAnchor.referenceRectTransform != null)
        {
            RectTransformExtensions.CopyRectTransform(focusAnchor.referenceRectTransform, this.rectTransform);
            return;
        }

        if (focusAnchor.overrideAnchors)
        {
            this.rectTransform.anchorMin = focusAnchor.anchorMin;
            this.rectTransform.anchorMax = focusAnchor.anchorMax;
        }

        if (focusAnchor.overridePivot)
            this.rectTransform.pivot = focusAnchor.pivot;

        if (focusAnchor.overrideAnchoredPosition)
            this.rectTransform.anchoredPosition = focusAnchor.anchoredPosition;

        if (focusAnchor.overrideSizeDelta)
            this.rectTransform.sizeDelta = focusAnchor.sizeDelta;

        if (focusAnchor.overrideLocalPosition)
            this.rectTransform.localPosition = focusAnchor.localPosition;

        if (focusAnchor.overrideRotation)
            this.rectTransform.localEulerAngles = focusAnchor.eulerAngles;

        if (focusAnchor.overrideScale)
            this.rectTransform.localScale = focusAnchor.localScale;

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
    }
#endregion

    [Button("Copy Current RectTransform Properties to Anchors")]
    public void CopyCurrentRectTransformProperties()
    {
        this.rectTransform = GetComponent<RectTransform>();

        if (this.focusedAnchor.CopyCurrentRectTransformProperties)
            CopyPropertiesToAnchor(ref this.focusedAnchor);

        if (this.unFocusedAnchor.CopyCurrentRectTransformProperties)
            CopyPropertiesToAnchor(ref this.unFocusedAnchor);

        void CopyPropertiesToAnchor(ref OnFocusAnchor anchor)
        {
            anchor.anchorMin = this.rectTransform.anchorMin;
            anchor.anchorMax = this.rectTransform.anchorMax;
            anchor.pivot = this.rectTransform.pivot;
            anchor.anchoredPosition = this.rectTransform.anchoredPosition;
            anchor.sizeDelta = this.rectTransform.sizeDelta;
            anchor.localPosition = this.rectTransform.localPosition;
            anchor.eulerAngles = this.rectTransform.localEulerAngles;
            anchor.localScale = this.rectTransform.localScale;
        }
        
        #if UNITY_EDITOR

        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}