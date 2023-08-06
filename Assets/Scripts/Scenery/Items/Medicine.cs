using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medicine : Item
{
    public override void InventoryInteraction()
    {
        inventoryController.PlayerController.ModifyHealth(2);
    }
}