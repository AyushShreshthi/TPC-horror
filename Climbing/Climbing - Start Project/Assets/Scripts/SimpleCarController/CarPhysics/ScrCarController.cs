using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrCarController : MonoBehaviour
{
    public ScrWheel[] wheels;

    [Header("Car Specs")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;

    [Header("Inputs")]
    public float steerInput;

    private float ackremannAngleLeft;
    private float ackremannAngleRight;
    
    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");

        if (steerInput > 0)
        {
            ackremannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackremannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else if (steerInput < 0)
        {
            ackremannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackremannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {

            ackremannAngleLeft = 0;
            ackremannAngleRight = 0;
        }

        foreach(ScrWheel w in wheels)
        {
            if (w.wheelFrontLeft)
            {
                w.steerAngle = ackremannAngleLeft;
            }
            if (w.wheelFrontRight)
            {
                w.steerAngle = ackremannAngleRight;
            }
        }
    }
}
