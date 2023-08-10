using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelButton : MonoBehaviour
{
    [SerializeField] private Item associatedItem;

    private Button panelButton;
    private TextMeshProUGUI panelText;

    private void Awake()
    {
        panelButton = GetComponent<Button>();
        panelText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateAppearence(Item item)
    {
        if (item == null)
        {
            associatedItem = null;
            panelButton.interactable = false;
            panelText.text = string.Empty;
            panelButton.onClick.RemoveAllListeners();
        }
        else
        {
            associatedItem = item;
            panelButton.interactable = true;
            panelText.text = associatedItem.ObjectName;
            if (associatedItem.IsStackable)
            {
                panelText.text += " " + associatedItem.Quantity.ToString() + "x";
            }
            else if (associatedItem is Weapon weapon)
            {
                panelText.text += " - " + weapon.Durability.ToString();
            }
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(associatedItem.InventoryInteraction);
        }
    }
}