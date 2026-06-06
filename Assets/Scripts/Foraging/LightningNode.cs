using UnityEngine;
using UnityEngine.UI;

public class LightningNode : MonoBehaviour
{
    [SerializeField] private RectTransform rectTrans;
    [SerializeField] private Image segment;
    [SerializeField] private Image glow;

    public Transform ChildAttachPoint => this.childAttachPoint;
    [SerializeField] private Transform childAttachPoint;

    public void Reset()
    {
        this.gameObject.SetActive(false);
        this.glow.gameObject.SetActive(false);
        this.rectTrans.sizeDelta = new Vector2(20, 20);
    }
}