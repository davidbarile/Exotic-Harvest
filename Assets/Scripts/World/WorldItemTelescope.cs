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
        SetAnimatorState("Decoration", true);
    }

    protected override bool DoOnBeginDrag()
    {
        SetLootFieldParent(ForagingManager.IN.NightSkyLootField);

        SetAnimatorState("BeginDrag");
        return true;
    }

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

    private void SetAnimatorState(string stateName, bool shouldForce = false)
    {
        if (this.currentAnimationState == stateName && !shouldForce)
            return;

        this.currentAnimationState = stateName;
        
        if(shouldForce)
            this.animator.Play(stateName, 0, 0f);
        else
            this.animator.CrossFade(stateName, 0.1f);
    }
}