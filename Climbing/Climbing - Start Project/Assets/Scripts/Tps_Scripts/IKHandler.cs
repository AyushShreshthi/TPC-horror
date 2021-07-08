using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    Animator anim;
    StateManagerShoot states;

    public float lookWeight = 1;
    public float bodyWeight = 0.8f;
    public float headWeight = 1;
    public float clampWeight = 1;

    float targetWeight;

    public Transform weaponHolder;
    public Transform rightShoulder;

    public Transform overrideLookTarget;

    public Transform rightHandIKTarget;
    public Transform rightElbowTarget;
    public float rightHandIKWeight;
    float targetRHWeight;

    public Transform leftHandIKTarget;
    public Transform leftElbowTarget;
    public float leftHandIKWeight;
    float targetLHWeight;

    Transform aimHelper;

    [HideInInspector]
    public bool LHIK_dis_notAiming;

    public bool bypassAngleClamp;
    bool disableIK;
    public void Start()
    {
        aimHelper = new GameObject().transform;
        anim = GetComponent<Animator>();
        states = GetComponent<StateManagerShoot>();
    }

    public void Update()
    {
        if (!states.meleeWeapon)
        {
            HandleShoulders();

            AimWeight();
            HandleRightHandIKWeight();
            HandleLeftHandIKWeight();
            HandleShoulderRotation();
        }
        else
        {
            lookWeight = 0;
        }
    }

    private void HandleLeftHandIKWeight()
    {
        float multiplier = 3;

        if (states.inCover)
        {
            if (!LHIK_dis_notAiming)
            {
                targetLHWeight = 1;
                multiplier = 6;
            }
            else
            {
                multiplier = 10;

                if (states.aiming)
                {
                    targetLHWeight = 1;
                }
                else
                {
                    targetLHWeight = 0;
                    leftHandIKWeight = 0;
                }
            }
        }
        else
        {
            if (!LHIK_dis_notAiming)
            {
                targetLHWeight = 1;
                multiplier = 10;
            }
            else
            {
                multiplier = 10;
                targetLHWeight = (states.aiming) ? 1 : 0;
            }
        }

        if (states.reloading ||states.vaulting || disableIK)
        {
            targetLHWeight = 0;
            multiplier = 10;
        }

        if (disableIK)
        {
            leftHandIKWeight = 0;
        }

        leftHandIKWeight = Mathf.Lerp(leftHandIKWeight, targetLHWeight, states.myDelta * multiplier);
    }

    private void HandleRightHandIKWeight()
    {
        float multiplier = 3;

        if (states.inCover)
        {
            targetRHWeight = 0;

            if (states.aiming)
            {
                targetRHWeight = 1;
                multiplier = 2;
            }
            else
            {
                multiplier = 10;
            }
        }
        else
        {
            rightHandIKWeight = lookWeight;
        }

        if (states.reloading)
        {
            targetRHWeight = 0;
            multiplier = 5;
        }

        rightHandIKWeight = Mathf.Lerp(rightHandIKWeight, targetRHWeight, states.myDelta * multiplier);
    }

    private void AimWeight()
    {
        if (states.aiming && !states.reloading)
        {
            Vector3 directionTowardsTarget = aimHelper.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

            if (angle < 90 || bypassAngleClamp)
            {
                targetWeight = 1;
            }
            else
            {
                targetWeight = 0;
            }
        }
        else
        {
            targetWeight = 0;
        }
        float multiplier = (states.aiming) ? 5 : 30;

        lookWeight = Mathf.Lerp(lookWeight, targetWeight,
            states.myDelta * multiplier);

        rightHandIKWeight = lookWeight;

        if (!LHIK_dis_notAiming)
        {
            leftHandIKWeight = 1 - anim.GetFloat("LeftHandIKWeightOverride");
        }
        else
        {
            leftHandIKWeight = (states.aiming) ? 1 - anim.GetFloat("LeftHandIKWeightOverride") : 0;
        }

        if (disableIK)
        {
            targetWeight = 0;
            lookWeight = 0;
        }
    }

    private void HandleShoulders()
    {
        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        else
        {
            weaponHolder.position = rightShoulder.position;
        }
    }

    private void HandleShoulderRotation()
    {
        aimHelper.position = Vector3.Lerp(aimHelper.position, states.lookPosition, states.myDelta * 5);
        weaponHolder.LookAt(aimHelper.position);
        rightHandIKTarget.parent.transform.LookAt(aimHelper.position);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, clampWeight);

        Vector3 filterDirection = states.lookPosition;
        // filterDirection.y = offset;//if needed;


        anim.SetLookAtPosition(
            (overrideLookTarget != null) ?
            overrideLookTarget.position : filterDirection);

        if (leftHandIKTarget)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }

        if (rightHandIKTarget)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
        }

        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }


        if (rightElbowTarget)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightHandIKWeight);
            anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowTarget.position);
        }
        else
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }

        if (leftElbowTarget)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftHandIKWeight);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowTarget.position);
        }
        else
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }
    }
}
