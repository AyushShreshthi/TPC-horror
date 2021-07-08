using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] float minAngle, maxAngle;
    public void SetRotation(float amount)
    {
        float clamppedAngle = ClampAngle(amount);
        transform.eulerAngles = new Vector3(clamppedAngle, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private float ClampAngle(float amount)
    {
        float newAngle = CheckAngle(transform.eulerAngles.x - amount);
        float clamppedAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);
        return clamppedAngle;
    }

    public float GetAngle()
    {
        return CheckAngle(transform.eulerAngles.x);
    }
    public float CheckAngle(float value)
    {
        float angle = value - 180;
        if (angle > 0)
        {
            return angle - 180;
        }
        return angle + 180;
    }
    
}
