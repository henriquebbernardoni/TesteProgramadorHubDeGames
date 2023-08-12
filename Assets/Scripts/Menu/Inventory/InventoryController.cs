using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static List<Item> InventoryItems { get; private set; }

    [SerializeField] private GameObject inventoryPanel;
    private static GameObject _inventoryPanel;
    [SerializeField] private InventoryPanelButton[] inventoryButtons;
    private static InventoryPanelButton[] _inventoryButtons;
    [SerializeField] private InventoryPanelButton weaponSelectButton;
    private static InventoryPanelButton _weaponSelectButton;
    public static InventoryPanelButton WeaponSelectButton
        { get => _weaponSelectButton; private set => _weaponSelectButton = value; }

    private void Awake()
    {
        InventoryItems = new List<Item>();
        _inventoryPanel = inventoryPanel;
        _inventoryButtons = inventoryButtons;
        _weaponSelectButton = weaponSelectButton;
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
    public static void AddItemToInventory(Item item)
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
        if (item.GetType() == typeof(Supply))
        {
            SupplyCounter.Instance.UpdateText();
        }

        if (_inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryPanel();
        }
    }

    public static void RemoveItemFromInventory(Item item)
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

        if (_inventoryPanel.activeInHierarchy)
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

    private static void UpdateInventoryPanel()
    {
        for (int i = 0; i < _inventoryButtons.Length; i++)
        {
            if (i >= InventoryItems.Count)
            {
                _inventoryButtons[i].UpdateAppearence(null);
            }
            else
            {
                _inventoryButtons[i].UpdateAppearence(InventoryItems[i]);
            }
        }

        if (PlayerController.PC.GetWeapon())
        {
            _weaponSelectButton.UpdateAppearence(PlayerController.PC.GetWeapon());
        }
        else
        {
            _weaponSelectButton.UpdateAppearence(null);
        }
    }
}