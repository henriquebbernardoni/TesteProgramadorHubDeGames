using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private string objectName;
    public string ObjectName { get => objectName; protected set => objectName = value; }
    [SerializeField] private string objectDescription;
    public string ObjectDescription { get => objectDescription; protected set => objectDescription = value; }
    //Para economizar espa�o no invent�rio, �tens que n�o possuem durabilidade ser�o amontoados
    [SerializeField] private bool isStackable;
    public bool IsStackable { get => isStackable; protected set => isStackable = value; }
    [SerializeField] private int quantity;
    public int Quantity { get => quantity; protected set => quantity = value; }

    public void AddQuantity()
    {
        quantity++;
    }
    public void RemoveQuantity()
    {
        quantity--;
    }

    //A intera��o que ocorre caso o jogador aperte o bot�o no invent�rio
    //Alguns �tens n�o ter�o rea��o alguma.
    public virtual void InventoryInteraction()
    {
        InventoryController.RemoveItemFromInventory(this);
    }
}