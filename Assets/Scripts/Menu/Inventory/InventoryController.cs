using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private List<Item> inventoryItems = new();

    [SerializeField] private InventoryPanelButton[] inventoryButtons;
    [SerializeField] private InventoryPanelButton weaponSelectButton;
    [SerializeField] private GameObject inventoryPanel;

    [SerializeField] private SurvivorController playerCharacter;

    public InventoryPanelButton WeaponSelectButton { get => weaponSelectButton; private set => weaponSelectButton = value; }
    public SurvivorController PlayerCharacter { get => playerCharacter; private set => playerCharacter = value; }
    public List<Item> InventoryItems { get => inventoryItems; private set => inventoryItems = value; }

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
            for (int i = 0; i < InventoryItems.Count; i++)
            {
                if (InventoryItems[i].GetType() == item.GetType())
                {
                    firstItem = InventoryItems[i];
                    break;
                }
            }
            if (firstItem == null)
            {
                InventoryItems.Add(item);
                item.AddQuantity();
            }
            else
            {
                firstItem.AddQuantity();
            }
        }
        else
        {
            InventoryItems.Add(item);
        }

        LevelController.Instance.AddToCollectedItems();

        if (inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryPanel();
        }
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (item.IsStackable)
        {
            Item firstItem = null;
            for (int i = 0; i < InventoryItems.Count; i++)
            {
                if (InventoryItems[i].GetType() == item.GetType())
                {
                    firstItem = InventoryItems[i];
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
                    InventoryItems.Remove(item);
                }
            }
        }
        else
        {
            if (!InventoryItems.Contains(item))
            {
                return;
            }
            InventoryItems.Remove(item);
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
        if (inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryPanel();
        }
    }

    private void UpdateInventoryPanel()
    {
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            if (i >= InventoryItems.Count)
            {
                inventoryButtons[i].UpdateAppearence(null);
            }
            else
            {
                inventoryButtons[i].UpdateAppearence(InventoryItems[i]);
            }
        }

        if (PlayerCharacter.GetWeapon())
        {
            weaponSelectButton.UpdateAppearence(PlayerCharacter.GetWeapon());
        }
        else
        {
            weaponSelectButton.UpdateAppearence(null);
        }
    }

    public void SetPlayerCharacter(SurvivorController survivor)
    {
        PlayerCharacter = survivor;
    }
}