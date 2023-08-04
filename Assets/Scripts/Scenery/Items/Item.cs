using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private string objectName;
    [SerializeField] private string objectDescription;
    //Para economizar espaço no inventário, ítens que não possuem durabilidade serão amontoados
    [SerializeField] private bool isStackable;
    private int quantity;

    public string ObjectName { get => objectName; protected set => objectName = value; }
    public string ObjectDescription { get => objectDescription; protected set => objectDescription = value; }
    public bool IsStackable { get => isStackable; protected set => isStackable = value; }
    public int Quantity { get => quantity; protected set => quantity = value; }

    protected InventoryController inventoryController;

    private void Awake()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    //A interação que ocorre caso o jogador aperte o botão no inventário
    //Alguns ítens não terão reação alguma.
    public virtual void InventoryInteraction()
    {
        Debug.Log(objectName);
        inventoryController.RemoveItemFromInventory(this);
    }

    public void AddQuantity()
    {
        quantity++;
    }
    public void RemoveQuantity()
    {
        quantity--;
    }
}