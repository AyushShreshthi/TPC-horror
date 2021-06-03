using UnityEngine;
using System.Collections;
using System;

namespace Controller
{
    public class HandleMovement : MonoBehaviour
    {

        public Rigidbody rb;
        StateManager states;

        InputHandler ih;

        public float moveSpeed = 4;
        public float rotateSpeed = 4;

        Vector3 storeDirection;

        public void Init()
        {
            states = GetComponent<StateManager>();
            rb = GetComponent<Rigidbody>();
            ih = GetComponent<InputHandler>();

            rb.angularDrag = 999;
            rb.drag = 4;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void Tick()
        {
            Vector3 v = ih.camHolder.forward * states.vertical;
            Vector3 h = ih.camHolder.right * states.horizontal;

            v.y = 0;
            h.y = 0;

            bool isGround = isGroundTowarsDirection((v + h).normalized);
            states.isGroundForward = isGround;

            if (states.onGround)
            {
                if (isGround)
                    rb.AddForce((v + h).normalized * Speed());
                else
                    rb.velocity = Vector3.zero;
            }

            if(Mathf.Abs(states.vertical) > 0 || Mathf.Abs(states.horizontal) > 0)
            {
                storeDirection = (v + h).normalized;

                storeDirection += transform.position;

                Vector3 targetDir = (storeDirection - transform.position).normalized;
                targetDir.y = 0;

                if (targetDir == Vector3.zero)
                    targetDir = transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        private bool isGroundTowarsDirection(Vector3 dir)
        {
            bool retVal = false;

            Vector3 origin = dir;
            float offset = 0.5f;
            origin *= offset;
            origin += Vector3.up / 2;
            origin += transform.position;

            bool forward = DoRayCast(origin, -Vector3.up);

            retVal = forward;

            if (Input.GetKey(KeyCode.LeftShift))
                retVal = true;

            return retVal;
        }

        private bool DoRayCast(Vector3 origin, Vector3 direction)
        {
            bool retVal = false;
            RaycastHit hit;

            if(Physics.Raycast(origin,direction,out hit, 1))
            {
                retVal = true;
            }
            return retVal;
        }

        float Speed()
        {
            return moveSpeed;
        }
    }
}
