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
        //ClimbIK ik;

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
            //ik = GetComponentInChildren<ClimbIK>();
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
            Vector3 targetPos = curCurve.GetPointAt(_t);
            transform.position = targetPos;

            //HandleWeightAll(_t, a_mountCurve);

            HandleRotation();
        }

        private void HandleRotation()
        {
            Vector3 targetDir = targetPoint.transform.position;

            if (targetDir == Vector3.zero)
                targetDir = transform.position;

            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
        }

        private void HandleWeightAll(float t, AnimationCurve a_mountCurve)
        {
            throw new NotImplementedException();
        }

        private void InitClimbing()
        {
            if (!initClimb)
            {
                initClimb = true;

                //if (ik != null)
                //{
                //    ik.UpdateAllPointOnOne(targetPoint);
                //    ik.UpdateAllTargetPositions(targetPoint);
                //    ik.ImmediatePlaceHelpers();
                //}

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
                    //ik.AddWeightInfluenceAll(0);
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

                //if (climbState == ClimbStates.onPoint)
                //{
                //    //ik.
                //}

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
                    //LerpIKLandingSide_Linear();
                    WrapUp();
                    break;
                case ConnectionType.direct:
                    UpdateDirectVariables(inputDirection);
                    Direct_RootMovement();
                   // DirectHandleIK();
                    WrapUp(true);
                    break;
                case ConnectionType.dismount:
                    HandleDismountVariables();
                    Dismount_RootMovement();
                    //HandleDismountIK();
                    DismountWrapUp();
                    break;
            }
        }
        #endregion

        #region InTransition Functions

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
            throw new NotImplementedException();
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

               // _ikT = 0;
                //_fikT = 0;
            }
        }
        #endregion

        #region Direct
        private void DirectHandleIK()
        {

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

            //HandleWeightAll(_t, a_jumpingCurve);

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

                //InitIK(inputDirection);
            }
        }

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
        private void LerpIKLandingSide_Linear()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region InBetween
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
                if (twoStep)
                    _targetPos += back;

                _distance = Vector3.Distance(_targetPos, _startPos);

                //InitIK(directionToPoint, !twoStep);
            }
        }

        private void InitIK(Vector3 directionToPoint, bool v=false)
        {
            throw new NotImplementedException();
        }
        #endregion

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
