using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiInventoryCell : MonoBehaviour
{
    public Transform Container => this.container;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject selectedOutline;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemQuantityText;
    private void Start()
    {
        SetSelected(false);
    }

    public void HandleClick()
    {
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        if (this.selectedOutline != null)
        {
            this.selectedOutline.SetActive(selected);
        }
    }
    
    public void AddItem(UiInventoryItem item, int quantity)
    {
        if (itemNameText != null)
        {
            itemNameText.text = item.name;
        }

        if (itemQuantityText != null)
        {
            itemQuantityText.text = quantity.ToString();
        }
    }
}