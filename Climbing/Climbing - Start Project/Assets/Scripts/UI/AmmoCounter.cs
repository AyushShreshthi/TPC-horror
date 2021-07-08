using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField] Text text;

    PlayerShoot playerShoot;
    WeaponReloader reloader;
    private void Awake()
    {
        GameManager.Instance.OnLocalPlayerJoined += HandleOnLocalPLayerJoined;
    }

    private void HandleOnLocalPLayerJoined(PlayerForStieve player)
    {
        
        playerShoot = player.PlayerShoot;
        playerShoot.OnWeaponSwitched += HandleOnWeaponSwitched;
        //reloader = playerShoot.ActiveWeapon.Reloader;
        //reloader.OnAmmoChanged+=HandleOnAmmoChanged;

        //HandleOnAmmoChanged();

    }

    private void HandleOnWeaponSwitched(Shooter activeWeapon)
    {
        reloader = activeWeapon.Reloader;
        reloader.OnAmmoChanged += HandleOnAmmoChanged;
        HandleOnAmmoChanged();
    }

    private void HandleOnAmmoChanged()
    {
        int amountInInventory = reloader.RoundsRemainingInInventory;
        int amountInClip = reloader.RoundsremainingInClip;
        text.text = string.Format("{0}/{1}",amountInClip,amountInInventory);
    }

    
}
