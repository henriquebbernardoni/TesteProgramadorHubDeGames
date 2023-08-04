using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private string objectName;
    [SerializeField] private string objectDescription;
    //Para economizar espa�o no invent�rio, �tens que n�o possuem durabilidade ser�o amontoados
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

    //A intera��o que ocorre caso o jogador aperte o bot�o no invent�rio
    //Alguns �tens n�o ter�o rea��o alguma.
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