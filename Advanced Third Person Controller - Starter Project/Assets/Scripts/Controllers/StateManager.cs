using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPC
{
    public class StateManager : MonoBehaviour
    {
        [Header("Info")]
        public GameObject modelPrefab;
        public bool inGame;
        public bool isPlayer;
        
        [Header("Stats")]
        public float groundDistance = 0.6f;
        public float groundOffset = 0f;
        public float distanceToCheckForward = 1.3f;
        public float runSpeed = 6;
        public float walkSpeed = 4;
        public float jumpForce = 15;
        public float airTimeThreshold = 0.8f;

        [Header("Inputs")]
        public float horizontal;
        public float vertical;
        public bool jumpInput;

        [Header("States")]
        public bool obstacleForward;
        public bool groundForward;
        public float groundAngle;
        public bool vaulting;

        [Header("Combat")]
        public bool meleeWeapon;
        public bool aiming;
        public bool inCover;
        public bool reloading;

        #region StateRequests
        [Header("State Requests")]
        public CharStates curState;
        public bool onGround;
        public bool run;
        public bool walk;
        public bool onLocomotion;
        public bool inAngle_MoveDir;
        public bool jumping;
        public bool canJump;
        public bool canVault = true;  // if we dont want player to jump make it false
        #endregion


        #region Variables
        [HideInInspector]
        public Vector3 moveDirection;
        [HideInInspector]
        public Vector3 aimPosition;
        Transform aimhelper;
        float currentY;
        float currentZ;
        public float airTime;
        [HideInInspector]
        public bool prevGround;
        [HideInInspector]
        public Vector3 targetVaultPosition;
        [HideInInspector]
        public Vector3 startVaultPosition;
        [HideInInspector]
        public bool skipGroundCheck;

        public float vaultOverHeight;                    //vaulting variables 
        public float vaultFloorHeightDifference;         //vaulting variables 
       
        public enum VaultType
        {
            idle, walk, run,walk_up,climb_up
        }

        [HideInInspector]
        public VaultType curVaultType;

        #endregion

        LayerMask ignoreLayers;
        public enum CharStates
        {
            idle, moving, onAir, hold, vaulting
        }

        #region References
        GameObject activeModel;
        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rBody;
        [HideInInspector]
        public Collider controllerCollider;
        bool quadratic;

        public Vector3 offsetBack = new Vector3(0, 0, -0.3f);
        public Vector3 offsetFront = new Vector3(0, 0, 0.3f);


        enum ClimbCheckType
        {
            walk_up,climb_up
        }
        internal void FixedTick()
        {
            if (curState == CharStates.hold
                || curState == CharStates.vaulting)
            {
                return;
            }

            obstacleForward = false;
            groundForward = false;
            onGround = OnGround();

            if (onGround)
            {
                Vector3 origin = transform.position;

                //clear forward
                origin += Vector3.up * 0.8f;
                IsClear(origin, transform.forward, distanceToCheckForward, ref obstacleForward);
                if (!obstacleForward && !vaulting)
                {
                    //is ground forward?
                    origin += transform.forward * 0.6f;
                    IsClear(origin, -Vector3.up, groundDistance * 3, ref groundForward);

                }
                else
                {
                    if(Vector3.Angle(transform.forward, moveDirection) > 30)
                    {
                        obstacleForward = false;
                    }
                }
            }
            UpdateState();
            MonitorAirTime();
        }
        void UpdateState()
        {
            if (curState == CharStates.hold)
            {
                return;
            }
            if(vaulting)
            {
                curState = CharStates.vaulting;
                return;
            }
            if(horizontal!=0 || vertical != 0)
            {
                curState = CharStates.moving;
            }
            else
            {
                curState = CharStates.idle;
            }

            if (!onGround)
            {
                curState = CharStates.onAir;
            }
        }
        void MonitorAirTime()
        {
            if (!jumping)
                anim.SetBool(Statics.onAir, !onGround);

            if (onGround)
            {
                if (prevGround != onGround)
                {
                    anim.SetInteger(Statics.jumpType,
                        (airTime > airTimeThreshold) ?
                        (horizontal != 0 || vertical != 0)?2:1 :
                        0);
                }
                airTime = 0;
            }
            else
            {
                airTime += Time.deltaTime;
            }
            prevGround = onGround;
        }
        private void IsClear(Vector3 origin, Vector3 direction, float distance, ref bool isHit)
        {
            RaycastHit hit=new RaycastHit();
            float targetDistance = distance;
            if (run)
                targetDistance += 0f;// chanagable 

            int numberOfhits = 0;
            for(int i = -1; i < 2; i++)
            {
                Vector3 targetOrigin = origin;
                targetOrigin += transform.right * (i * 0.3f);
                Debug.DrawRay(targetOrigin, direction * targetDistance, Color.green);
                if(Physics.Raycast(targetOrigin,direction,out hit, targetDistance, ignoreLayers))
                {
                   // isHit = true;
                    numberOfhits++;
                }
            }
            #region esehi
            //Debug.DrawRay(origin, direction * distance, Color.green);
            //if(Physics.Raycast(origin,direction,out hit, distance, ignoreLayers))
            //{
            //    isHit = true;
            //}
            //else
            //{
            //    isHit = false;
            //}
            #endregion
            if (numberOfhits > 2)
            {
                isHit = true;
            }
            else
            {
                isHit = false;
            }
            if (obstacleForward)
            {
                Vector3 incomingVec = hit.point - origin;
                Vector3 reflectVect = Vector3.Reflect(incomingVec, hit.normal);
                float angle = Vector3.Angle(incomingVec, reflectVect);

                if (angle < 70)
                {
                    obstacleForward = false;
                }
                else
                {
                    if(numberOfhits > 2)
                    {
                        bool willVault = false;

                        CanVaultOver(hit, ref willVault);

                        if (willVault)
                        {
                            
                            curVaultType = VaultType.walk;
                            if (run)
                                curVaultType = VaultType.run;
                            obstacleForward = false;
                            return;
                        }
                        else
                        {
                            bool willClimb = false;
                            ClimbOver(hit, ref willClimb,ClimbCheckType.walk_up);
                            if (!willClimb)
                            {
                                ClimbOver(hit, ref willClimb, ClimbCheckType.climb_up);
                                if (willClimb)
                                {
                                    obstacleForward = true;
                                    return;
                                }
                            }
                            if (!willClimb)
                            {
                                obstacleForward = true;
                                return;
                            }
                        }
                    }
                }
            }

            if (groundForward)
            {
                if (horizontal != 0 || vertical != 0)
                {
                    Vector3 p1 = transform.position;
                    Vector3 p2 = hit.point;
                    float diffY = p1.y - p2.y;
                    groundAngle = diffY;
                }
                float targetIncline = 0;

                if (Mathf.Abs(groundAngle) > 0.3f)
                {
                    targetIncline = (groundAngle < 0) ? 1 : -1;
                }
                if (groundAngle == 0)
                {
                    targetIncline = 0;
                }
                anim.SetFloat(Statics.incline, targetIncline, 0.3f, Time.deltaTime);
            }
            
        }

        private void ClimbOver(RaycastHit hit, ref bool willClimb,ClimbCheckType ct)
        {
            float targetDistance = distanceToCheckForward + 0.2f;
            if (run)
                targetDistance += 0f;//0.5f

            Vector3 climbCheckOrigin = transform.position + (Vector3.up * Statics.walkupHeight);
            switch (ct)
            {
                case ClimbCheckType.walk_up:
                    climbCheckOrigin += Vector3.up * Statics.walkupHeight;
                    break;
                case ClimbCheckType.climb_up:
                    climbCheckOrigin += Vector3.up * Statics.climbMaxHeight;
                    break;
            }
            RaycastHit climbHit;
            
            Vector3 wallDirection = -hit.normal * targetDistance;
            Debug.DrawRay(climbCheckOrigin, wallDirection, Color.yellow);
            if(Physics.Raycast(climbCheckOrigin,wallDirection,out climbHit, 1.2f, ignoreLayers))
            {
                print("it's a wall");
            }
            else
            {
                Vector3 origin2 = hit.point;
                origin2.y = transform.position.y;

                switch (ct)
                {
                    case ClimbCheckType.walk_up:
                        origin2 += Vector3.up * Statics.walkupHeight;
                        break;
                    case ClimbCheckType.climb_up:
                        origin2 += Vector3.up * Statics.climbMaxHeight;
                        break;
                }
                //origin2 += wallDirection;
                Debug.DrawRay(origin2, -Vector3.up, Color.white);

                if(Physics.Raycast(origin2,-Vector3.up,out climbHit, 1.2f, ignoreLayers))
                {

                    float diff = climbHit.point.y - transform.position.y;
                    if (Mathf.Abs(diff) > 0.4f)
                    {
                        vaulting = true;
                        targetVaultPosition = climbHit.point;
                        obstacleForward = false;
                        willClimb = true;
                        if (diff < 1.5f)                        // addition we made
                        {                                       // addition we made
                            ct = ClimbCheckType.walk_up;        // addition we made
                        }                                       // addition we made
                        switch (ct)
                        {
                            case ClimbCheckType.walk_up:
                                curVaultType = VaultType.walk_up;
                                skipGroundCheck = true;
                                break;

                            case ClimbCheckType.climb_up:
                                curVaultType = VaultType.climb_up;
                                skipGroundCheck = true;
                                Vector3 startPos = hit.normal * Statics.climbUpStartPosOffset;
                                startPos = hit.point + startPos;
                                startPos.y = transform.position.y;
                                startVaultPosition = startPos;
                                break;
                        }
                        return;
                    }
                }
            }
        }

        private void CanVaultOver(RaycastHit hit, ref bool willVault)
        {
            if (!onLocomotion || !inAngle_MoveDir)
                return;

            //we hit a wall around knee high
            //then we need to see if we can vault over it
            Vector3 wallDirection = -hit.normal * 0.5f;
            //the opposite of the normal, is going to return us the direction
            //if the whole level is set with box colliders, then this will work like a charm

            RaycastHit vHit;

            Vector3 wallOrigin = transform.position + Vector3.up * vaultOverHeight;
            Debug.DrawRay(wallOrigin, wallDirection * Statics.vaultCheckDistance, Color.cyan);

            if(Physics.Raycast(wallOrigin,wallDirection,out vHit, Statics.vaultCheckDistance, ignoreLayers))
            {
                willVault = false;
                return;
            }
            else
            {
                // its not a wall, but can we vault over it?
                if (canVault && !vaulting)
                {
                    Vector3 startOrigin = hit.point;
                    startOrigin.y = transform.position.y;
                    Vector3 vOrigin = startOrigin + Vector3.up * vaultOverHeight;

                    if (!run)
                        vOrigin += wallDirection * Statics.vaultCheckDistance;
                    else
                        vOrigin += wallDirection * Statics.vaultCheckDistance_Run;
                    Debug.DrawRay(vOrigin, -Vector3.up * Statics.vaultCheckDistance);

                    if (Physics.Raycast(vOrigin, -Vector3.up, out vHit, Statics.vaultCheckDistance, ignoreLayers))
                    {
                        float hitY = vHit.point.y;
                        float diff = hitY - transform.position.y;
                        if (Mathf.Abs(diff) < vaultFloorHeightDifference)
                        {
                            vaulting = true;
                            targetVaultPosition = vHit.point;
                            willVault = true;
                            return;
                        }
                    }
                }
            }
        }

        public bool OnGround()
        {
            if (skipGroundCheck)
                return false;

            bool r = false;
            if (curState == CharStates.hold)
            {
                return false;
            }
            Vector3 origin = transform.position + (Vector3.up * 0.55f);

            RaycastHit hit = new RaycastHit();
            bool isHit = false;

            //quadratic = true;

            if (quadratic)
            {
                FindGround(origin, ref hit, ref isHit);
                if (!isHit)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 newOrigin = origin;

                        switch (i)
                        {
                            case 0:
                                newOrigin += Vector3.forward / 3;
                                break;
                            case 1:
                                newOrigin -= Vector3.forward / 3;
                                break;
                            case 2:
                                newOrigin -= Vector3.right / 3;
                                break;
                            case 3:
                                newOrigin += Vector3.right / 3;
                                break;

                        }
                        FindGround(newOrigin, ref hit, ref isHit);

                        if (isHit == true)
                            break;
                    }
                }

                r = isHit;

                if (r != false)
                {
                    Vector3 targetPosition = transform.position;
                    targetPosition.y = hit.point.y + groundOffset;
                    transform.position = targetPosition;
                }

            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 rightOffset = Vector3.zero;

                    switch (i)
                    {
                        
                        case 1:
                            rightOffset = transform.right * 0.2f;
                            break;
                        case 2:
                            rightOffset = -transform.right * 0.2f;
                            break;
                        case 0:
                        default:
                            break;
                    }


                    Vector3 backOrigin = transform.InverseTransformDirection(offsetBack);
                    backOrigin += transform.position + rightOffset;
                    backOrigin.y = transform.position.y + groundDistance;

                    bool backHit = false;

                    FindGround(backOrigin, ref hit, ref backHit);
                    Vector3 p1 = backOrigin;// + (-Vector3.up * groundDistance);
                    p1.y = transform.position.y;

                    if (backHit)
                        p1 = hit.point;

                    Vector3 frontOrigin = transform.InverseTransformDirection(offsetFront);
                    frontOrigin += transform.position+rightOffset;
                    frontOrigin.y = transform.position.y + groundDistance;

                    bool frontHit = false;

                    FindGround(frontOrigin, ref hit, ref frontHit);
                    Vector3 p2 = frontOrigin;
                    p2.y = p1.y - 0.15f;

                    if (frontHit)
                        p2 = hit.point;

                    if (frontHit || backHit)
                    {
                        float distance = Vector3.Distance(p2, p1);
                        distance /= 2;
                        if (groundAngle > 0)
                        {
                            distance += groundAngle;
                        }
                        else
                        {
                            distance += -groundAngle * 0.3f;
                        }

                        Vector3 median = ((p2 - p1) * distance) + p1;
                        Vector3 targetPosition = transform.position;
                        targetPosition.y = median.y + groundOffset;
                        transform.position = targetPosition;
                        r = true;

                        if (r)
                            break;
                    }
                    else
                    {
                        r = false;
                    }
                }
            }
            return r;
        }
        void FindGround(Vector3 origin, ref RaycastHit hit, ref bool isHit)
        {
            float dis = groundDistance;
            if (!quadratic)
                dis = groundDistance +   0.3f;

            Debug.DrawRay(origin, -Vector3.up * dis, Color.red);
            if(Physics.Raycast(origin,-Vector3.up,out hit, dis, ignoreLayers))
            {
                isHit = true;
            }
        }
        internal void RegularTick()
        {
            onGround = OnGround();
        }
        #endregion

        #region Init Phase
        public void Init()
        {
            inGame = true;
            CreateModel();
            SetupAnimator();
            AddControllerReferences();
            canJump = true;

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 3 | 1 << 8|1<<9);

            controllerCollider = GetComponent<Collider>();
            if (controllerCollider == null)
            {
                Debug.Log("No Collider found for the controller !!! ");
            }
        }

        internal void LegFront()
        {
            Vector3 ll = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Vector3 rl = anim.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 rel_ll = transform.InverseTransformPoint(ll);
            Vector3 rel_rr = transform.InverseTransformPoint(rl);

            bool left = rel_ll.z > rel_rr.z;
            anim.SetBool(Statics.mirrorJump, left);
        }

        void CreateModel()
        {
            activeModel = Instantiate(modelPrefab);
            activeModel.transform.parent = this.transform;
            activeModel.transform.localPosition = Vector3.zero;// where to instantiate our player
            activeModel.transform.localEulerAngles = Vector3.zero;
            activeModel.transform.localScale = Vector3.one;
        }
        void SetupAnimator()
        {
            anim = GetComponent<Animator>();
            Animator childAnim = activeModel.GetComponent<Animator>();
            anim.avatar = childAnim.avatar;
            Destroy(childAnim);
        }
        void AddControllerReferences()
        {
            gameObject.AddComponent<Rigidbody>();
            rBody = GetComponent<Rigidbody>();
            rBody.angularDrag = 999;
            rBody.drag = 2;//4
            rBody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationZ;
        }
        #endregion
       
    }

}
