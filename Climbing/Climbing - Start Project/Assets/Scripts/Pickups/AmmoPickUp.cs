using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : PickupItem
{
    [SerializeField] EWeaponType weaponType;
    [SerializeField] float respwanTime;
    [SerializeField] int amount;
    public override void OnPickup(Transform item)
    {
        var playerInventory=item.GetComponentInChildren<Container>();
        GameManager.Instance.Respawner.Despawn(gameObject, respwanTime);
           
        playerInventory.Put(weaponType.ToString(),amount);
        //item.GetComponent<Player>().PlayerShoot.ActiveWeapon.Reloader.HandleOnAmmoChanged();

    }
}
