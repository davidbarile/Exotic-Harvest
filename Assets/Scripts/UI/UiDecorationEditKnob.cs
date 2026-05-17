using UnityEngine;
using UnityEngine.EventSystems;

public class UiDecorationEditKnob : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform editRT;
    [SerializeField] private Transform corner;

    private Vector3 initialSize, initialScale, initialRotation;
    private Vector3 newScale, newRotation;

    private void Start()
    {
        this.initialSize = this.editRT.sizeDelta;
        this.initialScale = this.editRT.localScale;
        this.initialRotation = this.editRT.localEulerAngles;

        DragManager.OnDragModeChanged += OnDragModeChanged;
        this.transform.position = this.corner.position;

        
    }

    private void OnDestroy()
    {
        DragManager.OnDragModeChanged -= OnDragModeChanged;
    }

    private void OnDragModeChanged(bool inIsActive)
    {
        this.gameObject.SetActive(inIsActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }
}