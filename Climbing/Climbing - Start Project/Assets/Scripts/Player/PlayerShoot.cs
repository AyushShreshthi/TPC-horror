using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerForStieve))]
public class PlayerShoot : WeaponController
{
    bool isPlayerAlive;
    private void Start()
    {
        isPlayerAlive = true;
        GetComponent<PlayerForStieve>().PlayerHealth.OnDeath += PlayerHealth_OnDeath;
    }

    private void PlayerHealth_OnDeath()
    {
        isPlayerAlive = false;
    }

    private void Update()
    {
        if (!isPlayerAlive)
            return; 
        
        if (GameManager.Instance.InputController.MouseWheelDown)
            SwitchWeapon(1);
        if (GameManager.Instance.InputController.MouseWheelUp)
            SwitchWeapon(-1);


        if (GameManager.Instance.LocalPlayer.PlayerState.MoveState== PlayerState.EMoveState.SPRINTING)
            return;

        if (!CanFire)
            return;
        if (GameManager.Instance.InputController.Fire1)
        {
            
            ActiveWeapon.Fire();
        }
        
    }
}
