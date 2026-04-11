using UnityEngine;
using TMPro;

public class UiInventoryCell : MonoBehaviour
{
    public static UiInventoryCell SelectedCell = null;

    public Transform Container => this.container;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject selectedOutline;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemQuantityText;

    public int CellIndex { get; set; }

    public UiInventoryItem Item { get; private set; }

    private void Awake()
    {
        SetSelected(false);
    }

    public void HandleClick()
    {
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            if (SelectedCell != null && SelectedCell != this)
            {
                SelectedCell.SetSelected(false);
            }
            SelectedCell = this;
        }
        else
        {
            if (SelectedCell == this)
            {
                SelectedCell = null;
            }
        }
        
        if (this.selectedOutline != null)
        {
            this.selectedOutline.SetActive(selected);
        }
    }

    public void ClearItem(bool destroyItem = true)
    {
        if (this.itemNameText != null)
            this.itemNameText.text = string.Empty;

        if (this.itemQuantityText != null)
            this.itemQuantityText.text = string.Empty;

        if (this.Item != null)
        {
            if (destroyItem)
                this.Item.Delete();
               
            this.Item = null;//not sure
        }
    }

    public void AddItem(UiInventoryItem inItem, InventoryItemData inItemData)
    {
        if (this.itemNameText)
            this.itemNameText.text = inItemData.DisplayName;

        if (this.itemQuantityText)
            this.itemQuantityText.text = inItemData.MaxStack > 1 ? $"{inItemData.Quantity}<size=80%>/{inItemData.MaxStack}</size>" : string.Empty;

        inItem.transform.localPosition = Vector3.zero;
        inItem.transform.localRotation = Quaternion.identity;
        inItem.transform.localScale = Vector3.one;

        this.Item = inItem;
        this.Item.Configure(inItemData);
    }

    public void SwapItems(UiInventoryCell otherCell, UiInventoryItem otherItem)
    {
        var thisItem = this.Item;
        var thisItemData = InventoryItemData.Copy(thisItem.ItemData);
        var otherItemData = InventoryItemData.Copy(otherItem.ItemData);

        if (otherCell.Container.TryGetComponent<DragTarget>(out var dragTarget))
        {
            dragTarget.SetAsParent(otherItem.transform);
        }

        AddItem(thisItem, otherItemData);
        otherCell.AddItem(otherItem, thisItemData);

        SaveManager.Data.InventoryItems[this.CellIndex] = this.Item.ItemData;
        SaveManager.Data.InventoryItems[otherCell.CellIndex] = otherItem.ItemData;
    }
    
    public void MergeItems(UiInventoryCell otherCell, UiInventoryItem otherItem)
    {
        if (otherCell.Container.TryGetComponent<DragTarget>(out var dragTarget))
        {
            dragTarget.SetAsParent(otherItem.transform);
        }

        var totalQuantity = this.Item.ItemData.Quantity + otherItem.ItemData.Quantity;
        int quantityInThisStack = Mathf.Min(totalQuantity, this.Item.ItemData.MaxStack);
        int quantityInOtherStack = totalQuantity - quantityInThisStack;

        if(this.Item.ItemData.Quantity == this.Item.ItemData.MaxStack)
        {
            //this stack is full, just swap items
            SwapItems(otherCell, otherItem);
            return;
        }

        var thisItemData = InventoryItemData.Copy(this.Item.ItemData);
        var otherItemData = InventoryItemData.Copy(otherItem.ItemData);
        
        thisItemData.Quantity = quantityInOtherStack;
        otherItemData.Quantity = quantityInThisStack;

        AddItem(this.Item, otherItemData);
        otherCell.AddItem(otherItem, thisItemData);

        if (quantityInOtherStack <= 0)
            otherCell.ClearItem(true);

        SaveManager.Data.InventoryItems[this.CellIndex] = this.Item.ItemData;
        SaveManager.Data.InventoryItems[otherCell.CellIndex] = otherItem.ItemData;
    }
}