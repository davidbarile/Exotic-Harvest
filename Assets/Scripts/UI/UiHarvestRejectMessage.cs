using UnityEngine;
using TMPro;

public class UiHarvestRejectMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        Hide();
    }

    public void Show(string message, Vector3 inPosition,string title = "Invalid")
    {
        this.titleText.text = title;
        this.messageText.text = message;

        this.transform.position = inPosition;
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (this == null || this.gameObject == null) return;
        
        this.gameObject.SetActive(false);
    }
}