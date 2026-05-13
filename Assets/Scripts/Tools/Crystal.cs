using UnityEngine;
using static GlobalEnums;

public class Crystal : PassiveHarvester
{
    protected override bool CheckGenerationConditions()
    {
        return true;// not using this
    }

    protected override void RefreshQuantityDisplay()
    {
        if (this.DecorationData == null)
            return;

        if (this.quantityText && (this.showQuantityTextWhenEmpty || !this.IsEmpty))
        {
            if (this.activeResourceDisplay != null)
            {
                int amountCollected = Mathf.FloorToInt((float)this.CurrentAmount * this.DecorationData.ConversionRatio);
                int total = Mathf.FloorToInt((float)this.MaxCapacity * this.DecorationData.ConversionRatio);

                if (this.ActiveResourceType == GlobalEnums.EResourceType.Moonbeams)
                {
                    amountCollected = this.CurrentAmount; // Moonbeams display differently, showing actual amount instead of converted amount
                    total = this.MaxCapacity;
                }

                this.activeResourceDisplay.SetValue(amountCollected, total);
                this.quantityText.text = $"{amountCollected}/{total}";
            }
            else
                this.quantityText.text = $"{this.CurrentAmount}/{this.MaxCapacity}";
        }

        UpdateFillMeter(false);
    }
    
    public override bool CollectAll()
    {
        if (this.DecorationData == null || this.IsEmpty)
            return false;

        int amountToCollect = Mathf.FloorToInt((float)this.DecorationData.CurrentAmount * this.DecorationData.ConversionRatio);

        if (this.ActiveResourceType == GlobalEnums.EResourceType.Moonbeams)
        {
            amountToCollect = this.CurrentAmount; // Moonbeams display differently, showing actual amount instead of converted amount
        }

        ResourceManager.IN.AddResource(this.ActiveResourceType, amountToCollect);

        int collectedAmount = amountToCollect;
        this.DecorationData.CurrentAmount = 0;
        SetActiveResourceType(EResourceType.None);
        ResourceManager.OnResourceGained?.Invoke(this.ActiveResourceType, collectedAmount);
        OnCollected(collectedAmount);
        RefreshQuantityDisplay();

        return true;
    }
}