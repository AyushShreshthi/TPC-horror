using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrWheel : MonoBehaviour
{
    public bool wheelFrontLeft;
    public bool wheelFrontRight;
    public bool wheelRearLeft;
    public bool wheelRearRight;

    private Rigidbody rb;
    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;


    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springForce;
    private float damperForce;
    private float springVelocity;


    private Vector3 suspensionForce;
    private float wheelAngle;
    private Vector3 wheelVelocityLS;
    private float Fx;
    private float Fy;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerTime;
    public float steerAngle;
    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel; 
    }
    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up*wheelAngle);

        Debug.DrawRay(transform.position, -transform.up * (springLength + wheelRadius), Color.green);
    }

    void FixedUpdate()
    {
        if(Physics.Raycast(transform.position,-transform.up,out RaycastHit hit, maxLength + wheelRadius))
        {
            lastLength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce +damperForce)* transform.up;
            wheelVelocityLS =transform.InverseTransformDirection( rb.GetPointVelocity(hit.point));

            Fx = Input.GetAxis("Vertical")*springForce*0.1f;
            Fy = wheelVelocityLS.x * springForce;

            rb.AddForceAtPosition(suspensionForce+(Fx*transform.forward)+ (Fy*-transform.right),hit.point);
        }
    }
}
