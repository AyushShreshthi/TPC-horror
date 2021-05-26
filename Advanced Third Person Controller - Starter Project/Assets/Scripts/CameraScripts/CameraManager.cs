using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPC
{
    public class CameraManager : MonoBehaviour
    {
        public bool holdCamera;
        public bool addDefaultAsNormal;
        public Transform target;

        #region Variables
        [SerializeField]
        public string activeStateID;
        [SerializeField]
        public float moveSpeed ;
        [SerializeField]
        float turnSpeed;
        [SerializeField]
        float turnSpeedController;
        [SerializeField]
        float turnSmoothing;
        [SerializeField]
        bool isController;
        public bool lockCursor;

        #endregion

        #region References
        [HideInInspector]
        public Transform pivot;
        [HideInInspector]
        public Transform camTrans;
        #endregion

        static public CameraManager singleton;

        Vector3 targetPosition;
        [HideInInspector]
        public Vector3 targetPositionOffset;

        #region Internal Variables
        float x;
        float y;
        float lookAngle;
        float tiltAngle;
        float offsetX;
        float offsetY;
        float smoothX=0;
        float smoothY=0;
        float smoothXVelocity=0;
        float smoothYVelocity=0;
        #endregion

        [SerializeField]
        List<CameraState> cameraState = new List<CameraState>();
        CameraState activeState;
        CameraState defaultState;
        LayerMask ignoreLayers;
        private void Awake()
        {
            singleton = this;
        }
        private void Start()
        {
            if(Camera.main.transform==null)
            {
                Debug.Log("You haven't assigned a camera with the tag MainCamera");
            }
            camTrans = Camera.main.transform.parent;
            pivot = camTrans.parent;

            //Create default state
            CameraState cs = new CameraState();
            cs.id = "default";
            cs.minAngle = 25;
            cs.maxAngle = 25;
            cs.cameraFOV = Camera.main.fieldOfView;
            cs.cameraZ = camTrans.localPosition.z;
            cs.pivotPosition = pivot.localPosition;
            defaultState = cs;

            if (addDefaultAsNormal)
            {
                cameraState.Add(defaultState);
                defaultState.id = "normal";
            }

            activeState =  defaultState;
            activeStateID = activeState.id;
            FixPositions();

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            ignoreLayers = ~(1 << 3 | 1 << 8);
        }
        private void FixedUpdate()
        {
            if (target)
            {
                targetPosition = target.localPosition +targetPositionOffset;
            }

            CameraFollow();

            if (!holdCamera)
                HandleRotation();

            FixPositions();
        }

        void CameraFollow()
        {
            Vector3 camPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
           
            transform.position = camPosition;
        }
        void HandleRotation()
        {
            HandleOffsets();

            x = Input.GetAxis("Mouse X") + offsetX;
            y = Input.GetAxis("Mouse Y") + offsetY;

            float targetTurnSpeed = turnSpeed;

            if (isController)
            {
                targetTurnSpeed = turnSpeedController;
            }

            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXVelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYVelocity, turnSmoothing);
            }
            else
            {
                smoothX = x;
                smoothY = y;
            }

            lookAngle += smoothX * targetTurnSpeed;

            //reset the look angle when it does a full circle
            if (lookAngle > 360)
                lookAngle = 0;
            if (lookAngle < -360)
                lookAngle = 0;

            transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

            tiltAngle -= smoothY * targetTurnSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, -activeState.minAngle, activeState.maxAngle);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
        }

        void HandleOffsets()
        {
            if (offsetX != 0)
            {
                offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
            }
            if (offsetY != 0)
            {
                offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
            }
        }
        private void FixPositions()
        {
            Vector3 targetPivotPosition = (activeState.useDefaultPosition) ? defaultState.pivotPosition : activeState.pivotPosition;
            pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPivotPosition, Time.deltaTime * moveSpeed);

            float targetZ = (activeState.userDefaultCameraZ) ? defaultState.cameraZ : activeState.cameraZ;
            float actualZ = targetZ;

            CameraCollisions(targetZ,ref actualZ);
            
            Vector3 targetP = camTrans.localPosition;
            targetP.z = Mathf.Lerp(targetP.z, actualZ, Time.deltaTime*7 );
            camTrans.localPosition = targetP;

            float targetFOV = (activeState.useDefaultFOV) ? defaultState.cameraFOV : activeState.cameraFOV;
            if (targetFOV < 1)
            {
                targetFOV = 2;
            }
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime* moveSpeed);
        }

        private void CameraCollisions(float targetZ, ref float actualZ)
        {
            float step = Mathf.Abs(targetZ);
            int stepCount = 2;
            float stepIncremental = step / stepCount;

            RaycastHit hit;
            Vector3 origin = pivot.position;
            Vector3 direction = -pivot.forward;
            Debug.DrawRay(origin, direction * step, Color.blue);

            if (Physics.Raycast(origin, direction, out hit, step, ignoreLayers))
            {
                float distance = Vector3.Distance(hit.point, origin);
                actualZ = -(distance / 2);
            }
            else
            {
                for (int s = 1; s < stepCount + 1; s++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 dir = Vector3.zero;
                        Vector3 secondOrigin = origin + (direction * s) * stepIncremental;

                        switch (i)
                        {
                            case 0:
                                dir = camTrans.right;
                                break;
                            case 1:
                                dir = -camTrans.right;
                                break;
                            case 2:
                                dir = camTrans.up;
                                break;
                            case 3:
                                dir = -camTrans.up;
                                break;
                            default:
                                break;
                        }
                        Debug.DrawRay(secondOrigin, dir * 1, Color.red);
                        if(Physics.Raycast(secondOrigin,dir,out hit, 1, ignoreLayers))
                        {
                            float distance = Vector3.Distance(secondOrigin, origin);
                            actualZ = -(distance / 2);
                            return;
                        }
                    }
                }
            }
        }

        CameraState GetState(string id)
        {
            CameraState r = null;
            for(int i = 0; i < cameraState.Count; i++)
            {
                if (cameraState[i].id == id)
                {
                    r = cameraState[i];
                    break;
                }
            }
            return r;
        }

        public void ChangeState(string id)
        {
            if (activeState.id != id)
            {
                CameraState targetState = GetState(id);
                if (targetState == null)
                {
                    Debug.Log("camera state ' " + id + " ' not found! Usind Previous");
                }
                activeState = targetState;
                activeStateID = activeState.id;

            }
        }


    }

    [System.Serializable]
    public class CameraState
    {
        [Header("Name of state")]
        public string id;

        [Header("Limits")]
        public float minAngle;
        public float maxAngle;

        [Header("Pivot Position")]
        public bool useDefaultPosition;
        public Vector3 pivotPosition;

        [Header("Camera Position")]
        public bool userDefaultCameraZ;
        public float cameraZ;

        [Header("Camera FOV")]
        public bool useDefaultFOV;
        public float cameraFOV;
    }
}
