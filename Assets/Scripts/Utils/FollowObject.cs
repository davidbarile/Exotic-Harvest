using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool shouldOffsetForWorldCanvas, shouldOffsetForScreenCanvas;

    [SerializeField] private bool scaleUpByCanvasRatio, scaleDownByCanvasRatio;
   
    [Space, SerializeField] private bool shouldForceZeroDepth;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed = -1f;

    [SerializeField] private bool shouldScaleWithTarget;
    [SerializeField] private float scaleMultiplier = 1f;

    [SerializeField] private bool updateInEditor;

    private Vector3 originalScale;

    private RectTransform rectTrans;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (this.target != null && this.updateInEditor)
        {    
            LateUpdate();
        }
    }
#endif

    private void Start()
    {
        if (this.target == null)
            return;

        this.originalScale = this.transform.localScale;
        this.rectTrans = this.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (this.target != null)
        {
            var targetPosition = this.target.position;

            //Position
            if (this.shouldOffsetForWorldCanvas)
                targetPosition = TransformPositionToScreenSpace(targetPosition);
            else if (this.shouldOffsetForScreenCanvas)
                targetPosition = TransformPositionToWorldSpace(targetPosition);
            
            if (this.followSpeed <= 0f)
                this.transform.position = targetPosition;
            else
                this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, this.followSpeed * Time.deltaTime);
                
            this.transform.localPosition += this.offset;

            if (this.shouldForceZeroDepth)
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);

            //Scale
            if (this.shouldScaleWithTarget)
                this.transform.localScale = this.target.localScale * this.scaleMultiplier;

            if (this.scaleUpByCanvasRatio)
                this.transform.localScale = this.originalScale * DragManager.UiCanvasScaleFactor;
            else if (this.scaleDownByCanvasRatio)
                this.transform.localScale = this.originalScale / DragManager.UiCanvasScaleFactor;
        }
    }

    private Vector3 TransformPositionToScreenSpace(Vector3 inPosition)
    {
        if (UiManager.IN == null)
            return Vector3.zero;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(UiManager.IN.DragCanvas, inPosition, UiManager.IN.WorldCamera, out Vector3 outWorldPos))
            inPosition = outWorldPos;

        return inPosition;
    }

    private Vector3 TransformPositionToWorldSpace(Vector3 inPosition)
    {
        if (UiManager.IN == null)
            return Vector3.zero;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(UiManager.IN.WorldRectTrans, inPosition, UiManager.IN.DragCamera, out Vector3 outWorldPos))
            inPosition = outWorldPos;

        return inPosition;
    }
}