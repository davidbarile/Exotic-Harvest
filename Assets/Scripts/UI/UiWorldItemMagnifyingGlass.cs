using UnityEngine;

[RequireComponent(typeof(MagnifyingGlass))]
public class UiWorldItemMagnifyingGlass : UiDecorationBase
{
    private MagnifyingGlass linkedMagnifyingGlass;

    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<MagnifyingGlass>();
        this.linkedMagnifyingGlass = GetComponent<MagnifyingGlass>();
    }

    protected override bool DoOnDrag()
    {
        this.linkedMagnifyingGlass.ScrollInnerWorld();//dunno maybe this should all be in here

        if (!base.DoOnDrag())
        {
            return false;
        }

        return true;
    }
}