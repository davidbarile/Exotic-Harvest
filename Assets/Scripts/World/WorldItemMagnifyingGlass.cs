using UnityEngine;

[RequireComponent(typeof(MagnifyingGlass))]
public class WorldItemMagnifyingGlass : SearchToolBase //DecorationBase/Draggable
{
    private MagnifyingGlass linkedMagnifyingGlass;

    protected override void Awake()
    {
        base.Awake();
        this.linkedMagnifyingGlass = GetComponent<MagnifyingGlass>();
        this.searchAreaLayerMask = LayerMask.GetMask("MeadowSearchArea");
    }

    protected override bool DoOnBeginDrag()
    {
        SetLootFieldParent(ForagingManager.IN.MeadowLootField);

        SetSearchMode(true);
        return true;
    }
}