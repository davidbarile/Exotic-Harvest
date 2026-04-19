using System;
using UnityEngine;
using UnityEngine.UI;

public static class RectTransformExtensions
{
    public static void SetBoxColliderDimensionsToMatch(this RectTransform rectTransform)
    {
        var boxCollider = rectTransform.GetComponent<BoxCollider>();
        boxCollider.size = rectTransform.rect.size;
    }

    public static void FillParent(this RectTransform rectTransform)
    {
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
    }

	public static float GetWidth(this RectTransform rect)
	{
		return rect.sizeDelta.x;
	}

	public static void SetWidth(this RectTransform rect, float width)
	{
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
	}

	public static float GetHeight(this RectTransform rect)
	{
		return rect.sizeDelta.y;
	}

	public static void SetHeight(this RectTransform rect, float height)
	{
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	public static void CopyRectTransform(this RectTransform rect, RectTransform rectToCopy, bool shouldForceRebuildLayout = false)
	{
		rect.anchoredPosition = rectToCopy.anchoredPosition;
		rect.pivot = rectToCopy.pivot;
		rect.sizeDelta = rectToCopy.sizeDelta;
		rect.anchorMin = rectToCopy.anchorMin;
		rect.anchorMax = rectToCopy.anchorMax;
		rect.localScale = rectToCopy.localScale;
		rect.localRotation = rectToCopy.localRotation;

		if (shouldForceRebuildLayout)
			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
	}
}