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

        print($"WorldItemCrystal '{this.gameObject.name}' detected trigger enter with collider: {inCollider.gameObject.name}");

        if (inCollider.CompareTag("Moonbeam"))
        {
            print($"2.  WorldItemCrystal '{this.gameObject.name}' detected trigger enter with collider: {inCollider.gameObject.name}");
            var moonbeam = inCollider.GetComponentInParent<Moonbeam>();
            if (moonbeam != null)
            {
                print($"3.  WorldItemCrystal '{this.gameObject.name}' detected trigger enter with collider: {inCollider.gameObject.name}");
                if (!this.linkedPassiveHarvester.CollectableResourceTypes.Contains(moonbeam.ResourceType))
                    return;

                print($"4.  WorldItemCrystal '{this.gameObject.name}' detected trigger enter with collider: {inCollider.gameObject.name}");

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
            var success = this.linkedPassiveHarvester.TryAddAmount(this.activeMoonbeam.Amount, this.activeMoonbeam.ResourceType);

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