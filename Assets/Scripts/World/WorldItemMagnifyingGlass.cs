using UnityEngine;

[RequireComponent(typeof(MagnifyingGlass))]
public class WorldItemMagnifyingGlass : SearchToolBase
{
    private MagnifyingGlass linkedMagnifyingGlass;

    protected override void Awake()
    {
        base.Awake();
        this.linkedMagnifyingGlass = GetComponent<MagnifyingGlass>();
    }
}