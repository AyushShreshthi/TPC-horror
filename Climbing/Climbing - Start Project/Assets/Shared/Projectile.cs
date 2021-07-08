using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeToLive,damage;
    [SerializeField]Transform bullethole;
    Vector3 destination;
    private void Start()
    {
        Destroy(gameObject, timeToLive);
    }
    private void Update()
    {
        if (IsDestinationReached())
        {
            Destroy(gameObject);
            return;
        }
        if (WeaponController.wc.shotguninHand)
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (destination != Vector3.zero)
            return;

        RaycastHit hit;
        if(Physics.Raycast(transform.position,transform.forward,out hit, 10f))
        {
            CheckDestructable(hit);
        }
    }
    private void CheckDestructable(RaycastHit hitInfo)
    {
        //print("Hit"+other.name);
        var destructable = hitInfo.transform.GetComponent<Destructables>();

        destination = hitInfo.point+hitInfo.normal*0.008f;

        Transform hole= Instantiate(bullethole, hitInfo.point, Quaternion.LookRotation(hitInfo.normal) * Quaternion.Euler(0, 180, 0)); ;
        hole.SetParent(hitInfo.transform);
        if (destructable == null)
            return;
        destructable.TakeDamage(damage);
    }

    bool IsDestinationReached()
    {
        if(destination==Vector3.zero)
            return false;

        Vector3 directionToDestination = destination - transform.position;

        float dot = Vector3.Dot(directionToDestination, transform.forward);

        if (dot < 0)
            return true;

        return false;

    }
    
}
