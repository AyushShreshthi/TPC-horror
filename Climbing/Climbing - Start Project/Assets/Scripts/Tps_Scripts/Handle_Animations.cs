using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Handle_Animations : MonoBehaviour
{
    public Animator anim;

    StateManagerShoot states;
    Vector3 lookDirection;

    public void Start()
    {
        states = GetComponent<StateManagerShoot>();
        SetupAnimator();
    }
    public void Update()
    {
        anim.speed = 1 * states.tm.myTimeScale;


        states.reloading = anim.GetBool("Reloading");
        anim.SetBool("Aim", states.aiming);
        anim.SetBool("OnGround", (!states.vaulting) ? states.onGround : true);

        anim.SetInteger("WeaponType", states.weaponAnimType);

        if(states.aiming || states.inCover)
        {
            anim.SetBool("ExitLocomotion", true);
        }
        else
        {
            anim.SetBool("ExitLocomotion", false);
        }


        if (!states.canRun)
        {
            anim.SetFloat("Forward", states.vertical, 0.1f, states.myDelta);
            anim.SetFloat("Sideways", states.horizontal, 0.1f, states.myDelta);
        }
        else
        {
            float movement = Mathf.Abs(states.vertical) + Mathf.Abs(states.horizontal);

            bool walk = states.walk;

            movement = Mathf.Clamp(movement, 0, (walk || states.reloading) ? 0.5f : 1);

            anim.SetFloat("Forward", movement, 0.1f, states.myDelta); 
        }

        anim.SetBool("Cover", states.inCover);
        anim.SetInteger("CoverDirection", states.coverDirection);
        anim.SetBool("CrouchToUpAim", states.crouchCover);
        anim.SetFloat("Stance", states.stance);
        anim.SetBool("AimAtSides", states.aimAtSides);
    }
    public void SetupAnimator(Animator targetAnim=null)
    {
        anim = GetComponent<Animator>();
        if (targetAnim == null)
        {
            Animator[] anims = GetComponentsInChildren<Animator>();

            for (int i = 0; i < anims.Length; i++)
            {
                if (anims[i] != anim)
                {
                    anim.avatar = anims[i].avatar;
                    Destroy(anims[i]);
                    break;
                }
            }
        }
        else
        {
            anim.avatar = targetAnim.avatar;
            Destroy(targetAnim);
        }
    }

    public void StartReload()
    {
        if (!states.reloading)
        {
            anim.SetTrigger("Reload");
        }
    }
}
