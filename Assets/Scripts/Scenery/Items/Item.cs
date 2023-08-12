using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private string objectName;
    public string ObjectName { get => objectName; protected set => objectName = value; }
    [SerializeField] private string objectDescription;
    public string ObjectDescription { get => objectDescription; protected set => objectDescription = value; }
    //Para economizar espaço no inventário, ítens que não possuem durabilidade serão amontoados
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

    //A interação que ocorre caso o jogador aperte o botão no inventário
    //Alguns ítens não terão reação alguma.
    public virtual void InventoryInteraction()
    {
        InventoryController.RemoveItemFromInventory(this);
    }
}