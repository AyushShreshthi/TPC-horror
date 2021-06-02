using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Climbing
{
    public class ClimbBehaviour : MonoBehaviour
    {
        #region Variables
        public bool climbing;
        bool initClimb;
        bool waitToStartClimb;
        bool waitForWrapUp;

        Animator anim;
        ClimbIK ik;

        Manager curManager;
        Point targetPoint;
        Point curPoint;
        Point prevPoint;
        Neighbour neighbour;
        ConnectionType curConnection;

        ClimbStates climbState;
        ClimbStates targetState;

        public enum ClimbStates
        {
            onPoint,
            betweenPoints,
            inTransit
        }

        #region Curves
        CurvesHolder curveHolder;
        BezierCurve directCurveHorizontal;
        BezierCurve directCurveVertical;
        BezierCurve dismountCurve;
        BezierCurve mountCurve;
        BezierCurve curCurve;

        #endregion

        Vector3 _startPos;
        Vector3 _targetPos;
        float _distance;
        float _t;
        bool initTransit;
        bool rootReached;
        bool ikLandSideReached;
        bool ikFollowSideReached;

        bool lockInput;
        Vector3 inputDirection;
        Vector3 targetPosition;

        public Vector3 rootOffset = new Vector3(0, -0.86f, 0);
        public float speed_linear = 1.3f;
        public float speed_direct = 2;

        public AnimationCurve a_jumpingCurve;
        public AnimationCurve a_mountCurve;
        public bool enableRootMovement;
        float _rmMax = 0.25f;
        float _rmT;

        #endregion

        void SetCurveReferences()
        {
            GameObject chPrefab = Resources.Load("CurvesHolder") as GameObject;
            GameObject chGO = Instantiate(chPrefab) as GameObject;

            curveHolder = chGO.GetComponent<CurvesHolder>();

            directCurveHorizontal = curveHolder.ReturnCurve(CurveType.horizontal);
            directCurveVertical = curveHolder.ReturnCurve(CurveType.vertical);
            dismountCurve = curveHolder.ReturnCurve(CurveType.dismount);
            mountCurve = curveHolder.ReturnCurve(CurveType.mount);

        }

        private void Start()
        {
            anim = GetComponentInChildren<Animator>();
            ik = GetComponentInChildren<ClimbIK>();
            SetCurveReferences();
        }

        private void FixedUpdate()
        {
            if (climbing)
            {
                if (!waitToStartClimb)
                {
                    HandleClimbing();
                    InititateFallOff();
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
            }
        }

        #region Mains
        private void HandleMount()
        {
            if (!initTransit)
            {
                initTransit = true;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                _t = 0;
                _startPos = transform.position;
                _targetPos = targetPosition + rootOffset;

                curCurve = mountCurve;
                curCurve.transform.rotation = targetPoint.transform.rotation;
                BezierPoint[] points = curCurve.GetAnchorPoints();
                points[0].transform.position = _startPos;
                points[points.Length - 1].transform.position = _targetPos;
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
            
            }

            HandleWeightAll(_t, a_mountCurve);

            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;

            HandleRotation();
        }

        private void HandleRotation()
        {
            Vector3 targetDir = targetPoint.transform.position;

            if (targetDir == Vector3.zero)
            {
                print("transform");
                targetDir = transform.position;
            }

            Quaternion targetRot = Quaternion.LookRotation(Vector3.zero,targetDir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);

        }

        private void HandleWeightAll(float t, AnimationCurve aCurve)
        {
            float inf = aCurve.Evaluate(t);
            ik.AddWeightInfluenceAll(1 - inf);

            if (curPoint.pointType == PointType.hanging)
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
                    GetComponent<Controller.StateManager>().EnableController();
                    anim.SetBool("onAir", true);
                }
            }
        }

        private void HandleClimbing()
        {
            if (!lockInput)
            {
                inputDirection = Vector3.zero;

                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");

                inputDirection = ConvertToInputDirection(h, v);

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
                    Direct_RootMovement();
                    DirectHandleIK();
                    WrapUp(true);
                    break;
                case ConnectionType.dismount:
                    HandleDismountVariables();
                    Dismount_RootMovement();
                    HandleDismountIK();
                    DismountWrapUp();
                    break;
            }
        }
        #endregion

        #region Dismount
        private void DismountWrapUp()
        {
            if (rootReached)
            {
                climbing = false;
                initTransit = false;
                GetComponent<Controller.StateManager>().EnableController();
            }
        }

        private void HandleDismountIK()
        {
            if (enableRootMovement)
                _ikT += Time.deltaTime * 3;

            _fikT += Time.deltaTime * 2;

            HandleIKWeight_Dismount(_ikT, _fikT, 1, 0);
        }

        private void HandleIKWeight_Dismount(float ht, float gt, int from, int to)
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

            float t2 = t1 * 1;

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
                _rmT = 0;
                _startPos = transform.position;
                _targetPos = targetPosition;

                curCurve = dismountCurve;
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
        private void Direct_RootMovement()
        {
            if (enableRootMovement)
            {
                _t += Time.deltaTime * speed_direct;
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

            HandleWeightAll(_t, a_jumpingCurve);

            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;

            HandleRotation();
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
                _t = 0;
                _rmT = 0;
                _targetPos = targetPosition + rootOffset;
                _startPos = transform.position;

                bool vertical = (Mathf.Abs(inpD.y) > 0.1f);
                curCurve = (vertical) ? directCurveVertical : directCurveHorizontal;
                curCurve.transform.rotation = curPoint.transform.rotation;

                if (!vertical)
                {
                    if (!(inpD.x > 0))
                    {
                        Vector3 eulers = curCurve.transform.eulerAngles;
                        eulers.y = -180;
                        curCurve.transform.eulerAngles = eulers;
                    }
                }
                else
                {
                    if (!(inpD.y > 0))
                    {
                        Vector3 eulers = curCurve.transform.eulerAngles;
                        eulers.x = 180;
                        eulers.y = 180;
                        curCurve.transform.eulerAngles = eulers;
                    }
                }

                BezierPoint[] points = curCurve.GetAnchorPoints();
                points[0].transform.position = _startPos;
                points[points.Length - 1].transform.position = _targetPos;

                
               InitIK_Direct(inputDirection);
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

        private void WrapUp(bool direct=false)
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

            _ikT += lerpSpeed * 3;

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

                _t = 0;
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

        #endregion

        #region Others
        private void BetweenPoints(Vector3 inD)
        {
            Neighbour n = targetPoint.ReturnNeighbour(prevPoint);

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
            neighbour = null;
            neighbour = curManager.ReturnNeighour(inD, curPoint);

            if (neighbour != null)
            {
                targetPoint = neighbour.target;
                prevPoint = curPoint;
                climbState = ClimbStates.inTransit;
                UpdateConnectionTransitByType(neighbour, inD);

                lockInput = true;
            }
        }

        private void UpdateConnectionTransitByType(Neighbour n, Vector3 inputD)
        {
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
                    TransitDir transitDir2 = ReturnTransitDirection(direction, true);
                    PlayAnim(transitDir2,true);
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
                    GetComponent<Controller.StateManager>().EnableController();
                    anim.SetBool("OnAir", true);
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

        private Vector3 ConvertToInputDirection(float horizontal, float vertical)
        {
            int h = (horizontal != 0) ? (horizontal < 0) ?
                -1 : 1 : 0;

            int v = (vertical != 0) ? (vertical < 0) ?
                -1 : 1 : 0;

            int z = v + h;

            z = (z != 0) ?
                (z < 0) ? -1 : 1:
                 0;

            Vector3 retVal = Vector3.zero;
            retVal.x = h;
            retVal.y = v;

            return retVal;
        }

        private void LookForClimbSpot()
        {
            Transform camTrans = Camera.main.transform;
            Ray ray = new Ray(camTrans.position, camTrans.forward);

            RaycastHit hit;
            LayerMask lm = (1 << gameObject.layer) | (1 << 3);
            lm = ~lm;

            float maxDistance = 20;

            
            if (Physics.Raycast(ray,out hit, maxDistance, lm))
            {
                if (hit.transform.GetComponentInParent<Manager>())
                {
                    Manager tm = hit.transform.GetComponentInParent<Manager>();

                    Point closestPoint = tm.ReturnClosest(transform.position);

                    float distanceToPoint = Vector3.Distance(transform.position, closestPoint.transform.parent.position);

                    if (distanceToPoint < 5)
                    {
                        curManager = tm;
                        targetPoint = closestPoint;
                        targetPosition = closestPoint.transform.position;
                        curPoint = closestPoint;


                        climbing = true;
                        lockInput = true;
                        targetState = ClimbStates.onPoint;

                        anim.CrossFade("To_Climb", 0.4f);
                        GetComponent<Controller.StateManager>().DisableController();

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
                if(targetAngle<22.5f && targetAngle > -22.5f)
                {
                    retVal = TransitDir.j_up;
                }
                else if(targetAngle < 180 + 22.5f && targetAngle > 180 - 22.5f)
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
            return retVal;
        }

        private void PlayAnim(TransitDir dir, bool jump=false)
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
            j_right
        }

        #endregion
    }
}
