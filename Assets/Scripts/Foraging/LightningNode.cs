using UnityEngine;
using UnityEngine.UI;

public class LightningNode : MonoBehaviour
{
    public LightningBolt LightningBolt { get; private set; }
    public int NumBranches { get; private set; }
    [SerializeField] private RectTransform rectTrans;
    [SerializeField] private Image segment;
    [SerializeField] private Image glow;

    private readonly int width = 6;

    public Transform ChildAttachPoint => this.childAttachPoint;
    [SerializeField] private Transform childAttachPoint;

    public void Configure(LightningBolt inLightningBolt, float inSize, float inRotation, Transform inParent, int inNumBranches, bool inShouldShow)
    {
        this.LightningBolt = inLightningBolt;
        this.NumBranches = inNumBranches;
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.width);
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inSize);
        this.transform.rotation = Quaternion.Euler(0, 0, inRotation);
        this.transform.SetParent(inParent);
        this.transform.localPosition = Vector3.zero;
        this.gameObject.SetActive(inShouldShow);
        this.glow.gameObject.SetActive(true);
    }

    public void Reset()
    {
        this.NumBranches = 1;
        this.gameObject.SetActive(false);
        this.glow.gameObject.SetActive(false);
        this.transform.rotation = Quaternion.identity;
        this.transform.localPosition = Vector3.zero;
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.width);
        this.rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
    }
}