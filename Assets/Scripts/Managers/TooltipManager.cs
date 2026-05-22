using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager IN;

    public bool IsTooltipActive => this.tooltip.gameObject.activeSelf;

    [SerializeField] private UiTooltip tooltip;

    private float delayToHide = 5f;
    private bool blockHideTooltip;

    private void Start()
    {
        HideTooltip();
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
}