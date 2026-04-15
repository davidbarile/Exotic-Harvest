using UnityEngine;
using static GlobalEnums;

public class WorldItemTelescope : SearchToolBase //DecorationBase/Draggable
{
    private Telescope linkedTelescope;

    protected override void Awake()
    {
        base.Awake();
        this.linkedTelescope = GetComponent<Telescope>();
        this.searchAreaLayerMask = LayerMask.GetMask("NightSkySearchArea");
    }

    protected override bool DoOnBeginDrag()
    {
        SetLootFieldParent(ForagingManager.IN.NightSkyLootField);

        SetSearchMode(true);
        return true;
    }
}