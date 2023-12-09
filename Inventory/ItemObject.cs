using UnityEngine;

public class ItemObject : MonoBehaviour
{

    public InventoryItemData itemData;

    public void OnHandlePickUp()
    {
        InventorySystem.Instance.Add(itemData);
        Destroy(gameObject);
    }

}
