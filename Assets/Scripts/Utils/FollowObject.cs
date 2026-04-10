using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed = -1f;

    [SerializeField] private bool shouldScaleWithTarget;
    [SerializeField] private float scaleMultiplier = 1f;

    [SerializeField] private bool updateInEditor;
    private Vector3 initialScale;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (this.target != null && this.updateInEditor)
        {
            this.initialScale = transform.localScale;
            Update();
        }
    }
#endif

    private void Start()
    {
        this.initialScale = transform.localScale;
    }

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

            if (this.shouldScaleWithTarget)
            {
                this.transform.localScale = Vector3.Scale(this.initialScale, this.target.localScale) * this.scaleMultiplier;
            }
        }
    }
}