using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using static GlobalEnums;

public class DragTarget : MonoBehaviour, IPoolable
{
    [Header("If null, will use this transform")]
    [SerializeField] private Transform itemContainer;
    public Transform ItemContainer => this.itemContainer == null ? this.transform : this.itemContainer;

    [Header("Leave null to not highlight on valid drag over")]
    [SerializeField] private GameObject highlightObject;
    [Header("Leave null to not highlight on valid drag start")]
    [SerializeField] private GameObject isValidHighlight;

    public EDecorationType AcceptedDecorationTypes => this.acceptedDecorationTypes;
    [Space, SerializeField] private EDecorationType acceptedDecorationTypes = EDecorationType.All;

    public bool ShouldSnapToCenter => this.shouldSnapToCenter;
    [Space, SerializeField] private bool shouldSnapToCenter;

    public int MaxChildren => this.maxChildren;
    [Space, SerializeField] private int maxChildren = -1; // -1 for unlimited


    [Header("Optional Bounds")]
    public Collider2D BoundsCollider;
    [Tooltip("-1 = Disabled")]
    public float UnsnapRange = -1;

    [Space]
    public bool IsDragOutOfInventoryZone;
    public bool IsDragOverOpenInventoryZone;

    private Image baseImage;

    private void Awake()
    {
        if (this.itemContainer == null)
            this.itemContainer = this.transform;

        this.BoundsCollider = this.BoundsCollider == null ? GetComponent<Collider2D>() : this.BoundsCollider;

        this.baseImage = GetComponent<Image>();
        if (this.baseImage != null)
            this.baseImage.raycastTarget = false;

        SetHighlight(false);
        SetIsValidHighlight(false);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        UnregisterEvents();
        
        DragManager.OnDragStartedWithDecorationType += OnDragStartedWithDecorationType;
        DragManager.OnDragStarted += OnDragStarted;
        DragManager.OnDragEnded += OnDragEnded;
    }
    
    private void UnregisterEvents()
    {
        DragManager.OnDragStartedWithDecorationType -= OnDragStartedWithDecorationType;
        DragManager.OnDragStarted -= OnDragStarted;
        DragManager.OnDragEnded -= OnDragEnded;
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    public void OnSpawn()
    {
        RegisterEvents();
    }

    public void OnDespawn()
    {
        UnregisterEvents();
    }

    private void OnDragStartedWithDecorationType(EDecorationType decorationType)
    {
        var isValid = AllowsDecorationType(decorationType);
        SetIsValidHighlight(isValid);
    }
    
    private void OnDragStarted()
    {
        if (this.baseImage != null)
            this.baseImage.raycastTarget = true;
    }

    private void OnDragEnded()
    {
        if (this.baseImage != null)
            this.baseImage.raycastTarget = false;
            
        SetHighlight(false);
        SetIsValidHighlight(false);
    }

    public bool AllowsDecorationType(EDecorationType decorationType)
    {
        return this.AcceptedDecorationTypes == EDecorationType.All ||
            decorationType == EDecorationType.All ||
            (this.AcceptedDecorationTypes & decorationType) != 0;
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (this.highlightObject)
            this.highlightObject.SetActive(isHighlighted);
    }

    public void SetIsValidHighlight(bool isHighlighted)
    {
        //Debug.Log($"Setting valid highlight to {isHighlighted} for {gameObject.name}");
        if (this.isValidHighlight)
            this.isValidHighlight.SetActive(isHighlighted);
    }

    public void SetAsParent(Transform inChildObject)
    {
        inChildObject.SetParent(this.itemContainer, true);

        if (this.shouldSnapToCenter)
            inChildObject.localPosition = Vector3.zero;
    }

    public void SetChildPositions(Draggable inDraggable)
    {
        if (this.ShouldSnapToCenter)
        {
            // Calculate total width of all children
            float totalWidth = 0f;
            for (int i = 0; i < this.ItemContainer.childCount; i++)
            {
                var child = this.ItemContainer.GetChild(i);
                if (child.TryGetComponent<RectTransform>(out var rt))
                {
                    totalWidth += rt.rect.width * rt.localScale.x;
                }
            }

            var containerWidth = this.ItemContainer.GetComponent<RectTransform>().rect.width * this.ItemContainer.localScale.x;
            // Center children and space evenly within ItemContainer
            float spacing = (containerWidth - totalWidth) / (ItemContainer.childCount + 1);
            float currentX = -containerWidth / 2f + spacing;
            for (int i = 0; i < ItemContainer.childCount; i++)                
            {
                var child = ItemContainer.GetChild(i);
                if (child.TryGetComponent<RectTransform>(out var rt))                    
                {
                    float childWidth = rt.rect.width * rt.localScale.x;
                    child.localPosition = new Vector3(currentX + childWidth / 2f, 0f, 0f) + (inDraggable.SnapToCenterOffset * transform.localScale.y);
                    currentX += childWidth + spacing;
                }
            }
            
            foreach (Transform child in this.ItemContainer)
            {
                if(child.TryGetComponent<Draggable>(out var draggable))
                    draggable.SaveItemPosition();
            }
        }
        else
        {
            var targetParentCanvas = this.GetComponentInParent<Canvas>();
            var isTargetWorldCanvas = targetParentCanvas == UiManager.IN.WorldCanvas;

            var targetCanvas = isTargetWorldCanvas ? UiManager.IN.WorldCanvas : UiManager.IN.UICanvas;

            var dropPos = DragManager.GetDragPosition(Input.mousePosition, inDraggable.TargetRectTransform, targetCanvas);
            inDraggable.transform.position = dropPos + DragManager.OffsetFromCursor;
            inDraggable.SaveItemPosition();
        }
    }
}