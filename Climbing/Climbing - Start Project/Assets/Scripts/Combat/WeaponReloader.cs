using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloader : MonoBehaviour
{
    [SerializeField] int maxAmmo;
    [SerializeField] float reloadTime;
    [SerializeField] int clipSize;
    [SerializeField] Container inventory;
    [SerializeField] EWeaponType weaponType;

    //int ammo;
     public int shotsFiredInClip;
    bool isReloading;
    System.Guid containerItemId;

    public event System.Action OnAmmoChanged;

    public int RoundsremainingInClip
    {
        get
        {
            return clipSize - shotsFiredInClip;
        }
    }
    public int RoundsRemainingInInventory
    {
        get
        {
            return inventory.GetAmountRemaining(containerItemId);
        }
    }
    public bool IsReloading
    {
        get
        {
            return isReloading;
        }
    }

    private void Awake()
    {
        inventory.OnContainerReady += () =>
        {
            containerItemId = inventory.Add(weaponType.ToString(), maxAmmo);
        };
       
    }
    public void Reload()
    {
        if (isReloading)
            return;

        isReloading = true;
       print("Reload started");
        GameManager.Instance.Timer.Add(()=>{
            ExecuteReload
                (inventory.TakeFromContainer(containerItemId, clipSize - RoundsremainingInClip));
            },reloadTime);

        
    }
    private void ExecuteReload(int amount)
    {
        print("reload executed!");
        isReloading = false;
         
        shotsFiredInClip -= amount;
        HandleOnAmmoChanged();

    }
    public void TakeFromClip(int amount)
    {
        shotsFiredInClip += amount;
        HandleOnAmmoChanged();
    }

    public void HandleOnAmmoChanged()
    {
        OnAmmoChanged?.Invoke();
    }
}
