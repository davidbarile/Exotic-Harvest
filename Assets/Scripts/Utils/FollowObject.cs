using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool shouldOffsetForWorldCanvas;
    [SerializeField] private bool shouldForceZeroDepth;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed = -1f;

    [SerializeField] private bool shouldScaleWithTarget;
    [SerializeField] private float scaleMultiplier = 1f;

    [SerializeField] private bool updateInEditor;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (this.target != null && this.updateInEditor)
        {    
            Update();
        }
    }
#endif

    private void Update()
    {
        if (this.target != null)
        {
            if (this.followSpeed <= 0f)
            {
                this.transform.position = this.target.position;
            }
            else
            {
                this.transform.position = Vector3.Lerp(this.transform.position, this.target.position, this.followSpeed * Time.deltaTime);
            }

            this.transform.localPosition += this.offset;

            if(this.shouldOffsetForWorldCanvas)
                this.transform.localPosition -= DragManager.ScreenToWorldCameraDelta;

            if (this.shouldForceZeroDepth)
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);

            if (this.shouldScaleWithTarget)
                this.transform.localScale = this.target.localScale * this.scaleMultiplier;
        }
    }
}