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

        this.OnDeinitialize = () =>
        {
            ForagingManager.IN.MeadowLootField.transform.SetParent(ForagingManager.IN.LootContainersParent);
            ForagingManager.IN.MeadowLootField.transform.localPosition = Vector3.zero;
            ForagingManager.IN.MeadowLootField.transform.localScale = Vector3.one;
        };

        SetSearchMode(true);
        return true;
    }
}