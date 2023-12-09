using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public InventorySystem system;

    private void Start()
    {
        system.onInventoryChangedEventCallback += OnUpdateInventory;
    }

    public void OnUpdateInventory()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.transform.gameObject);
        }

        OnDrawInventory();
    }

    public void OnDrawInventory()
    {
        foreach(InventoryItem item in system.inventory)
        {
            AddInventorySlot(item);
        }
    }

    public void AddInventorySlot(InventoryItem item)
    {
        GameObject obj = Instantiate(itemSlotPrefab);
        obj.transform.SetParent(transform, false);

        ItemSlot slot = obj.GetComponent<ItemSlot>();
        slot.Set(item);
    }
}
