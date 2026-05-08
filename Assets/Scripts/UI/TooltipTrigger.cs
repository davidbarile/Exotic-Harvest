using UnityEngine;
using UnityEngine.EventSystems;
using static UiTooltip;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 tooltipOffset = new(0, 200, 0);

    [SerializeField] private ETailDirection tailDirection = ETailDirection.Down;

    [TextArea(0, 5)] public string TooltipText;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(this.TooltipText))
            return;
            
        TooltipManager.IN.ShowTooltip(this.TooltipText, this.transform.position + tooltipOffset, this.tailDirection);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.IN.HideTooltip();
    }

    private void OnDisable()
    {
        TooltipManager.IN.HideTooltip();
    }
}