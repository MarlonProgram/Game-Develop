using UnityEngine;

[CreateAssetMenu(fileName = "Inventory Item Data", menuName = "Inventory System/Create Item")]
public class InventoryItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite itemIcon;
    public GameObject itemPrefab;
}
