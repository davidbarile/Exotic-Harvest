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
        this.linkedPassiveHarvester = GetComponent<Crystal>();
    }

    protected override void Start()
    {
        base.Start();
        
        this.fillBarDisplay.SetActive(false);
        SetFillAmount(0f);
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

        if(inCollider.CompareTag("Moonbeam"))
        {
            var moonbeam = inCollider.GetComponentInParent<Moonbeam>();
            if(moonbeam != null)
            {
                if (!this.linkedPassiveHarvester.CollectableResourceTypes.Contains(moonbeam.ResourceType))
                    return;

                //start timer and fillbar
                this.activeMoonbeam = moonbeam;
                this.startActiveObjectHoverTime = DateTime.Now;
                this.fillBarDisplay.SetActive(true);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D inCollider)
    {
        //temp
        if (!this.isDragging)
            return;

        if (inCollider == null)
            return;

        var hoverDuration = (DateTime.Now - this.startActiveObjectHoverTime).TotalSeconds;

        if (hoverDuration >= this.timeToActivateHover && this.activeMoonbeam != null)
        {
            var success = this.linkedPassiveHarvester.AddAmount(this.activeMoonbeam.Amount);

            if (success)
                this.activeMoonbeam.Collect(false);

            //dunno - hide?
            this.activeMoonbeam = null;

            this.startActiveObjectHoverTime = DateTime.Now + TimeSpan.FromSeconds(1); //reset hover time to prevent immediate re-activation
            this.fillBarDisplay.SetActive(false);
            SetFillAmount(0f);
        }
        else
        {
            var percent = Mathf.Clamp01((float)(hoverDuration / this.timeToActivateHover));
            SetFillAmount(percent);
        }
    }
    
    private void OnTriggerExit2D(Collider2D inCollider)
    {
        //test & fix
        if (inCollider == null)
            return;

        if(inCollider.CompareTag("Moonbeam"))
        {
            //stop timer and fillbar
            this.fillBarDisplay.SetActive(false);
            SetFillAmount(0f);
            this.activeMoonbeam = null;
        }
    }
}