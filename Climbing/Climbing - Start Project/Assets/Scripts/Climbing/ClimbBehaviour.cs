using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPC;

namespace Climbing
{
    public class ClimbBehaviour : MonoBehaviour
    {
        #region Variables
        public bool climbing;
        public bool lookOnZ;
        public float directThreshold = 1;
        public float minDistance = 2.5f;
        bool initClimb;
        bool hold;
        bool waitToStartClimb;
        bool waitForWrapUp;
        bool dropOnLedge;
        bool hasRootmotion;
        bool isOnOpposite;

        Animator anim;
        ClimbIK ik;
        StateManager states;

        Manager curManager;
        Manager lastCheckedManager;
        Point targetPoint;
        Point curPoint;
        Point prevPoint;
        Neighbour neighbour;
        ConnectionType curConnection;
        GameObject dismountPointGO;
        Neighbour dismountNeighbour;
        Neighbour fallNeighbour;
        Neighbour jumpBackNeighbour;
        Neighbour jumpForwardNeighbour;

        ClimbStates climbState;
        ClimbStates targetState;

        public enum ClimbStates
        {
            onPoint,
            betweenPoints,
            inTransit
        }

        CurvesHolder curveHolder;
        BezierCurve curCurve;

        Vector3 _startPos;
        Vector3 _targetPos;
        float _distance;
        float _t;
        bool initTransit;
        bool rootReached;
        bool ikLandSideReached;
        bool ikFollowSideReached;
        bool skipDepthCheck;

        bool lockInput;
        Vector3 inputDirection;
        Vector3 targetPosition;

        public Vector3 rootOffset = new Vector3(0, -0.86f, 0);
        public float speed_linear = 1.5f;
        public float speed_direct = 2;
        public float speed_dropLedge = 1.5f;
        public float jump_back_speed = 1.5f;

        public AnimationCurve a_jumpingCurve;
        public AnimationCurve a_hangingToBrace;
        public AnimationCurve a_mountCurve;
        public AnimationCurve a_zeroToOne;
        public bool enableRootMovement;
        float _rmMax = 0.25f;
        float _rmT;

        LayerMask lm;

        AngleCheck curAngleCheck;
        #endregion

        #region Starting
        void SetCurveReferences()
        {
            GameObject chPrefab = Resources.Load("CurvesHolder") as GameObject;
            GameObject chGO = Instantiate(chPrefab) as GameObject;

            curveHolder = chGO.GetComponent<CurvesHolder>();

        }

        public void Start()
        {
            states = GetComponent<StateManager>();
            anim = GetComponentInChildren<Animator>();
            ik = GetComponentInChildren<ClimbIK>();
            SetCurveReferences();
            CreateDirections();

            GameObject disPrefab = Resources.Load("Dismount") as GameObject;
            dismountPointGO = Instantiate(disPrefab) as GameObject;
            dismountNeighbour = new Neighbour();
            dismountNeighbour.target = dismountPointGO.GetComponentInChildren<Point>();
            dismountNeighbour.target.dismountPoint = true;
            dismountNeighbour.cType = ConnectionType.dismount;
            dismountNeighbour.customConnection = true;

            fallNeighbour = new Neighbour();
            fallNeighbour.cType = ConnectionType.fall;
            fallNeighbour.customConnection = true;

            jumpBackNeighbour = new Neighbour();
            jumpBackNeighbour.cType = ConnectionType.jumpBack;
            jumpBackNeighbour.customConnection = true;

            jumpForwardNeighbour = new Neighbour();
            jumpForwardNeighbour.cType = ConnectionType.hanging_jump_air;
            jumpForwardNeighbour.customConnection = true;

            lm = (1 << gameObject.layer) | (1 << 3);
            lm = ~lm;

        }

        public void FixedUpdate()
        {
            if (climbing)
            {
                if (!waitToStartClimb)
                {
                    if (!hold)
                    {
                        HandleClimbing();
                        InititateFallOff();
                    }
                }
                else
                {
                    InitClimbing();
                    HandleMount();
                }
            }
            else
            {
                if (initClimb)
                {
                    transform.parent = null;
                    initClimb = false;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    LookForClimbSpot();
                }
                CharacterOnEdge();
            }
        }

        private void CharacterOnEdge()
        {
            if (!states.groundForward)
            {
                Vector3 origin = transform.position;
                origin += transform.forward;
                origin -= Vector3.up / 3;
                Vector3 direction = transform.position - origin;
                direction.y = 0;
                RaycastHit hit;

                Debug.DrawRay(origin, direction, Color.red, 5);

                if (Physics.Raycast(origin, direction, out hit, 1, lm))
                {
                    if (hit.transform.GetComponentInChildren<Manager>())
                    {
                        Manager tm = hit.transform.GetComponentInChildren<Manager>();

                        Point closestPoint = tm.ReturnClosest(transform.position);

                        float distanceToPoint = Vector3.Distance(transform.position, closestPoint.transform.parent.position);

                        if (distanceToPoint < 5)
                        {
                            if (Input.GetKey(KeyCode.Space))
                            {
                                curManager = tm;
                                targetPoint = closestPoint;
                                targetPosition = closestPoint.transform.position;
                                curPoint = closestPoint;
                                climbing = true;
                                lockInput = true;
                                dropOnLedge = true;
                                states.DisableController();
                                waitToStartClimb = true;

                                anim.SetBool("climbing", true);
                                hasRootmotion = anim.applyRootMotion;
                                anim.applyRootMotion = false;
                                hold = true;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Neighbour Manager

        Vector3[] availableDirections;
        void CreateDirections()
        {
            availableDirections = new Vector3[10];
            availableDirections[0] = new Vector3(1, 0, 0);
            availableDirections[1] = new Vector3(-1, 0, 0);
            availableDirections[2] = new Vector3(0, 1, 0);
            availableDirections[3] = new Vector3(0, -1, 0);
            availableDirections[4] = new Vector3(-1, -1, 0);
            availableDirections[5] = new Vector3(1, 1, 0);
            availableDirections[6] = new Vector3(1, -1, 0);
            availableDirections[7] = new Vector3(-1, 1, 0);
            availableDirections[8] = new Vector3(0, 0, -1);
            availableDirections[9] = new Vector3(0, 0, 1);
        }

        Neighbour ReturnNeighbour(Vector3 inpd, Point curPoint, Manager m)
        {
            if (m == null)
                return null;

            Neighbour retVal = null;

            Neighbour n = new Neighbour();

            curManager = m;
            Point tp = NeighbourPoint(inpd, curPoint, curManager);

            if (tp == null)
            {
                return null;
            }

            n.target = tp;

            float distance = Vector3.Distance(curPoint.transform.position, tp.transform.position);

            if (!lookOnZ)
            {
                if (distance < minDistance)
                {
                    n.cType = (distance < directThreshold) ? ConnectionType.inBetween : ConnectionType.direct;

                    if (n.cType == ConnectionType.direct && inpd.x != 0 && inpd.y != 0)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                n.cType = ConnectionType.direct;
            }

            retVal = n;

            return retVal;
        }

        private Point NeighbourPoint(Vector3 targetDirection, Point from, Manager m)
        {
            Point retVal = null;

            List<Point> canidates = CanidatePointsOnDirection(targetDirection, from, m);

            retVal = ReturnClosest(from, canidates);

            return retVal;
        }


        private List<Point> CanidatePointsOnDirection(Vector3 targetDirection, Point from, Manager m)
        {
            List<Point> retval = new List<Point>();

            for (int p = 0; p < m.allPoints.Count; p++)
            {
                Point targetPoint = m.allPoints[p];

                if (targetPoint == from)
                    continue;

                Vector3 relativePosition = from.transform.parent.InverseTransformPoint(targetPoint.transform.parent.position);

                if (IsDirectionValid(targetDirection, relativePosition))
                {
                    retval.Add(targetPoint);
                }
            }
            return retval;
        }

        private bool IsDirectionValid(Vector3 targetDirection, Vector3 canidate)
        {

            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;
            float angle = Mathf.Atan2(canidate.x, canidate.y) * Mathf.Rad2Deg;
            float depthAngle = Mathf.Atan2(canidate.x, canidate.z) * Mathf.Rad2Deg;

            if (targetDirection.y != 0)
            {
                targetAngle = Mathf.Abs(targetAngle);
                angle = Mathf.Abs(angle);

                if (angle < targetAngle + 22.5f && angle > targetAngle - 22.5f)
                {
                    return true;
                }
            }

            if (targetDirection.z != 0)
            {
                targetAngle = Mathf.Abs(Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg);
                angle = Mathf.Abs(Mathf.Atan2(canidate.x, canidate.z) * Mathf.Rad2Deg);

                if (angle < targetAngle + 22.5f && angle > targetAngle - 22.5f)
                {
                    return true;
                }
            }
            else
            {
                if (angle < targetAngle + 22.5f && angle > targetAngle - 22.5f)
                {
                    if (Mathf.Abs(depthAngle) > 60 && Mathf.Abs(depthAngle) < 100 || skipDepthCheck)
                    {

                        return true;
                    }
                }
            }
            return false;
        }

        Point ReturnClosest(Point cp, List<Point> l)
        {
            Point retVal = null;

            float minDist = Mathf.Infinity;

            for (int i = 0; i < l.Count; i++)
            {
                float tempDist = Vector3.Distance(cp.transform.position, l[i].transform.position);

                bool inAngle = InAngle(curPoint, l[i], curAngleCheck);

                if (tempDist < minDist && l[i] != cp && inAngle)
                {
                    minDist = tempDist;
                    retVal = l[i];
                }
            }
            return retVal;
        }

        Point ReturnPoint(Vector3 inpd, Point curPoint, Manager m)
        {
            if (m == null)
                return null;

            Point retval = null;

            Point tp = NeighbourPoint(inpd, curPoint, m);
            retval = tp;
            if (retval != null)
            {
                lastCheckedManager = curManager;
                curManager = m;
            }
            return retval;
        }
        Manager LookForManagerBehind()
        {
            Manager retVal = null;

            RaycastHit hit;
            Vector3 origin = transform.position;
            Vector3 direction = -transform.forward;

            Debug.DrawRay(origin, direction * 5);

            if (Physics.Raycast(origin, direction, out hit, 5, lm))
            {
                if (hit.transform.root.GetComponentInChildren<Manager>())
                {
                    retVal = hit.transform.root.GetComponentInChildren<Manager>();
                }
            }
            // if u want behind and diagonally managers
            // do the above same but with different direction
            return retVal;
        }

        bool CanDismount()
        {
            bool retVal = false;

            Vector3 worldP = curPoint.transform.position;
            Vector3 aboveOrigin = worldP - transform.forward;

            bool above = GetHit(aboveOrigin, Vector3.up, 1.5f, lm);

            Debug.DrawRay(aboveOrigin, Vector3.up * 1.5f, Color.red);
            if (!above)
            {
                Vector3 forwardOrigin = aboveOrigin + Vector3.up * 2;

                Debug.DrawRay(forwardOrigin, transform.forward * 2, Color.yellow);
                bool forward = GetHit(forwardOrigin, transform.forward, 2, lm);

                if (!forward)
                {
                    Vector3 disOrigin = worldP + (transform.forward + Vector3.up * 2);
                    RaycastHit hit;

                    Debug.DrawRay(disOrigin, -Vector3.up * 2, Color.green);

                    if (Physics.Raycast(disOrigin, -Vector3.up, out hit, 2))
                    {
                        Vector3 gp = hit.point;
                        gp.y += 0.04f + Mathf.Abs(dismountNeighbour.target.transform.localPosition.y);

                        dismountPointGO.transform.position = gp;
                        dismountPointGO.transform.rotation = transform.rotation;
                        retVal = true;

                    }
                }

            }
            return retVal;
        }

        private bool GetHit(Vector3 origin, Vector3 direction, float dis, LayerMask lm)
        {
            bool retVal = false;

            RaycastHit hit;

            if (Physics.Raycast(origin, direction, out hit, dis, lm))
            {
                retVal = true;
            }
            return retVal;
        }

        bool CanFall()
        {
            bool retVal = false;
            RaycastHit hit;
            if (Physics.Raycast(curPoint.transform.position, -Vector3.up, out hit, 3, lm))
            {
                retVal = true;
            }
            return retVal;
        }

        void FixHipPositions(Point p)
        {
            if (p.pointType == PointType.hanging)
            {
                //Vector3 targetP = Vector3.zero;
                //targetP.y = -1.5f;
                //p.transform.localPosition = targetP;

                // we might chage this in future

            }
            if (p.pointType == PointType.braced)
            {
                Vector3 targetP = Vector3.zero;
                targetP.y = -1.06f;
                targetP.z = -0.3703f;
                p.transform.localPosition = targetP;
            }
        }

        Manager LookForManagerSides(float x)
        {
            Manager retVal = null;

            RaycastHit hit;
            Vector3 origin = transform.position + -transform.forward;
            Vector3 direction = transform.right * x;

            Debug.DrawRay(origin, direction * 5);//5

            bool hitSides = GetHit(origin, direction, 2, lm);//2

            if (hitSides == false)
            {
                Vector3 towardsOrigin = origin + direction * 5;//5

                Debug.DrawRay(towardsOrigin, transform.forward * 5);//5

                if (Physics.Raycast(towardsOrigin, transform.forward, out hit, 5, lm))//5
                {
                    if (hit.transform.root.GetComponentInChildren<Manager>())
                    {
                        retVal = hit.transform.root.GetComponentInChildren<Manager>();

                    }
                }
            }
            return retVal;
        }

        Manager LookForManager(Vector3 direction)
        {
            Manager retVal = null;

            Vector3 worldP = curPoint.transform.position;
            Vector3 aboveOrigin = worldP - transform.forward;

            bool above = GetHit(aboveOrigin, direction, 1.5f, lm);

            Debug.DrawRay(aboveOrigin, direction * 1.5f, Color.red);

            if (!above)
            {
                Vector3 forwardOrigin = aboveOrigin + direction * 2;

                Debug.DrawRay(forwardOrigin, transform.forward * 2, Color.yellow);

                RaycastHit hit;

                if (Physics.Raycast(forwardOrigin, transform.forward, out hit, 2, lm))
                {
                    if (hit.transform.GetComponentInChildren<Manager>())
                    {
                        retVal = hit.transform.GetComponentInChildren<Manager>();
                    }
                }
            }

            return retVal;
        }

        private Manager LookForManager_Corner_In(Vector3 inpD)
        {
            Manager retVal = null;
            Vector3 origin = curPoint.transform.position;
            Vector3 sides = curPoint.transform.right * inpD.x;
            RaycastHit hit;
            Debug.DrawRay(origin, sides * 1, Color.red);

            if (Physics.Raycast(origin, sides, out hit, 1, lm))
            {
                if (hit.transform.GetComponentInChildren<Manager>())
                {
                    Manager m = hit.transform.GetComponentInChildren<Manager>();

                    retVal = m;
                }
            }
            return retVal;
        }

        private Manager LookForManager_Corner_Out(Vector3 inpd)
        {
            Manager retval = null;

            Vector3 origin = curPoint.transform.position;
            Vector3 sides = curPoint.transform.right * inpd.x;
            bool clearSides = GetHit(origin, sides, 1, lm);
            Debug.DrawRay(origin, sides * 1, Color.red);

            if (!clearSides)
            {
                Vector3 newOrigin = origin + sides;
                bool clearForward = GetHit(newOrigin, curPoint.transform.forward, 1, lm);
                Debug.DrawRay(newOrigin, transform.forward * 1, Color.yellow);

                if (!clearForward)
                {
                    Vector3 cornerOrigin = newOrigin + transform.forward;
                    Vector3 cornerDir = curPoint.transform.right * (-inpd.x);
                    Debug.DrawRay(cornerOrigin, cornerDir * 2, Color.blue);
                    RaycastHit cornerHit;

                    if (Physics.Raycast(cornerOrigin, cornerDir, out cornerHit, 2, lm))
                    {
                        if (cornerHit.transform.GetComponentInChildren<Manager>())
                        {
                            Manager m = cornerHit.transform.GetComponentInChildren<Manager>();

                            retval = m;
                        }
                    }
                }
            }
            return retval;
        }

        Manager LookForManager_Corner(Vector3 inpd)
        {
            Manager retVal = null;

            Vector3 origin = curPoint.transform.position;
            Vector3 sides = curPoint.transform.right * inpd.x;
            bool clearSides = GetHit(origin, sides, 1, lm);
            Debug.DrawRay(origin, sides * 1, Color.red);
            if (!clearSides)
            {
                Vector3 newOrigin = origin + sides;
                bool clearForward = GetHit(newOrigin, curPoint.transform.forward, 1, lm);
                Debug.DrawRay(newOrigin, transform.forward * 1, Color.yellow);
                if (!clearForward)
                {
                    Vector3 cornerOrigin = newOrigin + transform.forward;
                    Vector3 cornerDir = curPoint.transform.right * -inpd.x;
                    Debug.DrawRay(cornerOrigin, cornerDir * 2, Color.blue);
                    RaycastHit cornerhit;
                    if (Physics.Raycast(cornerOrigin, cornerDir, out cornerhit, 2, lm))
                    {
                        if (cornerhit.transform.GetComponentInChildren<Manager>())
                        {
                            Manager m = cornerhit.transform.GetComponentInChildren<Manager>();
                            retVal = m;
                        }
                    }
                }

            }
            return retVal;
        }
        Neighbour CheckManagerCorner(Vector3 inpD, Manager targetManager)
        {
            Neighbour n = new Neighbour();
            Point tp = null;

            if (targetManager != null)
            {
                tp = ReturnPoint(inpD, curPoint, targetManager);

            }
            n.target = tp;
            return n;
        }
        Manager LookForManager_Around()
        {
            Manager retVal = null;
            Vector3 origin = curPoint.transform.position + transform.forward;
            Vector3 direction = -curPoint.transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(origin, direction, out hit, 1, lm))
            {
                if (hit.transform.GetComponentInChildren<Manager>())
                {
                    Manager m = hit.transform.GetComponentInChildren<Manager>();
                    retVal = m;
                }
            }
            return retVal;
        }

        Manager LookForManager_Forward()
        {
            Manager retVal = null;

            Vector3 worldP = curPoint.transform.position;
            RaycastHit hit;
            Debug.DrawRay(worldP, transform.forward * 15, Color.blue);
            if (Physics.Raycast(worldP, transform.forward, out hit, 15, lm))
            {

                if (hit.transform.GetComponentInChildren<Manager>())
                {
                    Manager m = hit.transform.GetComponentInChildren<Manager>();

                    if (m != curManager)
                        retVal = m;
                }
            }

            return retVal;
        }

        Neighbour CheckForManagerAhead(Vector3 inpD, Manager targetManager)
        {
            Neighbour n = new Neighbour();
            Point tp = null;

            if (inpD.z > 0)
            {
                if (targetManager != null)
                {
                    tp = ReturnPoint(inpD, curPoint, targetManager);
                }
            }
            n.target = tp;

            return n;
        }
        Neighbour CheckForNearManager(Vector3 inpD, Manager targetManager)
        {
            Neighbour n = new Neighbour();
            Point tp = null;

            if (inpD.y > 0)
            {
                if (CanDismount())
                {
                    n = dismountNeighbour;

                    return n;
                }
                else
                {
                    targetManager = LookForManager(transform.up);
                    if (targetManager != null)
                    {
                        tp = ReturnPoint(inpD, curPoint, targetManager);
                    }
                }
            }
            if (inpD.y < 0)
            {
                targetManager = LookForManager(-transform.up);
                if (targetManager != null && targetManager != curManager)
                {
                    tp = ReturnPoint(inpD, curPoint, targetManager);
                }
                else if (CanFall())
                {
                    fallNeighbour.target = curPoint;
                    n = fallNeighbour;
                    return n;
                }
            }
            if (inpD.y == 0)
            {
                if (inpD.x != 0)
                {
                    targetManager = LookForManagerSides(inpD.x);

                    if (targetManager != null)
                    {
                        tp = ReturnPoint(inpD, curPoint, targetManager);

                    }
                }
            }
            n.target = tp;
            return n;
        }
        Neighbour DoConnectionTypeChecks(Vector3 inpD, Neighbour n, bool hanging = false)
        {
            if (n.customConnection)
                return n;

            if (n.target)
            {
                float distance = Vector3.Distance(curPoint.transform.parent.position, n.target.transform.parent.position);

                if (distance < minDistance)
                {
                    n.cType = (distance < directThreshold) ? ConnectionType.inBetween : ConnectionType.direct;

                    if (n.cType == ConnectionType.direct && inpD.x != 0 && inpD.y != 0)
                    {
                        n.target = null;
                    }

                }
                else
                {
                    n.target = null;
                }
            }
            if (hanging)
            {
                if (n.cType == ConnectionType.direct)
                {
                    if (inpD.z < 0)
                    {
                        n.target = null;
                    }
                }
            }
            if (n.target == null)
                curManager = lastCheckedManager;
            return n;
        }

        #endregion

        #region Mains
        private void HandleMount()
        {
            if (!initTransit)
            {
                initTransit = true;
                enableRootMovement = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                init_JumpBack = false;
                init_JumpForward = false;

                _t = 0;
                rotationT = 0;
                _startPos = transform.position;
                _targetPos = targetPosition + rootOffset;

                curCurve = (dropOnLedge) ? curveHolder.ReturnCurve(CurveType.dropLedge)
                    : curveHolder.ReturnCurve(CurveType.mount);

                curCurve.transform.rotation = targetPoint.transform.rotation;
                BezierPoint[] points = curCurve.GetAnchorPoints();
                points[0].transform.position = _startPos;
                points[points.Length - 1].transform.position = _targetPos;

                if (dropOnLedge)
                    anim.CrossFade("dropLedge", 0.4f);

                anim.SetFloat("Stance",
                    (targetPoint.pointType == PointType.braced) ? 0 : 1);

            }
            if (enableRootMovement)
            {
                _t += Time.deltaTime * 2;
            }
            if (_t >= 0.99f)
            {
                _t = 1;
                waitToStartClimb = false;
                lockInput = false;
                initTransit = false;
                ikLandSideReached = false;
                climbState = targetState;
                dropOnLedge = false;
            }

            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;

            HandleWeightAll(_t, a_mountCurve);

            HandleRotation();
        }

        private void HandleRotation()
        {
            Vector3 targetDir = targetPoint.transform.forward;

            if (targetDir == Vector3.zero)
            {
                targetDir = transform.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            rotationT += Time.deltaTime * 5;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationT);

        }
        void HandleRotationLocal()
        {
            if (!init_JumpBack)
            {

                rotationT = 0;
                init_JumpBack = true;
            }

            Quaternion targetRot = curPoint.transform.localRotation;

            rotationT += Time.deltaTime * 2;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, rotationT);
        }
        private void HandleWeightAll(float t, AnimationCurve aCurve)
        {
            float inf = aCurve.Evaluate(t);
            ik.AddWeightInfluenceAll(1 - inf);//1-inf

            if (curPoint.pointType == PointType.hanging && targetPoint.pointType == PointType.braced)
            {
                float inf2 = a_zeroToOne.Evaluate(t);

                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, inf2);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, inf2);
            }
            if (curPoint.pointType == PointType.hanging && targetPoint.pointType == PointType.hanging)
            {
                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
            }
        }

        private void InitClimbing()
        {
            if (!initClimb)
            {
                initClimb = true;

                if (ik != null)
                {
                    ik.UpdateAllPointsOnOne(targetPoint);
                    ik.UpdateAllTargetPositions(targetPoint);
                    ik.ImmediatePlaceHelpers();
                }

                curConnection = ConnectionType.direct;
                targetState = ClimbStates.onPoint;
                anim.SetBool("Move", false);
                anim.SetInteger("JumpType", 0);
            }
        }

        private void InititateFallOff()
        {
            if (climbState == ClimbStates.onPoint)
            {
                if (Input.GetKey(KeyCode.X))
                {
                    climbing = false;
                    initTransit = false;
                    ik.AddWeightInfluenceAll(0);
                    states.EnableController();
                    anim.SetBool("climbing", false);
                }
            }
        }

        private void HandleClimbing()
        {
            if (!lockInput)
            {
                lookOnZ = Input.GetKey(KeyCode.LeftShift);

                inputDirection = Vector3.zero;

                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");

                inputDirection = ConvertToInputDirection(h, v);
                if (climbState == ClimbStates.onPoint)
                {
                    float debugAngle = Vector3.Angle(curPoint.transform.forward, transform.forward);
                    if (debugAngle > 1)
                    {
                        inputDirection = Vector3.zero;
                        HandleRotationLocal();
                    }
                }
                if (inputDirection != Vector3.zero)
                {
                    switch (climbState)
                    {
                        case ClimbStates.onPoint:
                            OnPoint(inputDirection);
                            break;
                        case ClimbStates.betweenPoints:
                            BetweenPoints(inputDirection);
                            break;
                    }
                }

                transform.parent = curPoint.transform.parent;

                if (climbState == ClimbStates.onPoint)
                {
                    ik.UpdateAllTargetPositions(curPoint);
                    ik.ImmediatePlaceHelpers();
                }

            }
            else
            {
                InTransit(inputDirection);
            }
        }

        private void InTransit(Vector3 inputD)
        {
            switch (curConnection)
            {
                case ConnectionType.inBetween:
                    UpdateLinearvariables();
                    Linear_RootMovement();
                    LerpIKLandingSide_Linear();
                    WrapUp();
                    break;
                case ConnectionType.direct:
                    UpdateDirectVariables(inputDirection);
                    Direct_RootMovement(speed_direct);
                    DirectHandleIK();
                    HandleRotation();
                    WrapUp(true);
                    break;
                case ConnectionType.dismount:
                    HandleDismountVariables();
                    Dismount_RootMovement();
                    HandleDismountIK();
                    DismountWrapUp();
                    break;
                case ConnectionType.jumpBack:
                    JumpBackwards();
                    break;
                case ConnectionType.jumpBack_onManager:
                    UpdateDirectVariables(inputDirection);
                    Direct_RootMovement(jump_back_speed);
                    DirectHandleIK();
                    HandleRotation_Controlled(180, 2);
                    WrapUp(true);
                    break;
                case ConnectionType.hanging_jump_forward:
                    UpdateDirectVariables(inputDirection);
                    Direct_RootMovement(speed_direct);
                    DirectHandleIK();
                    HandleRotation();
                    WrapUp(true);
                    break;
                case ConnectionType.hanging_jump_air:
                    JumpForward();
                    break;
                case ConnectionType.hanging_turn_around:
                    UpdateLinearvariables();
                    Linear_RootMovement();
                    LerpIKLandingSide_Linear();
                    HandleRotation_Controlled(-90, 2);
                    WrapUp();
                    break;
                case ConnectionType.corner_in:
                case ConnectionType.corner_out:
                    UpdateLinearvariables();
                    Linear_RootMovement();
                    LerpIKLandingSide_Linear();
                    WrapUp();
                    break;
            }
        }

        private void HandleRotation_Controlled(float value, float rotSpeed)
        {
            if (!init_JumpBack)
            {
                targetAngle = transform.localRotation.y + value;
                init_JumpBack = true;
            }
            if (enableRootMovement)
            {
                rotationT += Time.deltaTime * rotSpeed;
            }
            if (rotationT > 1)
                rotationT = 1;

            transform.localRotation = Quaternion.Euler(
                transform.localRotation.x,
                Mathf.Lerp(transform.localRotation.y, targetAngle, rotationT),
                transform.localRotation.z);

        }
        #endregion

        #region Dismount
        private void DismountWrapUp()
        {
            if (rootReached)
            {
                climbing = false;
                initTransit = false;
                anim.SetBool("climbing", false);
                GetComponent<StateManager>().EnableController();
            }
        }

        private void HandleDismountIK()
        {
            if (enableRootMovement)
                _ikT += Time.deltaTime * 3;

            _fikT += Time.deltaTime * 2;

            HandleIKWeight_Dismount(_ikT, _fikT, 1, 0);
        }

        private void HandleIKWeight_Dismount(float ht, float ft, int from, int to)
        {
            float t1 = ht * 3;

            if (t1 > 1)
            {
                t1 = 1;
                ikLandSideReached = true;
            }

            float handsWeight = Mathf.Lerp(from, to, t1);
            ik.InfluenceWeight(AvatarIKGoal.LeftHand, handsWeight);
            ik.InfluenceWeight(AvatarIKGoal.RightHand, handsWeight);

            float t2 = ft * 1;

            if (t2 > 1)
            {
                t2 = 1;
                ikFollowSideReached = true;
            }

            float feetWeight = Mathf.Lerp(from, to, t2);
            ik.InfluenceWeight(AvatarIKGoal.LeftFoot, feetWeight);
            ik.InfluenceWeight(AvatarIKGoal.RightFoot, feetWeight);
        }

        private void Dismount_RootMovement()
        {
            if (enableRootMovement)
            {
                _t += Time.deltaTime / 2;
            }

            if (_t >= 0.99f)
            {
                _t = 1;
                rootReached = true;
            }

            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;
        }

        private void HandleDismountVariables()
        {
            if (!initTransit)
            {
                initTransit = true;
                enableRootMovement = false;
                rootReached = false;
                ikLandSideReached = false;
                ikFollowSideReached = false;
                _t = 0;
                rotationT = 0;
                _rmT = 0;
                _startPos = transform.position;
                _targetPos = targetPosition;

                curCurve = curveHolder.ReturnCurve(CurveType.dismount);
                BezierPoint[] points = curCurve.GetAnchorPoints();
                curCurve.transform.rotation = transform.rotation;
                points[0].transform.position = _startPos;
                points[points.Length - 1].transform.position = _targetPos;

                _ikT = 0;
                _fikT = 0;
            }
        }
        #endregion

        #region Direct
        private void DirectHandleIK()
        {
            if (inputDirection.y != 0)
            {
                LerpIKHands_Direct();
                LerpIKFeet_Direct();
            }
            else
            {
                LerpIKLandingSide_Direct();
                LerpIKFollowSide_Direct();
            }
        }
        void LerpIKHands_Direct()
        {
            if (enableRootMovement)
                _ikT += Time.deltaTime * 5;

            if (_ikT > 1)
            {
                _ikT = 1;
                ikLandSideReached = true;
            }

            Vector3 lhPosition = Vector3.LerpUnclamped(_ikStartPos[0], _ikTargetPos[0], _ikT);
            ik.UpdateTargetPositions(AvatarIKGoal.LeftHand, lhPosition);

            Vector3 rhPosition = Vector3.LerpUnclamped(_ikStartPos[2], _ikTargetPos[2], _ikT);
            ik.UpdateTargetPositions(AvatarIKGoal.RightHand, rhPosition);
        }

        void LerpIKFeet_Direct()
        {
            if (targetPoint.pointType == PointType.hanging)
            {
                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
            }
            else
            {
                if (enableRootMovement)
                    _fikT += Time.deltaTime * 5;

                if (_fikT > 1)
                {
                    _fikT = 1;
                    ikFollowSideReached = true;
                }

                Vector3 lfPosition = Vector3.LerpUnclamped(_ikStartPos[1], _ikTargetPos[1], _fikT);
                ik.UpdateTargetPositions(AvatarIKGoal.LeftFoot, lfPosition);

                Vector3 rfPosition = Vector3.LerpUnclamped(_ikStartPos[3], _ikTargetPos[3], _fikT);
                ik.UpdateTargetPositions(AvatarIKGoal.RightFoot, rfPosition);
            }
        }

        void LerpIKLandingSide_Direct()
        {
            if (enableRootMovement)
                _ikT += Time.deltaTime * 3.2f;

            if (_ikT > 1)
            {
                _ikT = 1;
                ikLandSideReached = true;
            }

            Vector3 landPosition = Vector3.LerpUnclamped(_ikStartPos[0], _ikTargetPos[0], _ikT);
            ik.UpdateTargetPositions(ik_L, landPosition);

            if (targetPoint.pointType == PointType.hanging)
            {
                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
            }
            else
            {
                Vector3 followPosition = Vector3.LerpUnclamped(_ikStartPos[1], _ikTargetPos[1], _ikT);
                ik.UpdateTargetPositions(ik_F, followPosition);
            }
        }

        void LerpIKFollowSide_Direct()
        {
            if (enableRootMovement)
                _fikT += Time.deltaTime * 2.6f;

            if (_fikT > 1)
            {
                _fikT = 1;
                ikFollowSideReached = true;
            }

            Vector3 landPosition = Vector3.LerpUnclamped(_ikStartPos[2], _ikTargetPos[2], _fikT);
            ik.UpdateTargetPositions(ik.ReturnOppositeIK(ik_L), landPosition);

            if (targetPoint.pointType == PointType.hanging)
            {
                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
            }
            else
            {

                Vector3 followPosition = Vector3.LerpUnclamped(_ikStartPos[3], _ikTargetPos[3], _fikT);
                ik.UpdateTargetPositions(ik.ReturnOppositeIK(ik_F), followPosition);
            }
        }
        private void Direct_RootMovement(float speed)
        {
            float targetSpeed = speed;

            if (enableRootMovement)
            {
                _t += Time.deltaTime * targetSpeed;
            }
            else
            {
                if (_rmT < _rmMax)
                {
                    _rmT += Time.deltaTime;
                }
                else
                    enableRootMovement = true;
            }

            if (_t > 0.95f)
            {
                _t = 1;
                rootReached = true;
            }

            if (!lookOnZ)
                HandleWeightAll(_t, a_jumpingCurve);
            else
                HandleWeightAll(_t, a_mountCurve);

            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;

            // HandleRotation();
        }

        private void UpdateDirectVariables(Vector3 inpD)
        {
            if (!initTransit)
            {
                initTransit = true;
                enableRootMovement = false;
                rootReached = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                init_JumpBack = false;
                init_JumpForward = false;
                _t = 0;
                rotationT = 0;
                _rmT = 0;
                _targetPos = targetPosition + rootOffset;
                _startPos = transform.position;

                bool vertical = (Mathf.Abs(inpD.y) > 0.1f);
                curCurve = FindCurveByInput(vertical, inpD);
                curCurve.transform.rotation = curPoint.transform.rotation;

                BezierPoint[] points = curCurve.GetAnchorPoints();
                points[0].transform.position = _startPos;
                points[points.Length - 1].transform.position = _targetPos;

                InitIK_Direct(inputDirection);
            }
        }

        private BezierCurve FindCurveByInput(bool vertical, Vector3 inpd)
        {
            if (!vertical)
            {
                if (inpd.x > 0)
                {
                    return curveHolder.ReturnCurve(CurveType.right);

                }
                else
                {
                    return curveHolder.ReturnCurve(CurveType.left);

                }
            }
            else
            {
                if (!lookOnZ)
                {
                    if (curPoint.pointType != PointType.hanging)
                    {
                        if (inpd.y > 0)
                        {
                            return curveHolder.ReturnCurve(CurveType.up);
                        }
                        else
                        {
                            return curveHolder.ReturnCurve(CurveType.down);

                        }
                    }
                    else
                    {
                        if (inpd.z > 0)
                        {
                            return null;
                        }
                        else
                        {
                            return curveHolder.ReturnCurve(CurveType.forward);
                        }
                    }
                }
                else
                {
                    if (inpd.y > 0)
                    {
                        return curveHolder.ReturnCurve(CurveType.forward);
                    }
                    else
                    {
                        return curveHolder.ReturnCurve(CurveType.back);
                    }
                }

            }
        }

        private void InitIK_Direct(Vector3 directionToPoint)
        {
            if (directionToPoint.y != 0)
            {
                _fikT = 0;
                _ikT = 0;

                UpdateIKTarget(0, AvatarIKGoal.LeftHand, targetPoint);
                UpdateIKTarget(1, AvatarIKGoal.LeftFoot, targetPoint);

                UpdateIKTarget(2, AvatarIKGoal.RightHand, targetPoint);
                UpdateIKTarget(3, AvatarIKGoal.RightFoot, targetPoint);

            }
            else
            {
                InitIK(directionToPoint, false);
                InitIKOpposite();
            }

        }
        #endregion

        #region WrapUp

        private void WrapUp(bool direct = false)
        {
            if (rootReached)
            {
                if (!anim.GetBool("Jump"))
                {
                    if (!waitForWrapUp)
                    {
                        StartCoroutine(WrapUpTransition(0.05f));
                        waitForWrapUp = true;
                    }
                }
            }
        }

        IEnumerator WrapUpTransition(float t)
        {
            yield return new WaitForSeconds(t);
            climbState = targetState;

            if (climbState == ClimbStates.onPoint)
            {
                curPoint = targetPoint;
            }
            initTransit = false;
            lockInput = false;
            inputDirection = Vector3.zero;
            waitForWrapUp = false;
        }

        #endregion

        #region Linear
        private void LerpIKLandingSide_Linear()
        {
            float speed = speed_linear * Time.deltaTime;
            float lerpSpeed = speed / _distance;

            _ikT += lerpSpeed * 2;

            if (_ikT > 1)
            {
                _ikT = 1;
                ikLandSideReached = true;
            }

            Vector3 ikPosition = Vector3.LerpUnclamped(_ikStartPos[0], _ikTargetPos[0], _ikT);
            ik.UpdateTargetPositions(ik_L, ikPosition);

            _fikT += lerpSpeed * 2;
            if (_fikT > 1)
            {
                _fikT = 1;
                ikFollowSideReached = true;
            }
            if (targetPoint.pointType == PointType.hanging)
            {
                ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
            }
            else
            {
                Vector3 followSide = Vector3.LerpUnclamped(_ikStartPos[1], _ikTargetPos[1], _fikT);
                ik.UpdateTargetPositions(ik_F, followSide);
            }
        }

        private void Linear_RootMovement()
        {
            float speed = speed_linear * Time.deltaTime;
            float lerpSpeed = speed / _distance;
            _t += lerpSpeed;

            if (_t > 1)
            {
                _t = 1;
                rootReached = true;
            }

            Vector3 currentPosition = Vector3.LerpUnclamped(_startPos, _targetPos, _t);
            transform.position = currentPosition;

            HandleRotation();
        }

        private void UpdateLinearvariables()
        {
            if (!initTransit)
            {
                initTransit = true;
                enableRootMovement = true;
                rootReached = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                init_JumpForward = false;
                init_JumpBack = false;
                _t = 0;
                rotationT = 0;
                _startPos = transform.position;
                _targetPos = targetPosition + rootOffset;
                Vector3 directionToPoint = (_targetPos - _startPos).normalized;

                bool twoStep = (targetState == ClimbStates.betweenPoints);
                Vector3 back = -transform.forward * 0.05f;

                bool diffType = targetPoint.pointType != curPoint.pointType;
                Vector3 down = -transform.up * 0.2f;

                if (diffType)
                {
                    if (curPoint.pointType == PointType.hanging)
                        diffType = false;
                }

                if (diffType && twoStep)
                    _targetPos += down;
                else if (twoStep)
                    _targetPos += back;

                _distance = Vector3.Distance(_targetPos, _startPos);

                InitIK(directionToPoint, !twoStep);
            }
        }

        #endregion

        #region IKS
        private void InitIKOpposite()
        {
            UpdateIKTarget(2, ik.ReturnOppositeIK(ik_L), targetPoint);
            UpdateIKTarget(3, ik.ReturnOppositeIK(ik_F), targetPoint);
        }

        private void InitIK(Vector3 directionToPoint, bool opposite)
        {
            Vector3 relativeDirection = transform.InverseTransformDirection(directionToPoint);

            if (Mathf.Abs(relativeDirection.y) > 0.5f)
            {
                float targetAnim = 0;

                if (targetState == ClimbStates.onPoint)
                {
                    ik_L = ik.ReturnOppositeIK(ik_L);
                }
                else
                {
                    if (Mathf.Abs(relativeDirection.x) > 0)
                    {
                        if (relativeDirection.x < 0)
                        {
                            ik_L = AvatarIKGoal.LeftHand;
                        }
                        else
                            ik_L = AvatarIKGoal.RightHand;
                    }

                    targetAnim = (ik_L == AvatarIKGoal.RightHand) ? 1 : 0;
                    if (relativeDirection.y < 0)
                    {
                        targetAnim = (ik_L == AvatarIKGoal.RightHand) ? 0 : 1;
                    }
                    anim.SetFloat("Movement", targetAnim);

                }
            }
            else
            {
                ik_L = (relativeDirection.x < 0) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;

                if (opposite)
                {
                    ik_L = ik.ReturnOppositeIK(ik_L);
                }
            }

            _ikT = 0;
            UpdateIKTarget(0, ik_L, targetPoint);

            ik_F = ik.ReturnOppositeLimb(ik_L);
            _fikT = 0;
            UpdateIKTarget(1, ik_F, targetPoint);
        }

        private void UpdateIKTarget(int posIndex, AvatarIKGoal _ikGoal, Point tp)
        {
            _ikStartPos[posIndex] = ik.ReturnCurrentPointPosition(_ikGoal);
            _ikTargetPos[posIndex] = tp.ReturnIK(_ikGoal).target.transform.position;
            ik.UpdatePoint(_ikGoal, tp);
        }

        AvatarIKGoal ik_L;
        AvatarIKGoal ik_F;
        float _ikT;
        float _fikT;
        Vector3[] _ikStartPos = new Vector3[4];
        Vector3[] _ikTargetPos = new Vector3[4];


        void PlaceIK(Point cp)
        {
            UpdateIKTarget(0, AvatarIKGoal.LeftHand, cp);
            UpdateIKTarget(1, AvatarIKGoal.LeftFoot, cp);

            UpdateIKTarget(2, AvatarIKGoal.RightHand, cp);
            UpdateIKTarget(3, AvatarIKGoal.RightFoot, cp);
        }

        #endregion

        #region Others
        private void BetweenPoints(Vector3 inD)
        {
            Neighbour n = ReturnNeighbour(inD, curPoint, curManager);

            if (n != null)
            {
                if (inD == n.direction)
                    targetPoint = prevPoint;
            }
            else
            {
                targetPoint = curPoint;
            }
            targetPosition = targetPoint.transform.position;
            climbState = ClimbStates.inTransit;
            targetState = ClimbStates.onPoint;
            prevPoint = curPoint;
            lockInput = true;
            anim.SetBool("Move", false);
        }

        private void OnPoint(Vector3 inD)
        {
            curAngleCheck = AngleCheck.skip;

            neighbour = null;
            Neighbour targetNeighbour = null;
            Manager targetManager = curManager;

            switch (curPoint.pointType)
            {
                case PointType.braced:
                    targetNeighbour = OnPointBraced(inD, targetManager);
                    break;
                case PointType.hanging:
                    targetNeighbour = OnPointHanging(inD, targetManager);
                    break;
            }
            neighbour = targetNeighbour;

            if (neighbour != null)
            {
                if (neighbour.target == null)
                    return;

                FixHipPositions(neighbour.target);

                targetPoint = neighbour.target;
                prevPoint = curPoint;
                climbState = ClimbStates.inTransit;
                UpdateConnectionTransitByType(neighbour, inD);

                lockInput = true;
            }

        }

        private Neighbour OnPointHanging(Vector3 inpD, Manager targetManager)
        {

            Neighbour n = new Neighbour();
            Point tp = null;
            curAngleCheck = AngleCheck.forward;
            skipDepthCheck = false;

            inpD.z = inpD.y;
            inpD.y = 0;

            tp = ReturnPoint(inpD, curPoint, targetManager);

            if (tp == null)
            {
                n = CheckForNearManager(inpD, targetManager);
                tp = n.target;

            }
            if (tp == null)
            {
                if (inpD.z > 0)
                {
                    targetManager = LookForManager_Forward();
                    if (targetManager != null)
                    {
                        n = CheckForManagerAhead(inpD, targetManager);
                        n.cType = ConnectionType.hanging_jump_forward;
                        tp = n.target;
                        return n;
                    }
                    else
                    {
                        n = jumpForwardNeighbour;
                        n.target = curPoint;
                        return n;
                    }
                }
                else
                if (inpD.z < 0)
                {
                    if (curPoint.doubleSided)
                    {
                        targetManager = LookForManager_Around();

                        if (targetManager != null)
                        {
                            curAngleCheck = AngleCheck.opposite;
                            tp = ReturnPoint(-inpD, curPoint, targetManager);
                            n.cType = ConnectionType.hanging_turn_around;
                            n.target = tp;
                            n.customConnection = true;
                            return n;
                        }
                    }
                }
                else
                if (inpD.x != 0)
                {
                    skipDepthCheck = true;
                    curAngleCheck = AngleCheck.skip;
                    targetManager = LookForManager_Corner_In(inpD);

                    if (targetManager != null)
                    {
                        n = CheckManagerCorner(inpD, targetManager);
                        tp = n.target;
                        n.cType = ConnectionType.corner_in;

                        if (n.target != null)
                        {
                            float dis = Vector2.Distance(curPoint.transform.parent.position,
                                n.target.transform.parent.position);
                            if (dis > directThreshold)
                            {
                                n.target = null;
                            }
                        }

                        return n;
                    }
                    else
                    {
                        targetManager = LookForManager_Corner_Out(inpD);
                        n = CheckManagerCorner(inpD, targetManager);
                        tp = n.target;
                        n.cType = ConnectionType.corner_out;
                        print(targetManager);
                        if (n.target != null)
                        {
                            float dis = Vector2.Distance(curPoint.transform.parent.position,
                                n.target.transform.parent.position);
                            if (dis > directThreshold)
                            {
                                n.target = null;
                            }
                        }

                        return n;
                    }
                }
            }

            if (tp == null)
                return null;
            n.target = tp;
            n = DoConnectionTypeChecks(inpD, n, true);
            PlaceIK(n.target);
            return n;
        }

        private Neighbour OnPointBraced(Vector3 inpD, Manager targetManager)
        {
            Neighbour n = new Neighbour();
            Point tp = null;

            curAngleCheck = AngleCheck.forward;
            skipDepthCheck = false;

            #region CheckBehind

            if (lookOnZ)
            {
                inpD.z = inpD.y;
                inpD.y = 0;
                inpD.x = 0;

                if (inpD.z < 0)
                {
                    targetManager = LookForManagerBehind();

                    if (targetManager == null)
                    {
                        n = jumpBackNeighbour;
                        jumpBackNeighbour.target = curPoint;
                        //skipAngleCheck = false;
                        return n;
                    }
                    else
                    {
                        curAngleCheck = AngleCheck.opposite;
                        n.cType = ConnectionType.jumpBack_onManager;
                        tp = ReturnPoint(inpD, curPoint, targetManager);
                        n.target = tp;
                        return n;
                    }
                }
            }

            #endregion

            tp = ReturnPoint(inpD, curPoint, targetManager);

            if (tp == null && !lookOnZ)
            {
                n = CheckForNearManager(inpD, targetManager);
                tp = n.target;

                if (tp == null && inpD.x != 0)
                {
                    skipDepthCheck = true;
                    curAngleCheck = AngleCheck.skip;
                    targetManager = LookForManager_Corner_In(inpD);

                    if (targetManager != null)
                    {
                        n = CheckManagerCorner(inpD, targetManager);
                        tp = n.target;
                        n.cType = ConnectionType.corner_in;

                        if (n.target != null)
                        {
                            float dis = Vector2.Distance(curPoint.transform.parent.position, n.target.transform.parent.position);
                            if (dis > directThreshold)
                            {
                                n.target = null;
                            }
                        }

                        return n;
                    }
                    else
                    {
                        targetManager = LookForManager_Corner_Out(inpD);
                        n = CheckManagerCorner(inpD, targetManager);
                        tp = n.target;
                        n.cType = ConnectionType.corner_out;
                        if (n.target != null)
                        {
                            float dis = Vector2.Distance(curPoint.transform.parent.position, n.target.transform.parent.position);
                            if (dis > directThreshold)
                            {
                                n.target = null;
                            }

                        }
                        return n;
                    }
                }

            }

            n.target = tp;
            n = DoConnectionTypeChecks(inpD, n);

            if (n.target == null)
                n = null;

            return n;
        }

        private void UpdateConnectionTransitByType(Neighbour n, Vector3 inputD)
        {
            //customRotationOnDirect = false;
            Vector3 desiredPos = Vector3.zero;
            curConnection = n.cType;

            Vector3 direction = targetPoint.transform.position - curPoint.transform.position;
            direction.Normalize();

            switch (n.cType)
            {
                case ConnectionType.inBetween:
                    float distance = Vector3.Distance(curPoint.transform.position, targetPoint.transform.position);
                    desiredPos = curPoint.transform.position + (direction * (distance / 2));

                    targetState = ClimbStates.betweenPoints;
                    TransitDir transitDir = ReturnTransitDirection(inputD, false);
                    PlayAnim(transitDir);
                    break;
                case ConnectionType.direct:
                    desiredPos = targetPoint.transform.position;

                    targetState = ClimbStates.onPoint;
                    TransitDir transitDir2 = ReturnTransitDirection(inputD, true);
                    PlayAnim(transitDir2, true);
                    //customRotationOnDirect = (transitDir2 == TransitDir.j_back);
                    break;
                case ConnectionType.dismount:
                    desiredPos = targetPoint.transform.position;
                    anim.SetInteger("JumpType", 20);

                    anim.SetBool("Move", true);
                    break;
                case ConnectionType.fall:
                    climbing = false;
                    initTransit = false;
                    ik.AddWeightInfluenceAll(0);
                    states.EnableController();
                    anim.SetBool("climbing", false);
                    break;
                case ConnectionType.jumpBack:
                    break;
                case ConnectionType.jumpBack_onManager:
                    desiredPos = targetPoint.transform.position;
                    targetState = ClimbStates.onPoint;
                    PlayAnim(TransitDir.j_back, true);
                    break;
                case ConnectionType.hanging_jump_forward:
                    desiredPos = targetPoint.transform.position;
                    targetState = ClimbStates.onPoint;
                    PlayAnim(TransitDir.h_j_forward, true);
                    break;
                case ConnectionType.hanging_turn_around:
                    Vector3 mid = ReturnCornerDir(curPoint, targetPoint, 0);
                    mid += curPoint.transform.right * 0.05f;
                    desiredPos = mid;
                    targetState = ClimbStates.betweenPoints;
                    break;

                case ConnectionType.corner_in:
                case ConnectionType.corner_out:

                    Vector3 corner = ReturnCornerDir(curPoint, targetPoint, 0.2f);
                    if (n.cType == ConnectionType.corner_in)
                        corner = ReturnCornerDir(curPoint, targetPoint, 0);

                    desiredPos = corner;
                    targetState = ClimbStates.betweenPoints;
                    TransitDir cDir = ReturnTransitDirection(inputD, false);
                    PlayAnim(cDir);
                    break;
                case ConnectionType.hanging_jump_air:
                    break;
            }
            switch (targetPoint.pointType)
            {
                case PointType.braced:
                    anim.SetFloat("Stance", 0);
                    break;
                case PointType.hanging:
                    anim.SetFloat("Stance", 1);
                    ik.InfluenceWeight(AvatarIKGoal.LeftFoot, 0);
                    ik.InfluenceWeight(AvatarIKGoal.RightFoot, 0);
                    break;
                default:
                    break;
            }
            targetPosition = desiredPos;
        }

        private Vector3 ReturnCornerDir(Point cp, Point tp, float multiplier = 1)
        {
            Vector3 cpPos = (-cp.transform.forward * multiplier) + cp.transform.position;
            Vector3 tpPos = (-tp.transform.forward * multiplier) + tp.transform.position;
            Vector3 direction = tpPos - cpPos;
            float distance = Vector3.Distance(cpPos, tpPos);
            Vector3 wp = cpPos + (direction * (distance / 2));
            return wp;
        }

        private Vector3 ConvertToInputDirection(float horizontal, float vertical)
        {
            int h = (horizontal != 0) ?
                (horizontal < 0) ? -1 : 1
                : 0;

            int v = (vertical != 0) ? (vertical < 0) ?
                -1 : 1 : 0;

            Vector3 retVal = Vector3.zero;
            retVal.x = h;
            retVal.y = v;

            return retVal;
        }

        private void LookForClimbSpot()
        {
            Vector3 origin = transform.position + Vector3.up;
            Vector3 direction = transform.forward;
            RaycastHit hit;

            float maxDistance = 3;

            Debug.DrawRay(origin, direction * maxDistance, Color.yellow);
            if (Physics.Raycast(origin, direction, out hit, maxDistance, lm))
            {
                if (hit.transform.GetComponentInChildren<Manager>())
                {
                    Manager tm = hit.transform.GetComponentInChildren<Manager>();

                    Point closestPoint = tm.ReturnClosest(transform.position);

                    float angle = Vector3.Angle(transform.forward,
                        closestPoint.transform.forward);

                    // Debug.Log(Vector3.Angle(closestPoint.transform.forward, Vector3.forward));
                    if (angle > 40)
                    {
                        closestPoint = null;
                        return;
                    }

                    float distanceToPoint = Vector3.Distance(transform.position,
                        closestPoint.transform.parent.position);
                    //print(distanceToPoint);
                    if (distanceToPoint < 5)
                    {
                        curManager = tm;
                        targetPoint = closestPoint;
                        FixHipPositions(targetPoint);

                        targetPosition = closestPoint.transform.position;
                        curPoint = closestPoint;
                        climbing = true;
                        lockInput = true;
                        targetState = ClimbStates.onPoint;
                        hasRootmotion = anim.applyRootMotion;
                        anim.applyRootMotion = false;
                        anim.SetBool("climbing", true);
                        hold = true;

                        states.DisableController();

                        //anim.CrossFade("To_Climb", 0.4f);
                        //GetComponent<Controller.StateManager>().DisableController();

                        waitToStartClimb = true;
                    }
                }
            }
        }
        #endregion

        #region Animations
        private TransitDir ReturnTransitDirection(Vector3 inpd, bool jump)
        {
            TransitDir retVal = default(TransitDir);

            float targetAngle = Mathf.Atan2(inpd.x, inpd.y) * Mathf.Rad2Deg;

            if (!jump)
            {
                if (Mathf.Abs(inpd.y) > 0)
                {
                    retVal = TransitDir.m_vert;
                }
                else
                {
                    retVal = TransitDir.m_hor;
                }

            }
            else
            {
                if (targetAngle < 22.5f && targetAngle > -22.5f)
                {
                    retVal = TransitDir.j_up;
                }
                else if (targetAngle < 180 + 22.5f && targetAngle > 180 - 22.5f)
                {
                    retVal = TransitDir.j_down;
                }
                else if (targetAngle < 90 + 22.5f && targetAngle > 90 - 22.5f)
                {
                    retVal = TransitDir.j_right;
                }
                else if (targetAngle < -90 + 22.5f && targetAngle > -90 - 22.5f)
                {
                    retVal = TransitDir.j_left;
                }

                if (Mathf.Abs(inpd.y) > Mathf.Abs(inpd.x))
                {
                    if (inpd.y < 0)
                        retVal = TransitDir.j_down;
                    else
                        retVal = TransitDir.j_up;
                }
            }
            if (lookOnZ)
            {
                if (inpd.z < 0)
                {
                    retVal = TransitDir.j_back;
                }
            }
            return retVal;
        }

        private void PlayAnim(TransitDir dir, bool jump = false)
        {
            int target = 0;

            switch (dir)
            {
                case TransitDir.m_hor:
                    target = 5;
                    break;
                case TransitDir.m_vert:
                    target = 6;
                    break;
                case TransitDir.j_up:
                    target = 0;
                    break;
                case TransitDir.j_down:
                    target = 1;
                    break;
                case TransitDir.j_left:
                    target = 3;
                    break;
                case TransitDir.j_right:
                    target = 2;
                    break;
                case TransitDir.j_back:
                    target = 33;
                    break;
                case TransitDir.h_j_forward:
                    target = 44;
                    break;
            }
            anim.SetInteger("JumpType", target);

            if (!jump)
                anim.SetBool("Move", true);
            else
                anim.SetBool("Jump", true);
        }
        enum TransitDir
        {
            m_hor,
            m_vert,
            j_up,
            j_down,
            j_left,
            j_right,
            j_back,
            j_forward,
            h_j_forward
        }

        #endregion

        #region Jump Back

        float rotationT;
        float targetAngle;
        bool init_JumpBack;

        void JumpBackwards()
        {
            if (!init_JumpBack)
            {
                anim.applyRootMotion = false;
                init_JumpBack = true;
                ik.AddWeightInfluenceAll(0);
                states.EnableController();
                states.rBody.isKinematic = true;
                anim.SetBool("Jump", true);
                anim.SetInteger("JumpType", 22);
                enableRootMovement = false;
                rotationT = 0;

                targetAngle = transform.localRotation.y + 180;
            }

            if (enableRootMovement)
            {
                rotationT += Time.deltaTime * 2;
            }
            if (rotationT > 1)
                rotationT = 1;
            transform.localRotation = Quaternion.Euler(
                transform.localRotation.x,
                Mathf.Lerp(transform.localRotation.y, targetAngle, rotationT),
                transform.localRotation.z);

        }
        void AddJumpBackwardsForce()
        {
            states.rBody.isKinematic = false;
            StartCoroutine("JumpBackForce");
            enableRootMovement = true;
        }
        IEnumerator JumpBackForce()
        {
            yield return new WaitForEndOfFrame();
            states.rBody.AddForce(-transform.forward * 5 + Vector3.up * 2, ForceMode.Impulse);
        }
        public void StopClimibng()
        {
            states.EnableController();
            climbing = false;
            anim.SetBool("climbing", false);
            anim.SetBool("Jump", false);
            init_JumpBack = false;
            init_JumpForward = false;
        }

        #endregion

        #region JumpForward

        bool init_JumpForward;
        void JumpForward()
        {
            if (!init_JumpForward)
            {
                anim.applyRootMotion = false;
                init_JumpForward = true;
                states.EnableController();
                states.rBody.isKinematic = true;
                anim.SetBool("Jump", true);
                anim.SetInteger("JumpType", 55);
                enableRootMovement = false;
            }
            if (enableRootMovement)
            {
                ik.AddWeightInfluenceAll(0);
            }
        }

        void AddJumpForwardForce()
        {
            states.rBody.isKinematic = false;
            StartCoroutine("JumpForwardForce");
            enableRootMovement = true;
        }
        IEnumerator JumpForwardForce()
        {
            yield return new WaitForEndOfFrame();
            states.rBody.AddForce((transform.forward * 5) + (Vector3.up * 2), ForceMode.Impulse);
        }
        #endregion

        #region useful
        public void UnHold()
        {
            hold = false;
        }
        public void EnableRootMovement()
        {
            enableRootMovement = true;
        }

        bool InAngle(Point cp, Point tp, AngleCheck check)
        {
            bool r = false;

            float angl = Vector3.Angle(cp.transform.forward, tp.transform.forward);

            switch (check)
            {
                case AngleCheck.skip:
                    r = true;
                    break;
                case AngleCheck.forward:
                    if (angl < 25)
                        r = true;
                    break;
                case AngleCheck.opposite:
                    if (angl > 155)
                        r = true;
                    break;
                default:
                    break;
            }

            return r;
        }

        #endregion
    }
    public enum AngleCheck
    {
        skip,
        forward,
        opposite
    }
}