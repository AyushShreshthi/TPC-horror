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
        public PointStats pointD;

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
        public void UpdatePoint(AvatarIKGoal ik,Point targetPoint)
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
            
            lhTargetPosition = CreateIKPos(p, AvatarIKGoal.LeftHand);

            rhTargetPosition = CreateIKPos(p, AvatarIKGoal.RightHand);

            lfTargetPosition = CreateIKPos(p, AvatarIKGoal.LeftFoot);

            rfTargetPosition = CreateIKPos(p, AvatarIKGoal.RightFoot);
             
        }
        public void UpdateTargetPositions(AvatarIKGoal ik,Vector3 targetPosition )
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
                    retVal = CreateIKPos(lfPoint, AvatarIKGoal.LeftFoot);
                    break;
                case AvatarIKGoal.RightFoot:
                    retVal = CreateIKPos(rfPoint, AvatarIKGoal.RightFoot);
                    break;
                case AvatarIKGoal.LeftHand:
                    retVal = CreateIKPos(lhPoint, AvatarIKGoal.LeftHand);
                    break;
                case AvatarIKGoal.RightHand:
                    retVal = CreateIKPos(rhPoint, AvatarIKGoal.RightHand);
                    break;
                default:
                    break;
            }
            return retVal;
        }

        public Vector3 ReturnIKPosition_OnTargetPoint(Point p,AvatarIKGoal ik)
        {
            Vector3 retVal = default(Vector3);

            switch (ik)
            {
                case AvatarIKGoal.LeftFoot:
                    retVal = CreateIKPos(p, AvatarIKGoal.LeftFoot);
                    break;
                case AvatarIKGoal.RightFoot:
                    retVal = CreateIKPos(p, AvatarIKGoal.RightFoot);
                    break;
                case AvatarIKGoal.LeftHand:
                    retVal = CreateIKPos(p, AvatarIKGoal.LeftHand);
                    break;
                case AvatarIKGoal.RightHand:
                    retVal = CreateIKPos(p, AvatarIKGoal.RightHand);
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
                IKPositions lhHolder = pointD.GetIKPos(AvatarIKGoal.LeftHand);
                lhHelper.transform.position = Vector3.Lerp(lhHelper.transform.position, lhTargetPosition,Time.deltaTime*helperSpeed);
                
                UpdateIK(AvatarIKGoal.LeftHand, lhHolder, lhHelper, lh, lhPoint);//leftelbow
            }
            if (rhPoint)
            {
                IKPositions rhHolder = pointD.GetIKPos(AvatarIKGoal.RightHand);

                rhHelper.transform.position = Vector3.Lerp(rhHelper.transform.position, rhTargetPosition, Time.deltaTime * helperSpeed);
                
                UpdateIK(AvatarIKGoal.RightHand, rhHolder, rhHelper, rh, rhPoint);

            }
            if (hips == null)
                hips = anim.GetBoneTransform(HumanBodyBones.Hips);

            if (lfPoint)
            {
                IKPositions lfHolder = pointD.GetIKPos(AvatarIKGoal.LeftFoot);
                Vector3 targetPosition = lfTargetPosition;

                if (forceFeetHeight)
                {
                    if (targetPosition.y > hips.transform.position.y)
                    {
                        targetPosition.y = targetPosition.y - 0.2f;
                    }
                }
                lfHelper.transform.position = Vector3.Lerp(lfHelper.transform.position, targetPosition, Time.deltaTime * helperSpeed);
               
                UpdateIK(AvatarIKGoal.LeftFoot, lfHolder, lfHelper, lf, lfPoint);
            }
            if (rfPoint)
            {
                IKPositions rfHolder = pointD.GetIKPos(AvatarIKGoal.RightFoot);

                Vector3 targetPosition = rfTargetPosition;

                if (forceFeetHeight)
                {
                    if (targetPosition.y > hips.transform.position.y)
                    {
                        targetPosition.y = targetPosition.y - 0.2f;
                    }
                }
                rfHelper.transform.position = Vector3.Lerp(rfHelper.transform.position, targetPosition, Time.deltaTime * helperSpeed);
                
                UpdateIK(AvatarIKGoal.RightFoot, rfHolder, rfHelper, rf, rfPoint);
            }
        }

        private void UpdateIK(AvatarIKGoal ik, IKPositions holder, Transform helper, float weight, Point tP)
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
                    helper.rotation = transform.rotation;
                }

                if (holder.hasHint)
                {
                    anim.SetIKHintPositionWeight(holder.ikHint, weight);
                    Vector3 hintPos = toWP(tP, holder.hintPos);
                    anim.SetIKHintPosition(holder.ikHint, hintPos);
                }
                
            }
        }

        public void InfluenceWeight(AvatarIKGoal ik,float t)
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

        public Vector3 GetHipPos(Point p)
        {
            Vector3 hip = pointD.GetHip(p.pointType).hipPos;
            Vector3 targetP = p.transform.TransformPoint(hip);
            return targetP;
        }
        public Vector3 toWP(Point tp,Vector3 lp)
        {
            return tp.transform.TransformPoint(lp);
        }
        
        public Vector3 CreateIKPos(Point tp,AvatarIKGoal ikGoal)
        {
            Vector3 r = Vector3.zero;
            IKPositions ikPos = pointD.GetIKPos(ikGoal);
            r = toWP(tp, ikPos.ikPos);

            return r;
        }
    }
    
}
