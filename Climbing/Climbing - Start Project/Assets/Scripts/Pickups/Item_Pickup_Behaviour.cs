using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Pickup_Behaviour : MonoBehaviour
{
    public ItemsBase itemToPickup;
    Weapon_Manager wm;
    Text UItext;
    bool initItem;

    Weapon_Item wpToPickup;
    AmmoItem amItemToPick;

    public void Start()
    {
        UItext = CrosshairManager.GetInstance().pickItemText;
        wm = GetComponent<Weapon_Manager>();
        UItext.gameObject.SetActive(false);
    }
    public void Update()
    {
        CheckItemType();

        ActualPickup();
    }

    private void ActualPickup()
    {
        if (Input.GetKey(KeyCode.X))
        {
            WeaponActualPickup();
            AmmoItemActualPickup();
        }
    }

    private void AmmoItemActualPickup()
    {
        if (amItemToPick != null)
        {
            WeaponReferenceBase targetWeapon = wm.ReturnWeaponWithID(amItemToPick.weaponID);

            if (targetWeapon != null)
            {
                if (targetWeapon.carryingAmmo < targetWeapon.maxAmmo)
                {
                    targetWeapon.carryingAmmo += amItemToPick.ammoAmount;

                    if (targetWeapon.carryingAmmo > targetWeapon.maxAmmo)
                    {
                        targetWeapon.carryingAmmo = targetWeapon.maxAmmo;
                    }

                    GetComponent<Handle_Shooting>().carryingAmmo = targetWeapon.carryingAmmo;

                    Destroy(amItemToPick.gameObject);
                    amItemToPick = null;
                    itemToPickup = null;
                }
            }
        }
    }

    private void WeaponActualPickup()
    {
        if (wpToPickup != null)
        {
            WeaponReferenceBase targetWeapon = wm.ReturnWeaponWithID(wpToPickup.weaponID);

            if (targetWeapon != null)
            {
                wm.AvailableWeapons.Add(targetWeapon);
                if (wm.AvailableWeapons.Count > wm.maxWeapons )  //wm.maxweapons=2 
                {
                    WeaponReferenceBase prevWeapon = wm.ReturnCurrentWeapon();    

                    wm.AvailableWeapons.Remove(prevWeapon);

                    wm.SwitchWeaponWithTargetWeapon(targetWeapon);

                    if (prevWeapon.pickablePrefab != null)
                    {
                        Instantiate(prevWeapon.pickablePrefab,
                            (transform.position + transform.forward * 2 + Vector3.up),
                            Quaternion.Euler(0, 0, 90));
                    }
                }
            }
            Destroy(wpToPickup.gameObject);
            wpToPickup = null;
            itemToPickup = null;
        }
    }

    private void CheckItemType()
    {
        if (itemToPickup != null)
        {
            if (!initItem)
            {
                UItext.gameObject.SetActive(true);

                switch (itemToPickup.itemType)
                {
                    case ItemsBase.ItemType.weapon:
                        WeaponItemPickup();
                        break;

                    case ItemsBase.ItemType.ammo:
                        AmmoItemPickup();
                        break;

                    default:
                        break;
                }

                initItem = true;
            }
        }
        else
        {
            if (initItem)
            {
                initItem = false;
                wpToPickup = null;
                amItemToPick = null;
                UItext.gameObject.SetActive(false);
            }
        }
    }

    private void AmmoItemPickup()
    {
        amItemToPick = itemToPickup.GetComponent<AmmoItem>();

        WeaponReferenceBase forWp = wm.ReturnWeaponWithID(amItemToPick.weaponID);

        if (wm.AvailableWeapons.Contains(forWp))
        {
            if (forWp.carryingAmmo < forWp.maxAmmo)
            {
                UItext.text = "Press X to Pick Up Ammo For " + amItemToPick.weaponID;
            }
            else
            {
                UItext.text = "Ammo For " + amItemToPick.weaponID + " Is Full ";
            }
        }
        else
        {
            UItext.text = "Can't Pickup Ammo For " + amItemToPick.weaponID;
        }
    }

    private void WeaponItemPickup()
    {
        wpToPickup = itemToPickup.GetComponent<Weapon_Item>();

        string targetId = wpToPickup.weaponID;

        if (wm.AvailableWeapons.Count < wm.maxWeapons)
        {
            UItext.text = "Press X to Pick Up " + targetId;
        }
        else
        {
            UItext.text = "Press X to Switch " + wm.ReturnCurrentWeapon().weaponID + " with " + targetId;
        }
    }
}
