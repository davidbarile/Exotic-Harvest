using UnityEngine;
using UnityEngine.UI;

public class LightningNode : MonoBehaviour
{
    [SerializeField] private RectTransform rectTrans;
    [SerializeField] private Image segment;
    [SerializeField] private Image glow;

    public Transform ChildAttachPoint => this.childAttachPoint;
    [SerializeField] private Transform childAttachPoint;

    public void Configure(float inSize, float inRotation, Transform inParent)
    {
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20);
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inSize);
        this.transform.rotation = Quaternion.Euler(0, 0, inRotation);
        this.transform.SetParent(inParent);
        this.transform.localPosition = Vector3.zero;
        this.gameObject.SetActive(true);
    }

    public void Reset()
    {
        this.gameObject.SetActive(false);
        this.glow.gameObject.SetActive(false);
        this.rectTrans.sizeDelta = new Vector2(20, 20);
    }
}