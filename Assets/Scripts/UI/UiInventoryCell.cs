using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiInventoryCell : MonoBehaviour
{
    public Transform Container => container;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject selectedOutline;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemQuantityText;

    public void HandleClick()
    {
        SetSelected(true);
    }
    
    public void SetSelected(bool selected)
    {
        if (selectedOutline != null)
        {
            selectedOutline.SetActive(selected);
        }
    }
}