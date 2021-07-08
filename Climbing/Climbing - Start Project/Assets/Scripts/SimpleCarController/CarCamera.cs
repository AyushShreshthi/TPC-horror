using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public Transform objectfollow;
    public Vector3 offset;
    public float followSpeed = 10;
    public float lookSpeed = 10;

    public void LookAt()
    {
        Vector3 _lookdirection = objectfollow.position - transform.position;
        Quaternion _rot = Quaternion.LookRotation(_lookdirection, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);
    }
    public void MoveToTarget()
    {
        Vector3 _targetPos = objectfollow.position +
            objectfollow.forward * offset.z +
            objectfollow.right * offset.x +
            objectfollow.up * offset.y;

        transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        LookAt();
        MoveToTarget();
    }
}
