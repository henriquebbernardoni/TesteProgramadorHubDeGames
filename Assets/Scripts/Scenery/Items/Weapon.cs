using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] private int durability;

    public int Durability { get => durability; protected set => durability = value; }

    public override void InventoryInteraction()
    {
        if (inventoryController.PlayerController.GetWeapon())
        {
            if (inventoryController.PlayerController.GetWeapon() == this)
            {
                inventoryController.PlayerController.SetWeapon(null);
                inventoryController.AddItemToInventory(this);
            }
            else
            {
                inventoryController.AddItemToInventory(inventoryController.PlayerController.GetWeapon());
                inventoryController.PlayerController.SetWeapon(this);
                inventoryController.RemoveItemFromInventory(this);
            }
        }
        else
        {
            inventoryController.PlayerController.SetWeapon(this);
            inventoryController.RemoveItemFromInventory(this);
        }
    }

    public virtual IEnumerator WeaponBehaviour(SurvivorController attacker, ZombieController defender)
    {
        yield return new WaitForEndOfFrame();
        StartCoroutine(attacker.RechargeAttack());
        Durability--;
        if (Durability == 0)
        {
            attacker.SetWeapon(null);
            inventoryController.WeaponSelectButton.UpdateAppearence(null);
            WarningText.Instance.SetWarningText(ObjectName + " quebrou!");
        }
    }
}