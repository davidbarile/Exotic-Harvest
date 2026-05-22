using UnityEngine;
using UnityEngine.EventSystems;
using static UiTooltip;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 tooltipOffset = new(0, 200, 0);

    [SerializeField] private ETailDirection tailDirection = ETailDirection.Down;

    [TextArea(0, 5)] public string TooltipText;

    private float delayToHide = 5f;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(this.TooltipText))
            return;

        TooltipManager.IN.ShowTooltip(this.TooltipText, this.transform.position + tooltipOffset, this.tailDirection);

        if(PlatformManager.IsMobile)
        {
            CancelInvoke(nameof(HideTooltip));
            Invoke(nameof(HideTooltip), this.delayToHide);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (PlatformManager.IsMobile)
            return;

        TooltipManager.IN.HideTooltip();
    }

    private void OnDisable()
    {
        TooltipManager.IN.HideTooltip();
    }

    private void HideTooltip()
    {
        TooltipManager.IN.HideTooltip();
    }
}