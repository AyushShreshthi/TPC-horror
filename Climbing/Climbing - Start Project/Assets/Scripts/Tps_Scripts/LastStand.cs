using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class LastStand : MonoBehaviour
{
    Input_Handler ih;
    [HideInInspector]
    public Rigidbody rb;
    StateManagerShoot states;
    Collider col;
    bool initDownState;
    PhysicMaterial zFriction;
    PhysicMaterial mFriction;

    float horizontal;
    float vertical;

    float moveDuration = 1;
    float crawlSpeed = 0.1f;

    bool move;
    bool faceDown;
    float curTime;
    Vector3 moveDirection;

    public void Init(StateManagerShoot st)
    {
        ih = GetComponent<Input_Handler>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        states = st;

        zFriction = new PhysicMaterial();
        zFriction.dynamicFriction = 0;
        zFriction.staticFriction = 0;
        
        mFriction = new PhysicMaterial();
        mFriction.dynamicFriction = 1;
        mFriction.staticFriction = 1;
    }
    public void Tick()
    {
        if (!initDownState)
        {
            if(states.weaponManager.ReturnCurrentWeapon() != states.weaponManager.downWeapon)
            {
                states.handleAnimations.anim.CrossFade("Hit_to_Down", 0.3f);
                states.handleAnimations.anim.SetBool("Down", true);
                states.weaponManager.SwitchWeaponWithTargetWeapon(states.weaponManager.downWeapon);
                states.dummyModel = true;

            }

            initDownState = true;
        }

        Vector3 directionToLP = (states.lookPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToLP);
        faceDown = (angle > 90);
        states.handleAnimations.anim.SetBool("FaceDown", faceDown);
        states.handleAnimations.anim.SetBool("Aim", states.aiming);
        HandleAnim(states.vertical, states.horizontal);

        if(move || faceDown && states.aiming)
        {
            Vector3 targetDir = directionToLP;
            targetDir.y = 0;
            Quaternion targetRot = Quaternion.LookRotation((faceDown) ? -targetDir : targetDir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, states.myDelta*0.5f);
        }

        if(move && !states.aiming)
        {
            curTime += states.myDelta;

            if (curTime < moveDuration)
            {
                col.material = zFriction;
                rb.AddForce(moveDirection * crawlSpeed, ForceMode.VelocityChange);
            }
            else
            {
                crawlSpeed = 0.1f;
                moveDuration = 1;
                col.material = mFriction;
                curTime = 0;
                move = false;
            }
        }
        else
        {
            FindAngles();

            horizontal = states.horizontal;
            vertical = states.vertical;

            Vector3 v = ih.camPivot.forward * vertical;
            Vector3 h = ih.camPivot.right * horizontal;

            if(v!=Vector3.zero || h != Vector3.zero)
            {
                move = true;
                moveDirection = (v + h).normalized;
            }
        }
    }

    private void FindAngles()
    {
        Vector3 dir = states.lookPosition - transform.position;
        Vector3 relativePosition = transform.InverseTransformDirection(dir.normalized);

        float s = relativePosition.x;

        states.handleAnimations.anim.SetFloat("AimSides", s, 0.5f, states.myDelta);
    }

    private void HandleAnim(float vertical, float horizontal)
    {
        states.handleAnimations.anim.SetFloat("Forward", vertical);
        states.handleAnimations.anim.SetFloat("Sideways", horizontal);
    }

    public void AddMovement(Vector3 relativeDirection,float duration,float speed)
    {
        Vector3 worldDir = transform.TransformDirection(relativeDirection);
        moveDirection = worldDir;
        move = true;
        moveDuration = duration;
        crawlSpeed = speed;
    }
}
