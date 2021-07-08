using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsBase : MonoBehaviour
{
    public ItemType itemType;

    public enum ItemType
    {
        weapon,
        health,
        ammo,
        etc
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<Item_Pickup_Behaviour>())
            other.transform.GetComponent<Item_Pickup_Behaviour>().itemToPickup = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.GetComponent<Item_Pickup_Behaviour>())
            other.transform.GetComponent<Item_Pickup_Behaviour>().itemToPickup = null;
    }
}
