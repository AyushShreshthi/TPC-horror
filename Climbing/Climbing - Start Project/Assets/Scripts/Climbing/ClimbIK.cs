using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class ClimbIK : MonoBehaviour
    {
        Animator anim;

        Point lhPoint;
        Point lfPoint;
        Point rhPoint;
        Point rfPoint;

        public float lh = 1;
        public float rh = 1;
        public float lf = 1;
        public float rf = 1;

        Transform lhHelper;
        Transform lfHelper;
        Transform rhHelper;
        Transform rfHelper;

        Vector3 lhTargetPosition;
        Vector3 lfTargetPosition;
        Vector3 rhTargetPosition;
        Vector3 rfTargetPosition;

        public float helperSpeed = 25;

        Transform hips;

        public bool forceFeetHeight;

        private void Start()
        {
            anim = GetComponent<Animator>();
            hips = anim.GetBoneTransform(HumanBodyBones.Hips);

            lhHelper = new GameObject().transform;
            lhHelper.name = "lh helper ik";
            lfHelper = new GameObject().transform;
            lfHelper.name = "lf helper ik";
            rhHelper = new GameObject().transform;
            rhHelper.name = "rh helper ik";
            rfHelper = new GameObject().transform;
            rfHelper.name = "rf helper ik";
        }
        public void UpdateAllPointsOnOne(Point targetPoint)
        {
            lhPoint = targetPoint;
            lfPoint = targetPoint;
            rhPoint = targetPoint;
            rfPoint = targetPoint;
        }
        public void UpdatePoint(AvatarIKGoal ik, Point targetPoint)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    lfPoint = targetPoint;
                    break;
                case AvatarIKGoal.RightFoot:
                    rfPoint = targetPoint;
                    break;
                case AvatarIKGoal.LeftHand:
                    lhPoint = targetPoint;
                    break;
                case AvatarIKGoal.RightHand:
                    rhPoint = targetPoint;
                    break;
                default:
                    break;

            }
        }

        public void UpdateAllTargetPositions(Point p)
        {
            IKPositions lhHolder = p.ReturnIK(AvatarIKGoal.LeftHand);
            if (lhHolder.target)
                lhTargetPosition = lhHolder.target.position;

            IKPositions rhHolder = p.ReturnIK(AvatarIKGoal.RightHand);
            if (rhHolder.target)
                rhTargetPosition = rhHolder.target.position;

            IKPositions lfHolder = p.ReturnIK(AvatarIKGoal.LeftFoot);
            if (lfHolder.target)
                lfTargetPosition = lfHolder.target.position;

            IKPositions rfHolder = p.ReturnIK(AvatarIKGoal.RightFoot);
            if (rfHolder.target)
                rfTargetPosition = rfHolder.target.position;
        }
        public void UpdateTargetPositions(AvatarIKGoal ik, Vector3 targetPosition)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    lfTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.RightFoot:
                    rfTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.LeftHand:
                    lhTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.RightHand:
                    rhTargetPosition = targetPosition;
                    break;
                default:
                    break;
            }
        }

        public Vector3 ReturnCurrentPointPosition(AvatarIKGoal ik)
        {
            Vector3 retVal = default(Vector3);

            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    IKPositions lfHolder = lfPoint.ReturnIK(AvatarIKGoal.LeftFoot);
                    retVal = lfHolder.target.transform.position;
                    break;
                case AvatarIKGoal.RightFoot:
                    IKPositions rfHolder = rfPoint.ReturnIK(AvatarIKGoal.RightFoot);
                    retVal = rfHolder.target.transform.position;
                    break;
                case AvatarIKGoal.LeftHand:
                    IKPositions lhHolder = lhPoint.ReturnIK(AvatarIKGoal.LeftHand);
                    retVal = lhHolder.target.transform.position;
                    break;
                case AvatarIKGoal.RightHand:
                    IKPositions rhHolder = rhPoint.ReturnIK(AvatarIKGoal.RightHand);
                    retVal = rhHolder.target.transform.position;
                    break;
                default:
                    break;
            }
            return retVal;
        }

        public Point ReturnPointForIK(AvatarIKGoal ik)
        {
            Point retVal = null;

            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    retVal = lfPoint;
                    break;
                case AvatarIKGoal.RightFoot:
                    retVal = rfPoint;
                    break;
                case AvatarIKGoal.LeftHand:
                    retVal = lhPoint;
                    break;
                case AvatarIKGoal.RightHand:
                    retVal = rhPoint;
                    break;
                default:
                    break;
            }
            if (retVal == null)
                Debug.Log("point for " + ik.ToString() + " is not found");

            return retVal;
        }

        public AvatarIKGoal ReturnOppositeIK(AvatarIKGoal ik)
        {
            AvatarIKGoal retVal = default(AvatarIKGoal);

            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    retVal = AvatarIKGoal.RightFoot;
                    break;
                case AvatarIKGoal.RightFoot:
                    retVal = AvatarIKGoal.LeftFoot;
                    break;
                case AvatarIKGoal.LeftHand:
                    retVal = AvatarIKGoal.RightHand;
                    break;
                case AvatarIKGoal.RightHand:
                    retVal = AvatarIKGoal.LeftHand;
                    break;
                default:
                    break;
            }

            return retVal;
        }
        public AvatarIKGoal ReturnOppositeLimb(AvatarIKGoal ik)
        {
            AvatarIKGoal retVal = default(AvatarIKGoal);

            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    retVal = AvatarIKGoal.LeftHand;
                    break;
                case AvatarIKGoal.RightFoot:
                    retVal = AvatarIKGoal.RightHand;
                    break;
                case AvatarIKGoal.LeftHand:
                    retVal = AvatarIKGoal.LeftFoot;
                    break;
                case AvatarIKGoal.RightHand:
                    retVal = AvatarIKGoal.RightFoot;
                    break;
                default:
                    break;
            }

            return retVal;
        }

        public void AddWeightInfluenceAll(float w)
        {
            lh = w;
            lf = w;
            rh = w;
            rf = w;
        }

        public void ImmediatePlaceHelpers()
        {
            if (lhPoint != null)
            {
                lhHelper.position = lhTargetPosition;
            }
            if (rhPoint != null)
            {
                rhHelper.position = rhTargetPosition;
            }
            if (lfPoint != null)
            {
                lfHelper.position = lfTargetPosition;
            }
            if (rfPoint != null)
            {
                rfHelper.position = rfTargetPosition;
            }
        }

        private void OnAnimatorIK()
        {
            if (lhPoint)
            {
                IKPositions lhHolder = lhPoint.ReturnIK(AvatarIKGoal.LeftHand);

                if (lhHolder.target)
                {
                    lhHelper.transform.position = Vector3.Lerp(lhHelper.transform.position, lhTargetPosition, Time.deltaTime * helperSpeed);
                }
                UpdateIK(AvatarIKGoal.LeftHand, lhHolder, lhHelper, lh, AvatarIKHint.LeftElbow);//leftelbow
            }
            if (rhPoint)
            {
                IKPositions rhHolder = rhPoint.ReturnIK(AvatarIKGoal.RightHand);

                if (rhHolder.target)
                {
                    rhHelper.transform.position = Vector3.Lerp(rhHelper.transform.position, rhTargetPosition, Time.deltaTime * helperSpeed);
                }
                UpdateIK(AvatarIKGoal.RightHand, rhHolder, rhHelper, rh, AvatarIKHint.RightElbow);

            }
            if (hips == null)
                hips = anim.GetBoneTransform(HumanBodyBones.Hips);

            if (lfPoint)
            {
                IKPositions lfHolder = lfPoint.ReturnIK(AvatarIKGoal.LeftFoot);

                if (lfHolder.target)
                {
                    Vector3 targetPosition = lfTargetPosition;

                    if (forceFeetHeight)
                    {
                        if (targetPosition.y > hips.transform.position.y)
                        {
                            targetPosition.y = targetPosition.y - 0.2f;
                        }
                    }
                    lfHelper.transform.position = Vector3.Lerp(lfHelper.transform.position, targetPosition, Time.deltaTime * helperSpeed);
                }
                UpdateIK(AvatarIKGoal.LeftFoot, lfHolder, lfHelper, lf, AvatarIKHint.LeftKnee);
            }
            if (rfPoint)
            {
                IKPositions rfHolder = rfPoint.ReturnIK(AvatarIKGoal.RightFoot);

                if (rfHolder.target)
                {
                    Vector3 targetPosition = rfTargetPosition;

                    if (forceFeetHeight)
                    {
                        if (targetPosition.y > hips.transform.position.y)
                        {
                            targetPosition.y = targetPosition.y - 0.2f;
                        }
                    }
                    rfHelper.transform.position = Vector3.Lerp(rfHelper.transform.position, targetPosition, Time.deltaTime * helperSpeed);
                }
                UpdateIK(AvatarIKGoal.RightFoot, rfHolder, rfHelper, rf, AvatarIKHint.RightKnee);
            }
        }

        private void UpdateIK(AvatarIKGoal ik, IKPositions holder, Transform helper, float weight, AvatarIKHint h)
        {
            if (holder != null)
            {
                anim.SetIKPositionWeight(ik, weight);
                anim.SetIKRotationWeight(ik, weight);
                anim.SetIKPosition(ik, helper.position);
                anim.SetIKRotation(ik, helper.rotation);

                if (ik == AvatarIKGoal.LeftHand || ik == AvatarIKGoal.RightHand)
                {
                    Transform shoulder = (ik == AvatarIKGoal.LeftHand) ?
                        anim.GetBoneTransform(HumanBodyBones.LeftShoulder) :
                        anim.GetBoneTransform(HumanBodyBones.RightShoulder);

                    Vector3 offset = Vector3.zero;
                    offset += transform.forward;
                    offset += transform.up * 2.2f;
                    offset += transform.position;

                    Vector3 targetRotationDir = shoulder.transform.position - offset;
                    // targetRotationDir.x = 0;

                    Quaternion targetRot = Quaternion.LookRotation(-targetRotationDir);
                    helper.rotation = targetRot;

                }
                else
                {
                    helper.rotation = holder.target.transform.rotation;
                }

                if (holder.hint != null)
                {
                    anim.SetIKHintPositionWeight(h, weight);
                    anim.SetIKHintPosition(h, holder.hint.position);
                }

            }
        }

        public void InfluenceWeight(AvatarIKGoal ik, float t)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    lf = t;
                    break;
                case AvatarIKGoal.LeftHand:
                    lh = t;
                    break;
                case AvatarIKGoal.RightFoot:
                    rf = t;
                    break;
                case AvatarIKGoal.RightHand:
                    rh = t;
                    break;
            }
        }


    }

}