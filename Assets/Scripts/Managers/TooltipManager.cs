using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager IN;

    public bool IsTooltipActive => this.tooltip.gameObject.activeSelf;
    public bool IsHarvestRejectMessageActive => this.harvestRejectMessage.gameObject.activeSelf;

    [SerializeField] private UiTooltip tooltip;
    [SerializeField] private UiHarvestRejectMessage harvestRejectMessage;

    private float delayToHide = 5f;
    private bool blockHideTooltip;

    private void Start()
    {
        HideTooltip();
        harvestRejectMessage.Hide();
    }

    private void Update()
    {
        if (PlatformManager.IsDesktop)
            return;
            
        if(this.blockHideTooltip)
            return;

        if (Input.GetMouseButtonDown(0) && IsTooltipActive)
        {
            HideTooltip();
            HideHarvestRejectMessage();
        }
    }

    public void ShowTooltip(string inText, Vector3 inPosition, UiTooltip.ETailDirection inTailDirection = UiTooltip.ETailDirection.Down)
    {
        this.tooltip.Show(inText, inPosition, inTailDirection);

        this.blockHideTooltip = true;

        Invoke(nameof(UnblockTooltip), 0.1f);

        if(PlatformManager.IsMobile)
        {
            if(this != null && this.gameObject != null)
                CancelInvoke(nameof(HideTooltip));
                
            Invoke(nameof(HideTooltip), this.delayToHide);
        }
    }

    public void HideTooltip()
    {
        if(this != null && this.gameObject != null)
            CancelInvoke(nameof(HideTooltip));

        this.tooltip.Hide();
    }

    public void SetTextColor(Color inColor)
    {
        this.tooltip.SetTextColor(inColor);
    }

    public void SetBackgroundColor(Color inColor)
    {
        this.tooltip.SetBackgroundColor(inColor);
    }

    private void UnblockTooltip()
    {
        this.blockHideTooltip = false;
    }

    public void ShowHarvestRejectMessage(string inText, string inTitle, Vector3 inPosition)
    {
        this.harvestRejectMessage.Show(inText, inTitle, inPosition);

        this.blockHideTooltip = true;

        Invoke(nameof(UnblockTooltip), 0.1f);

        if(this != null && this.gameObject != null)
            CancelInvoke(nameof(HideHarvestRejectMessage));
                
        Invoke(nameof(HideHarvestRejectMessage), this.delayToHide);
    }

    public void HideHarvestRejectMessage()
    {
        if(this != null && this.gameObject != null)
            CancelInvoke(nameof(HideHarvestRejectMessage));

        this.harvestRejectMessage.Hide();
    }
}