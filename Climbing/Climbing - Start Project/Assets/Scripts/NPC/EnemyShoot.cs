using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyPlayer))]

public class EnemyShoot : WeaponController
{
    [SerializeField] float shootingSpeed;
    [SerializeField] float burstDurationMax;
    [SerializeField] float burstDurationMin;
    public static EnemyShoot es;
    EnemyPlayer enemyPlayer;
    bool shootFire;

    private void Start()
    {
        es = this;
        enemyPlayer = gameObject.GetComponent<EnemyPlayer>();
        enemyPlayer.OnTargetSelected += EnemyPlayer_OnTargetSelected;
    }

    private void EnemyPlayer_OnTargetSelected(StateManagerShoot target)
    {
        
            ActiveWeapon.AimTarget = target.transform;
            ActiveWeapon.AimTargetOffset = Vector3.up;
        if (target.stance <= 0)
        {
            ActiveWeapon.AimTargetOffset = Vector3.up * 0.5f;
            print("betha hua hai");
        }
        StartBurst();
        
    }
    void StartBurst()
    {
        if (!enemyPlayer.EnemyHealth.IsAlive)
            return;

        CheckReload();
        shootFire = true;

        GameManager.Instance.Timer.Add(EndBurst,UnityEngine.Random.Range(burstDurationMin,burstDurationMax));


    }
    void EndBurst()
    {
        shootFire = false;
        if (!enemyPlayer.EnemyHealth.IsAlive)
            return;

        CheckReload();

        GameManager.Instance.Timer.Add(StartBurst, shootingSpeed);
    }
    public bool gunreloading;
    void CheckReload()
    {
        if (ActiveWeapon.Reloader.RoundsremainingInClip > 2 && ActiveWeapon.Reloader.RoundsremainingInClip < 5)
        {
            gunreloading = true;
            ActiveWeapon.Reload();
        }
    }
    private void Update()
    {
        if (!shootFire || !CanFire || !enemyPlayer.EnemyHealth.IsAlive)
            return;

        ActiveWeapon.Fire();
    }
}
