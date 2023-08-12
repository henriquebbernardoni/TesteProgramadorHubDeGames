using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SupplyCounter : MonoBehaviour
{
    public static SupplyCounter Instance { get; private set; }

    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        Instance = this;
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameController.OnLevelSetUp += UpdateText;
    }

    private void OnDisable()
    {
        GameController.OnLevelSetUp -= UpdateText;
    }

    public void UpdateText()
    {
        SurvivorController[] survivorsRescued = GameController.Survivors.
            Where(x => x.GetState() != SurvivorController.SurvivorState.DEATH
            || x.GetState() != SurvivorController.SurvivorState.RESCUE).ToArray();
        Supply supply = InventoryController.InventoryItems.OfType<Supply>().FirstOrDefault();

        if (supply != null)
        {
            if (supply.Quantity < survivorsRescued.Length / 2)
            {
                textMeshPro.text = "Você precisa de mais " +
                    ((survivorsRescued.Length / 2) - supply.Quantity).ToString() + " Suprimentos";
            }
            else
            {
                textMeshPro.text = "Você tem a quantidade necessária de suprimentos!";
            }
        }
        else
        {
            textMeshPro.text = "Você precisa de mais " +
                (survivorsRescued.Length / 2).ToString() + " Suprimentos";
        }
    }
}