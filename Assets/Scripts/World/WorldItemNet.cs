using UnityEngine;

public class WorldItemNet : DecorationBase
{
    private int layerMask = -1;

    protected override void Awake()
    {
        this.linkedForager = GetComponent<Net>();
        this.layerMask = LayerMask.GetMask("Fireflies");//TODO: change
    }


    public override void OnDragUpdate()
    {
        base.OnDragUpdate();
         
        if (!this.harvestRejectMessage)
            return;

        var hitCollider = Physics2D.OverlapPoint(this.worldProxy.transform.position, this.layerMask);

        if (hitCollider != null)
        {
            var rejectMessage = ForagingManager.GetHarvestRejectMessage(this.harvestLocation, out var rejectTitle);

            if(string.IsNullOrEmpty(rejectMessage))
                this.harvestRejectMessage.Hide();
            else
                this.harvestRejectMessage.Show(rejectMessage, rejectTitle);
        }
        else
        {
            this.harvestRejectMessage.Hide();
        }
    }   
}