using UnityEngine;
using System.Collections;

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

            if (states.onGround)
            {
                rb.AddForce((v + h).normalized * Speed());
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

        float Speed()
        {
            return moveSpeed;
        }
    }
}
