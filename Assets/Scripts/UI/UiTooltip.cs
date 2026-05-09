using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiTooltip : MonoBehaviour
{
    public enum ETailDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRectTransform;
    [SerializeField] private Graphic background;
    [SerializeField] private Graphic[] tails;

    public void Show(string inText, Vector3 inPosition, ETailDirection inTailDirection = ETailDirection.Down)
    {
        this.tooltipText.text = inText;
        this.transform.position = inPosition;
        SetTailDirection(inTailDirection);
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (this == null || this.gameObject == null) return;
        
        this.gameObject.SetActive(false);
    }

    private void SetTailDirection(ETailDirection inTailDirection)
    {
        for (int i = 0; i < this.tails.Length; i++)
        {
            this.tails[i].transform.parent.gameObject.SetActive(i == (int)inTailDirection);
        }

        this.tooltipText.alignment = inTailDirection switch
        {
            ETailDirection.Up => TextAlignmentOptions.Center,
            ETailDirection.Down => TextAlignmentOptions.Center,
            ETailDirection.Left => TextAlignmentOptions.Right,
            ETailDirection.Right => TextAlignmentOptions.Left,
            _ => this.tooltipText.alignment
        };

        this.tooltipRectTransform.pivot = inTailDirection switch
        {
            ETailDirection.Up => new Vector2(0.5f, 1f),
            ETailDirection.Down => new Vector2(0.5f, 0f),
            ETailDirection.Left => new Vector2(0f, 0.5f),
            ETailDirection.Right => new Vector2(1f, 0.5f),
            _ => this.tooltipRectTransform.pivot
        };
    }

    public void SetTextColor(Color inColor)
    {
        this.tooltipText.color = inColor;
    }

    public void SetBackgroundColor(Color inColor)
    {
        this.background.color = inColor;

        for (int i = 0; i < this.tails.Length; i++)
        {
            this.tails[i].color = inColor;
        }
    }
}