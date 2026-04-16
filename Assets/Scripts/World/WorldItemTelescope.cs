using DG.Tweening;
using UnityEngine;
using static GlobalEnums;

[RequireComponent(typeof(Telescope))]
[RequireComponent(typeof(Animator))]
public class WorldItemTelescope : SearchToolBase //DecorationBase/Draggable
{
    private Telescope linkedTelescope;

    private Animator animator;
    private string currentAnimationState;

    protected override void Awake()
    {
        base.Awake();
        this.linkedTelescope = GetComponent<Telescope>();
        this.animator = GetComponent<Animator>();
        this.searchAreaLayerMask = LayerMask.GetMask("NightSkySearchArea");
    }

    protected override void Start()
    {
        base.Start();
        SetAnimatorState("Decoration");
    }

    protected override bool DoOnBeginDrag()
    {   
        SetLootFieldParent(ForagingManager.IN.NightSkyLootField);

        SetAnimatorState("BeginDrag");
        return true;
    }

    // public override void OnDragUpdate()
    // {
    //     base.OnDragUpdate();
    //     this.isOverSearchableArea = IsOverSearchableArea();

    //     SetSearchMode(this.isOverSearchableArea);

    //     if(this.isOverSearchableArea || !this.isMaskEnabled)
    //     {
    //         ScrollInnerWorld();
    //     }
    // }

    protected override void DoOnEndDrag()
    {
        base.DoOnEndDrag();

        SetAnimatorState("EndDrag");
    }

    public override void SetSearchMode(bool inIsSearchMode, bool inShouldForce = false)
    {
        if (!this.isMaskEnabled)
            return;

        if (this.IsInSearchMode == inIsSearchMode && !inShouldForce)
            return;

        this.IsInSearchMode = inIsSearchMode;

        Debug.Log($"Telescope Search Mode: {this.IsInSearchMode}. frame: {Time.frameCount}");

        if (inIsSearchMode)
        {
            SetAnimatorState("BeginSearch");
        }
        else
        {
            SetAnimatorState("EndSearch");
        }
    }

    private void SetAnimatorState(string stateName)
    {
        if (this.currentAnimationState == stateName)
            return;
        
        var info = this.animator.GetCurrentAnimatorStateInfo(0);

        if(info.normalizedTime < 1f)
            return;
      
        this.currentAnimationState = stateName;
        this.animator.CrossFade(stateName, 0.1f);
    }
}