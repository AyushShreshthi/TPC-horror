using Boo.Lang;
using Boo.Lang.Runtime.DynamicDispatching;
using System;
using System.Collections;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    Input_Handler ih;
    StateManagerShoot states;
    public Rigidbody rb;

    Vector3 lookPosition;
    Vector3 storeDirection;

    public float runSpeed;
    public float walkSpeed;
    public float aimSpeed;
    public float speedMultiplier;
    public float rotateSpeed;
    public float turnSpeed;

    public float coverAcceleration = 0.5f;
    public float coverMaxSpeed = 2;

    float horizontal;
    float vertical;

    Vector3 lookDirection;

    PhysicMaterial zFriction; 
    PhysicMaterial mFriction;
    Collider col;

    // a list of covers to ignore, like the cover we just left
    List<CoverPosition> ignoreCover = new List<CoverPosition>();
    public void Start()
    {
        ih = GetComponent<Input_Handler>();
        rb = GetComponent<Rigidbody>();
        states = GetComponent<StateManagerShoot>();
        col = GetComponent<Collider>();

        zFriction = new PhysicMaterial();
        zFriction.dynamicFriction = 0;
        zFriction.staticFriction = 0;

        mFriction = new PhysicMaterial();
        mFriction.dynamicFriction = 1;
        mFriction.staticFriction = 1;

    }

    public void Update()
    {
        lookPosition = states.lookPosition;
        lookDirection = lookPosition - transform.position;

        horizontal = states.horizontal;
        vertical = states.vertical;

        if (!states.inCover && !states.vaulting)
        {
            HandleMovementNormal();

            if (horizontal != 0 || vertical != 0)
                SearchForCover();
        }
        else
        {
            if (!states.aiming && !states.vaulting)
                HandleCoverMovement();

            GetOutOfCover();
        }

        col.isTrigger = states.vaulting;
    }

    private void GetOutOfCover()
    {
        if (vertical < -0.5f)
        {
            if (!states.aiming)
            {
                states.coverPos = null;
                states.inCover = false;

                StartCoroutine("ClearIgnoreList");
            }
        }
    }

    private void HandleCoverMovement()
    {
        if (horizontal != 0)
        {
            if (horizontal < 0)
            {
                states.coverDirection = -1;
            }
            else
            {
                states.coverDirection = 1;
            }
        }

        //Quaternion targetRotation = Quaternion.LookRotation(states.coverPos.pos1.forward);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        float lineLength = states.coverPos.length;

        float movement = ((horizontal * coverAcceleration)
            * coverMaxSpeed) * states.myDelta;

        float lerpMovement = movement / lineLength;

        states.coverPercentage -= lerpMovement;

        states.coverPercentage = Mathf.Clamp01(states.coverPercentage);

        Vector3 curvePathPosition = states.coverPos.curvePath.GetPointAt(states.coverPercentage);

        curvePathPosition.y = transform.position.y;

        HandleCoverRotation();

        transform.position = curvePathPosition;
            
    }

    private void HandleCoverRotation()
    {
        float forwardPerc = states.coverPercentage + 0.1f;

        if (forwardPerc > 0.99f)
        {
            forwardPerc = 1;
        }

        Vector3 positionNow = states.coverPos.curvePath.GetPointAt(states.coverPercentage);

        Vector3 positionForward = states.coverPos.curvePath.GetPointAt(forwardPerc);

        Vector3 direction = Vector3.Cross(positionNow, positionForward);

        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, states.myDelta * rotateSpeed);
    }

    private void SearchForCover()
    {
        Vector3 origin = transform.position + Vector3.up / 2;
        Vector3 direction = transform.forward;
        RaycastHit hit;

        if(Physics.Raycast(origin,direction,out hit, 2))
        {
            float distance = Vector3.Distance(origin, hit.point);

                if (distance < 1.5f)
                if (hit.transform.GetComponentInParent<CoverPosition>())
                {
                    if (!ignoreCover.Contains(hit.transform.GetComponentInParent<CoverPosition>()))
                    {
                        CoverPosition cov = hit.transform.GetComponentInParent<CoverPosition>();

                        if (distance < 0.5f && !states.vaulting)
                        {

                            states.GetInCover(hit.transform.GetComponentInParent<CoverPosition>());

                            ignoreCover.Add(hit.transform.GetComponentInParent<CoverPosition>());
                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.Space))
                            {
                                if (!states.vaulting)
                                {
                                    bool climb = false;

                                    if (cov.covertype == CoverPosition.Covertype.full)
                                    {
                                        climb = true;
                                    }
                                    states.Vault(climb);
                                }
                            }
                        }
                    }
                }
            
        }
    }

    IEnumerator ClearIgnoreList()
    {
        yield return new WaitForSeconds(1);
        ignoreCover.Clear();
    }

    private void HandleMovementNormal()
    {
        bool onGround = states.onGround;

        if (horizontal != 0 || vertical != 0 || !onGround || states.vaulting)
        {
            col.material = zFriction;
        }
        else
        {
            col.material = mFriction;
        }

        Vector3 v = ih.camTrans.forward * vertical;
        Vector3 h = ih.camTrans.right * horizontal;

        v.y = 0;
        h.y = 0;

        HandleMovement(h, v, onGround);
        HandleRotation(h, v, onGround);

        if (onGround)
        {
            rb.drag = 4;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void HandleRotation(Vector3 h, Vector3 v, bool onGround)
    {
        if (states.aiming && !states.inCover)
        {
           // lookDirection += transform.right;//add offset if needed;
            lookDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(rb.rotation, targetRotation, states.myDelta * rotateSpeed);

        }
        else
        {
            if (!states.inCover)
            {
                storeDirection = transform.position + h + v;

                Vector3 dir = storeDirection - transform.position;
                dir.y = 0;

                if (horizontal != 0 || vertical != 0)
                {
                    float angl = Vector3.Angle(transform.forward, dir);

                    if (angl != 0)
                    {
                        float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));

                        if (angle != 0)
                            rb.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * states.myDelta);
                    }
                }
            }
        }
    }

    private void HandleMovement(Vector3 h, Vector3 v, bool onGround)
    {
        if (onGround)
        {
            rb.AddForce((v + h).normalized * speed());
        }
    }

    float speed()
    {
        float speed = 0;

        if (states.aiming)
            speed = aimSpeed;
        else
        {
            if(states.walk || states.reloading)
            {
                speed = walkSpeed;
            }
            else
            {
                speed = runSpeed;
            }
        }
        speed *= speedMultiplier;

        return speed;
    }
}
