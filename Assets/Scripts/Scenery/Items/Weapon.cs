using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] private int durability;

    public int Durability { get => durability; protected set => durability = value; }

    public override void InventoryInteraction()
    {
        if (inventoryController.PlayerCharacter.GetWeapon())
        {
            if (inventoryController.PlayerCharacter.GetWeapon() == this)
            {
                inventoryController.PlayerCharacter.SetWeapon(null);
                inventoryController.AddItemToInventory(this);
            }
            else
            {
                inventoryController.AddItemToInventory(inventoryController.PlayerCharacter.GetWeapon());
                inventoryController.PlayerCharacter.SetWeapon(this);
                inventoryController.RemoveItemFromInventory(this);
            }
        }
        else
        {
            inventoryController.PlayerCharacter.SetWeapon(this);
            inventoryController.RemoveItemFromInventory(this);
        }
    }

    public virtual IEnumerator WeaponBehaviour(SurvivorController attacker, ZombieController defender)
    {
        attacker.FullStop();
        yield return new WaitForEndOfFrame();
        StartCoroutine(attacker.RechargeAttack());
        Durability--;
        if (Durability == 0)
        {
            attacker.SetWeapon(null);
            inventoryController.WeaponSelectButton.UpdateAppearence(null);
            WarningText.Instance.AddToWarningText(ObjectName + " quebrou!");
        }
    }
}