using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public static WeaponController wc;
    [SerializeField] float weaponSwitchTime;

    Shooter[] weapons;
    internal bool shotguninHand=false;
    int currentWeaponIndex;
    [HideInInspector]
    public bool CanFire;
    Transform weaponHolster;

    public event System.Action<Shooter> OnWeaponSwitched;

    Shooter m_ActiveWeapon;
    public Shooter ActiveWeapon
    {
        get
        {
            return m_ActiveWeapon;
        }
    }

    [System.Obsolete]
    private void Awake()
    {
        wc = this;

        CanFire = true;

        weaponHolster = transform.FindChild("Weapons");
        weapons = weaponHolster.GetComponentsInChildren<Shooter>(true);


        if (weapons.Length > 0)
        {
            Equip(0);
        }

    }
    void DeactivateWeapos()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
            weapons[i].transform.SetParent(weaponHolster);
        }
    }
    internal void SwitchWeapon(int direction)
    {
        CanFire = false;

        currentWeaponIndex += direction;
        if (currentWeaponIndex > weapons.Length - 1)
        {
            currentWeaponIndex = 0;
        }

        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = weapons.Length - 1;
        }
        GameManager.Instance.Timer.Add(() => {
            Equip(currentWeaponIndex);
        }, weaponSwitchTime);
        if (currentWeaponIndex == weapons.Length - 1)
            shotguninHand = true;
    }

   internal void Equip(int index)
    {
        DeactivateWeapos();
        CanFire = true;
        m_ActiveWeapon = weapons[index];
        m_ActiveWeapon.Equip();

        weapons[index].gameObject.SetActive(true);

        OnWeaponSwitched?.Invoke(m_ActiveWeapon);
    }
}
