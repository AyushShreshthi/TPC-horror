using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Handle_Shooting : MonoBehaviour
{
    StateManagerShoot states;

    [HideInInspector]
    public Animator weaponAnim;
    [HideInInspector]
    public Animator modelAnim;
    [HideInInspector]
    public float fireRate;

    float timer;

    [HideInInspector]
    public Transform bulletSpawnPoint;
    [HideInInspector]
    public GameObject smokeParticle;
    [HideInInspector]
    public ParticleSystem[] muzzle;


    public GameObject casingPrefab; // as we have only one casing prefab

    [HideInInspector]
    public Transform caseSpawn;

    //Weapon_Manager weaponManager;

    public int magazineBullets = 0;
    public int curBullets = 30;
    public int carryingAmmo;
    private EnemyHealth m_EnemyHealth;
    public EnemyHealth EnemyHealth
    {
        get
        {
            if (m_EnemyHealth == null)
                m_EnemyHealth = GetComponent<EnemyHealth>();
            return m_EnemyHealth;
        }
    }
    public void Start()
    {
        states = GetComponent<StateManagerShoot>();
    }

    bool shoot;
    bool dontShoot;

    bool emptyGun;

    public void Update()
    {
        if (!states.meleeWeapon)
            Shooting();
    }

    private void Shooting()
    {
        shoot = states.shoot;

        if (modelAnim != null)
        {
            modelAnim.SetBool("Shoot", false);

            if (curBullets > 0)
            {
                modelAnim.SetBool("Empty", false);
            }
            else
            {
                modelAnim.SetBool("Empty", true);
            }
        }

        if (shoot)
        {
            if (timer <= 0)
            {
                if (modelAnim != null)
                {
                    modelAnim.SetBool("Shoot", false);
                }

                weaponAnim.SetBool("Shoot", false);

                if (curBullets > 0)
                {
                    emptyGun = false;
                    states.audioManager.PlayGunSound();

                    if (modelAnim != null)
                    {
                        modelAnim.SetBool("Shoot", true);
                    }

                    weaponAnim.SetBool("Shoot", true); //moved the ik animation here

                    GameObject go = Instantiate(casingPrefab, caseSpawn.position, caseSpawn.rotation) as GameObject;
                    Rigidbody rig = go.GetComponent<Rigidbody>();
                    rig.AddForce(transform.right.normalized * 1 + Vector3.up * 1f, ForceMode.Impulse);
                    rig.AddRelativeTorque(go.transform.right * 1.5f, ForceMode.Impulse);

                    states.actualShooting = true;

                    for (int i = 0; i < muzzle.Length; i++)
                    {
                        muzzle[i].Emit(1);
                    }

                    RaycastShoot();

                    curBullets -= 1;
                }
                else
                {
                    if (emptyGun)
                    {
                        if (carryingAmmo > 0)
                        {
                            states.handleAnimations.StartReload();

                            int targetBullets = 0;

                            if (magazineBullets < carryingAmmo)
                            {
                                targetBullets = magazineBullets;
                            }
                            else
                            {
                                targetBullets = carryingAmmo;
                            }

                            carryingAmmo -= targetBullets;

                            curBullets = targetBullets;

                            states.weaponManager.ReturnCurrentWeapon().weaponStats.curBullets = curBullets;
                            states.weaponManager.ReturnCurrentWeapon().carryingAmmo = carryingAmmo;

                        }
                        else
                        {
                            states.audioManager.PlayEffect("empty_gun");
                        }
                    }
                    else
                    {
                        states.audioManager.PlayEffect("empty_gun");
                        emptyGun = true;
                    }
                }
                timer = fireRate;

            }
            else
            {
                states.actualShooting = false;

                weaponAnim.SetBool("Shoot", false);

                timer -= states.myDelta;
            }
        }
        else
        {
            if (timer > 0)
                timer -= states.myDelta;
            else
                timer = 0;


            weaponAnim.SetBool("Shoot", false);

            states.actualShooting = false;
        }
    }

    private void RaycastShoot()
    {
         Vector3 direction = states.lookHitPosition - bulletSpawnPoint.position;  
        RaycastHit hit;

        if (Physics.Raycast(bulletSpawnPoint.position, direction, out hit, 100, states.layerMask))
        {

            GameObject go = Instantiate(smokeParticle, hit.point, Quaternion.identity) as GameObject;
            go.transform.LookAt(bulletSpawnPoint.position);
            Debug.DrawRay(bulletSpawnPoint.position, direction, Color.blue);

            
            if (hit.transform.gameObject.tag == "EnemyHead")
            {
                hit.transform.gameObject.GetComponentInParent<EnemyHealth>().Die(); 
                hit.transform.gameObject.GetComponentInParent<EnemyPlayer>().enabled=false;
                hit.transform.gameObject.GetComponentInParent<EnemyShoot>().enabled=false;
            }
            else if (hit.transform.gameObject.GetComponentInParent<EnemyPlayer>())
            {

                hit.transform.gameObject.GetComponentInParent<EnemyHealth>().health -= 1;
                if (hit.transform.gameObject.GetComponentInParent<EnemyHealth>().health == 0)
                {
                    hit.transform.gameObject.GetComponentInParent<EnemyPlayer>().enabled = false;
                    hit.transform.gameObject.GetComponentInParent<EnemyShoot>().enabled = false;

                }
                hit.transform.gameObject.GetComponentInParent<EnemyHealth>().getinCoverEnemy = true;

            }
             

            
            //if (hit.transform.GetComponent<ShootingRangeTarget>())
            //{
            //    hit.transform.GetComponent<ShootingRangetarget>().HitTarget();
            //}
        }
    }
}
