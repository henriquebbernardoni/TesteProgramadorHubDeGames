using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private List<Item> inventoryItems;

    [SerializeField] private InventoryPanelButton[] inventoryButtons;
    [SerializeField] private GameObject inventoryPanel;

    private PlayerController playerController;

    private void Awake()
    {
        inventoryItems = new List<Item>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        SetPanelActiveInactive();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetPanelActiveInactive();
        }
    }

    //Essas duas funções tem que ser usadas para adequadamente colocar e retirar itens.
    public void AddItemToInventory(Item item)
    {
        if (item.IsStackable)
        {
            Item firstItem = null;
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].GetType() ==  item.GetType())
                {
                    firstItem = inventoryItems[i];
                    break;
                }
            }
            if (firstItem == null)
            {
                inventoryItems.Add(item);
                item.AddQuantity();
            }
            else
            {
                firstItem.AddQuantity();
            }
        }
        else
        {
            inventoryItems.Add(item);
        }
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (item.IsStackable)
        {
            Item firstItem = null;
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].GetType() == item.GetType())
                {
                    firstItem = inventoryItems[i];
                    break;
                }
            }
            if (firstItem == null)
            {
                return;
            }
            else
            {
                item.RemoveQuantity();
                if (item.Quantity == 0)
                {
                    inventoryItems.Remove(item);
                }
            }
        }
        else
        {
            if (!inventoryItems.Contains(item))
            {
                return;
            }
            inventoryItems.Remove(item);
        }

        if (inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryPanel();
        }
    }

    public void SetPanelActiveInactive()
    {
        if (inventoryPanel.activeInHierarchy)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }

        inventoryPanel.SetActive(!inventoryPanel.activeInHierarchy);
        UpdateInventoryPanel();
    }

    private void UpdateInventoryPanel()
    {
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            if (i >= inventoryItems.Count)
            {
                inventoryButtons[i].UpdateAppearence(null);
            }
            else
            {
                inventoryButtons[i].UpdateAppearence(inventoryItems[i]);
            }
        }
    }
}