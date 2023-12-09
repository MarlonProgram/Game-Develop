using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedEventCallback;

    private Dictionary<InventoryItemData, InventoryItem> _itemDictionary;
    public List<InventoryItem> inventory;

    public GameObject inventory_Bar;
    public bool inventory_Act;

    public GameObject selector;
    public int ID;

    private void Awake()
    {
        inventory = new List<InventoryItem>();
        _itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();

        Instance = this;
    }

    public void Add(InventoryItemData itemData)
    {
        if(_itemDictionary.TryGetValue(itemData, out InventoryItem value))
        {
            value.AddStack();

           
        }
        else
        {
            InventoryItem newItem = new InventoryItem(itemData);
            inventory.Add(newItem);
            _itemDictionary.Add(itemData, newItem);
            
        }
        onInventoryChangedEventCallback.Invoke();
    }

    public void Remove(InventoryItemData itemData)
    {
        if(_itemDictionary.TryGetValue(itemData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if(value.stackSize == 0)
            {
                inventory.Remove(value);
                _itemDictionary.Remove(itemData);
            }
        }

        onInventoryChangedEventCallback.Invoke();
    }

    public void Navigate()
    {

        if (inventory_Act)
        {
            inventory_Bar.SetActive(true);
            selector.SetActive(true);

            //selector.transform.position = 

            if (Input.GetKeyDown(KeyCode.LeftArrow) && ID > 0)
            {
                ID--;
            }
            if(Input.GetKeyDown(KeyCode.RightArrow) && ID < inventory.Count)
            {
                ID++;
            }

        }

        if (!inventory_Act)
        {
            inventory_Bar.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventory_Act = !inventory_Act;
        }

      

    }

    private void Update()
    {
        Navigate();
    }
}
