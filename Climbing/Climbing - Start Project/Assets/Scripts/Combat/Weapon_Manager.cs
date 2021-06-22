using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TPC
{
    public class Weapon_Manager : MonoBehaviour
    {
        public int maxWeapons = 2;
        public List<WeaponReferenceBase> AvailableWeapons = new List<WeaponReferenceBase>();

        public int weaponIndex;
        public List<WeaponReferenceBase> Weapons = new List<WeaponReferenceBase>();
        WeaponReferenceBase currentWeapon;
        IKHandler ikHandler;
        Handle_Shooting handleShooting;
        [HideInInspector]
        public StateManager states;
        CharacterAudioManager audioManager;

        public WeaponReferenceBase unarmed;
        public WeaponReferenceBase downWeapon;

        public bool startUnarmed;
        public void Start()
        {
            states = GetComponent<StateManager>();
            ikHandler = GetComponent<IKHandler>();
            handleShooting = GetComponent<Handle_Shooting>();
            audioManager = GetComponent<CharacterAudioManager>();

            AvailableWeapons.Add(Weapons[weaponIndex]);
            weaponIndex = 0;

            CloseAllWeapons();
            SwitchWeapon(weaponIndex);

            unarmed.animType = 10;
            unarmed.meleeWeapon = true;

            if (startUnarmed)
                SwitchWeaponWithTargetWeapon(unarmed);
        }
        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                if (weaponIndex < AvailableWeapons.Count - 1)
                {
                    weaponIndex++;
                }
                else
                {
                    weaponIndex = 0;
                }
                SwitchWeapon(weaponIndex);
            }

            //test   update with input handler(dangerous for enemy)
            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    if (!states.meleeWeapon)
            //    {
            //        SwitchWeaponWithTargetWeapon(unarmed);
            //        states.meleeWeapon = true;
            //    }
            //    else
            //    {
            //        SwitchWeaponWithTargetWeapon(AvailableWeapons[weaponIndex]);
            //        states.meleeWeapon = false;
            //    }
            //}

            foreach (WeaponReferenceBase w in Weapons)
            {
                if (w.holsterWeapon)
                {
                    if (w.holsterWeapon.activeInHierarchy && !AvailableWeapons.Contains(w))
                    {
                        w.holsterWeapon.SetActive(false);
                    }
                }
            }

        }
        public void SwitchWeapon(int desiredIndex)
        {
            if (desiredIndex > AvailableWeapons.Count - 1)
            {
                desiredIndex = 0;
                weaponIndex = 0;
            }

            WeaponReferenceBase targetWeapon = ReturnWeaponWithID(AvailableWeapons[desiredIndex].weaponID);

            SwitchWeaponWithTargetWeapon(targetWeapon);

            weaponIndex = desiredIndex;
        }

        public void SwitchWeaponWithTargetWeapon(WeaponReferenceBase targetWeapon)
        {
            if (currentWeapon != null)
            {
                if (currentWeapon.weaponModel != null)
                {
                    currentWeapon.weaponModel.SetActive(false);
                    if (currentWeapon.ikHolder)
                    {
                        currentWeapon.ikHolder.SetActive(false);
                    }
                }
                if (currentWeapon.holsterWeapon)
                {
                    currentWeapon.holsterWeapon.SetActive(true);
                }
            }

            WeaponReferenceBase newWeapon = targetWeapon;     //test


            if (newWeapon.holsterWeapon)
            {
                newWeapon.holsterWeapon.SetActive(false);
            }

            if (newWeapon.rightHandTarget)
            {
                ikHandler.rightHandIKTarget = newWeapon.rightHandTarget;
            }
            else
            {
                ikHandler.rightHandIKTarget = null;
            }
            if (newWeapon.leftHandTarget)
            {
                ikHandler.leftHandIKTarget = newWeapon.leftHandTarget;
            }
            else
            {
                ikHandler.leftHandIKTarget = null;
            }

            if (newWeapon.lookTarget)
            {
                ikHandler.overrideLookTarget = newWeapon.lookTarget;
            }
            else
            {
                ikHandler.overrideLookTarget = null;
            }

            if (newWeapon.modelAnimator)
            {
                handleShooting.modelAnim = newWeapon.modelAnimator;
            }
            else
            {
                handleShooting.modelAnim = null;
            }

            if (newWeapon.leftElbowTarget) { ikHandler.leftElbowTarget = newWeapon.leftElbowTarget; }
            else { ikHandler.leftElbowTarget = null; }

            if (newWeapon.rightElbowTarget) { ikHandler.rightElbowTarget = newWeapon.rightElbowTarget; }
            else { ikHandler.rightElbowTarget = null; }

            ikHandler.LHIK_dis_notAiming = newWeapon.dis_LHIK_notAiming;

            if (newWeapon.dis_LHIK_notAiming)
                ikHandler.leftHandIKWeight = 0;

            handleShooting.fireRate = newWeapon.weaponStats.fireRate;

            if (newWeapon.ikHolder)
            {
                handleShooting.weaponAnim = newWeapon.ikHolder.GetComponent<Animator>();
            }
            else
            {
                handleShooting.weaponAnim = null;
            }
            handleShooting.bulletSpawnPoint = newWeapon.bulletSpawner;
            handleShooting.curBullets = newWeapon.weaponStats.curBullets;
            handleShooting.magazineBullets = newWeapon.weaponStats.maxBullets;
            handleShooting.caseSpawn = newWeapon.casingSpawner;
            handleShooting.muzzle = newWeapon.muzzle;

            audioManager.gunSounds.clip = newWeapon.weaponStats.shootSound;

            handleShooting.carryingAmmo = newWeapon.carryingAmmo;

            if (newWeapon.weaponModel)
            {
                newWeapon.weaponModel.SetActive(true);
            }
            if (newWeapon.ikHolder)
            {
                newWeapon.ikHolder.SetActive(true);
            }

            //states.weaponAnimType = newWeapon.animType;
            //states.meleeWeapon = newWeapon.meleeWeapon;

            currentWeapon = newWeapon;      //test
        }
        private void CloseAllWeapons()
        {
            for (int i = 0; i < Weapons.Count; i++)
            {
                //ParticleSystem[] muzzleParticles = Weapons[i].weaponModel.GetComponentsInChildren<ParticleSystem>();
                //Weapons[i].muzzle = muzzleParticles;
                if (Weapons[i].weaponModel)
                {
                    Weapons[i].weaponModel.SetActive(false);
                    if (Weapons[i].ikHolder)
                    {
                        Weapons[i].ikHolder.SetActive(false);
                    }

                    if (Weapons[i].holsterWeapon)
                        Weapons[i].holsterWeapon.SetActive(false);
                }
            }
        }

        public WeaponReferenceBase ReturnWeaponWithID(string weaponID)
        {
            WeaponReferenceBase retVal = null;

            for (int i = 0; i < Weapons.Count; i++)
            {
                if (string.Equals(Weapons[i].weaponID, weaponID))
                {
                    retVal = Weapons[i];
                    break;
                }
            }
            return retVal;
        }

        public WeaponReferenceBase ReturnCurrentWeapon()
        {
            return currentWeapon;
        }

        public List<Sharable_Obj> PopulateAndReturnSharableList()
        {
            List<Sharable_Obj> retVal = new List<Sharable_Obj>();

            Sharable_Obj[] objs = GetComponentsInChildren<Sharable_Obj>();

            foreach (Sharable_Obj o in objs)
            {
                retVal.Add(o);
            }

            foreach (WeaponReferenceBase w in Weapons)
            {
                if (w.holsterWeapon)
                {
                    bool wasActive = w.holsterWeapon.activeInHierarchy;

                    w.holsterWeapon.SetActive(true);

                    if (w.holsterWeapon.GetComponent<Sharable_Obj>())
                    {
                        Sharable_Obj o = w.holsterWeapon.GetComponent<Sharable_Obj>();

                        if (!retVal.Contains(o))
                        {
                            retVal.Add(o);
                        }
                    }

                    if (!wasActive)
                    {
                        w.holsterWeapon.SetActive(false);
                    }
                }

                if (w.weaponModel)
                {
                    bool wasActive = w.weaponModel.activeInHierarchy;

                    w.weaponModel.SetActive(true);

                    if (w.weaponModel.GetComponent<Sharable_Obj>())
                    {
                        Sharable_Obj o = w.weaponModel.GetComponent<Sharable_Obj>();

                        if (!retVal.Contains(o))
                        {
                            retVal.Add(o);
                        }
                    }
                    if (!wasActive)
                    {
                        w.weaponModel.SetActive(false);
                    }
                }
            }
            return retVal;
        }
    }

    [System.Serializable]
    public class WeaponReferenceBase
    {
        public string weaponID;
        public GameObject weaponModel;
        public Animator modelAnimator;
        public GameObject ikHolder;
        public Transform rightHandTarget;
        public Transform leftHandTarget;
        public Transform lookTarget;
        public ParticleSystem[] muzzle;
        public Transform bulletSpawner;
        public Transform casingSpawner;
        public WeaponStats weaponStats;
        public Transform rightElbowTarget;
        public Transform leftElbowTarget;
        public int animType;

        public bool dis_LHIK_notAiming;

        public int carryingAmmo = 60;
        public int maxAmmo = 60;
        public GameObject pickablePrefab;

        public GameObject holsterWeapon;
        public bool meleeWeapon = false;
    }
    [System.Serializable]
    public class WeaponStats
    {
        public int curBullets;
        public int maxBullets;
        public float fireRate;
        public AudioClip shootSound;
        //etc
    }
}
