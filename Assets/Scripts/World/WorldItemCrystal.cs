using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Crystal))]
public class WorldItemCrystal : DecorationBase
{
    [Space,SerializeField] protected GameObject fillBarDisplay;
    [SerializeField] protected Image fillBarImage;

    [Space, Range(0f, 5f), SerializeField] protected float timeToActivateHover = 1f;//how long the player needs to hover over a searchable object before it "activates"

    private DateTime startActiveObjectHoverTime = DateTime.MinValue;

    private Moonbeam activeMoonbeam = null;

    protected override void Awake()
    {
        this.linkedForager = GetComponent<Crystal>();
    }

    protected override void Start()
    {
        base.Start();

        HideFillBar();
    }

    public void SetFillAmount(float fillAmount)
    {
        if (this.fillBarDisplay)
        {
            this.fillBarDisplay.SetActive(fillAmount > 0f);

            if (this.fillBarImage)
                this.fillBarImage.fillAmount = fillAmount;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D inCollider)
    {
        if (inCollider == null)
            return;

        if (inCollider.CompareTag("Moonbeam"))
        {
            var moonbeam = inCollider.GetComponentInParent<Moonbeam>();
            if (moonbeam != null)
            {
                if (!this.linkedForager.CanCollectResourceType(moonbeam.ResourceType))
                    return;

                //start timer and fillbar
                this.activeMoonbeam = moonbeam;
                this.startActiveObjectHoverTime = DateTime.Now;
                this.fillBarDisplay.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (this.activeMoonbeam == null)
        {
            HideFillBar();
            return;
        }

        var hoverDuration = (DateTime.Now - this.startActiveObjectHoverTime).TotalSeconds;

        if (hoverDuration >= this.timeToActivateHover && this.activeMoonbeam != null)
        {
            var success = this.linkedForager.TryAddAmount(this.activeMoonbeam.Amount, this.activeMoonbeam.ResourceType);

            if (success)
                this.activeMoonbeam.Collect(false);

            this.startActiveObjectHoverTime = DateTime.Now + TimeSpan.FromSeconds(1); //reset hover time to prevent immediate re-activation
            HideFillBar();
        }
        else
        {
            var percent = Mathf.Clamp01((float)(hoverDuration / this.timeToActivateHover));
            SetFillAmount(percent);
        }
    }

    private void OnTriggerExit2D(Collider2D inCollider)
    {
        HideFillBar();
    }
    
    private void HideFillBar()
    {
        this.fillBarDisplay.SetActive(false);
        SetFillAmount(0f);
        this.activeMoonbeam = null;
    }
}