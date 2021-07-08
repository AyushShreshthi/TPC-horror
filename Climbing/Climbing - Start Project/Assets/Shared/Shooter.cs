using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] float rateOfFire;
    [SerializeField] Projectile projectile;
    [SerializeField] Transform hand;
    [SerializeField] AudioController audioReload, audioFire;


    public Transform AimTarget;
    public Vector3 AimTargetOffset;


    public static Shooter st;
    public WeaponReloader Reloader;
    private ParticleSystem muzzleFireParSystem;

    float nextFireAllowed;
    public bool canFire;
   public  Transform muzzle;
    public  void Equip()
    {
        st = this;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
       
    }
    
    private void Awake()
    {
        muzzle = transform.Find("Model/Muzzle");
        Reloader = GetComponent<WeaponReloader>();
        muzzleFireParSystem = muzzle.GetComponent<ParticleSystem>();

    }
    public void Reload()
    {
        if (Reloader == null)
            return;
        Reloader.Reload();
        audioReload.Play();
    }
    void FireEffect()
    {
        if (muzzleFireParSystem == null)
            return;
        muzzleFireParSystem.Play();
    }

    public virtual void Fire()
    {
        
        canFire = false;
        if (Time.time < nextFireAllowed)
            return;

        if(Reloader!=null)
        {
            if (Reloader.IsReloading)
                return;
            if (Reloader.RoundsremainingInClip == 0)
                return;
            Reloader.TakeFromClip(1);
            
        }
        nextFireAllowed = Time.time + rateOfFire;


       // muzzle.LookAt((AimTarget.position+Vector3.right) + AimTargetOffset);
        FireEffect();

        //instantiate the projectile
        Instantiate(projectile, muzzle.position, muzzle.rotation);
        audioFire.Play();
        canFire = true;
    }
}
