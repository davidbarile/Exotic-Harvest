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

    public void AddItem(UiInventoryItem item, InventoryItemData itemData)
    {
        if (this.itemNameText != null)
        {
            this.itemNameText.text = itemData.DisplayName;
        }

        if (this.itemQuantityText != null)
        {
            this.itemQuantityText.text = itemData.Quantity > 1 ? itemData.Quantity.ToString() : string.Empty;
        }

        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        this.Item = item;
        this.Item.Configure(itemData);
    }

    public void SwapItems(UiInventoryCell otherCell, UiInventoryItem otherItem)
    {
        var thisItem = this.Item;
        var thisItemData = InventoryItemData.Copy(thisItem.ItemData);
        var otherItemData = InventoryItemData.Copy(otherItem.ItemData);

        if (otherCell.Container.TryGetComponent<UiDragTarget>(out var dragTarget))
        {
            dragTarget.SetAsParent(otherItem.transform);
        }

        AddItem(thisItem, otherItemData);
        otherCell.AddItem(otherItem, thisItemData);

        SaveManager.Data.AllInventoryItems[this.CellIndex] = this.Item.ItemData;
        SaveManager.Data.AllInventoryItems[otherCell.CellIndex] = otherItem.ItemData;
    }
    
    public void MergeItems(UiInventoryCell otherCell, UiInventoryItem otherItem)
    {
        if (otherCell.Container.TryGetComponent<UiDragTarget>(out var dragTarget))
        {
            dragTarget.SetAsParent(otherItem.transform);
        }

        var totalQuantity = this.Item.ItemData.Quantity + otherItem.ItemData.Quantity;
        int quantityInThisStack = Mathf.Min(totalQuantity, this.Item.ItemData.QuantityPerStack);
        int quantityInOtherStack = totalQuantity - quantityInThisStack;

        if(this.Item.ItemData.Quantity == this.Item.ItemData.QuantityPerStack)
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

        SaveManager.Data.AllInventoryItems[this.CellIndex] = this.Item.ItemData;
        SaveManager.Data.AllInventoryItems[otherCell.CellIndex] = otherItem.ItemData;
    }
}