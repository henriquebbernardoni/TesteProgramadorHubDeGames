using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] private int durability;
    public int Durability { get => durability; protected set => durability = value; }

    public virtual IEnumerator WeaponBehaviour(SurvivorController attacker, ZombieController defender)
    {
        attacker.FullStop();
        yield return new WaitForEndOfFrame();
        Debug.Log(attacker.name);
        StartCoroutine(attacker.RechargeAttack());
        Durability--;
        if (Durability == 0)
        {
            attacker.SetWeapon(null);
            InventoryController.WeaponSelectButton.UpdateAppearence(null);
            WarningText.Instance.AddToWarningText(ObjectName + " quebrou!");
        }
    }

    public override void InventoryInteraction()
    {
        if (PlayerController.PC.GetWeapon())
        {
            if (PlayerController.PC.GetWeapon() == this)
            {
                PlayerController.PC.SetWeapon(null);
                InventoryController.AddItemToInventory(this);
            }
            else
            {
                InventoryController.AddItemToInventory(PlayerController.PC.GetWeapon());
                PlayerController.PC.SetWeapon(this);
                InventoryController.RemoveItemFromInventory(this);
            }
        }
        else
        {
            PlayerController.PC.SetWeapon(this);
            InventoryController.RemoveItemFromInventory(this);
        }
    }
}