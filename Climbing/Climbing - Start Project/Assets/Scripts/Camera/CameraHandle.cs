using UnityEngine;
using System.Collections;

namespace Controller
{
    public class CameraHandle : MonoBehaviour
    {
        public Transform target;
        Transform pivot;

        public float lerpSpeed = 5;

        float turnSpeed = 1.5f;
        float turnSmoothing = .1f;
        float tiltAngle;
        float tiltMax = 75f;
        float tiltMin = 45f;
        float smoothX;
        float smoothY;
        float smoothXvelocity = 0;
        float smoothYvelocity = 0;

        float lookAngle;

        void Start()
        {
            transform.position = target.position;
            pivot = transform.GetChild(0);
        }


        void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * lerpSpeed);

            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            if(turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnSmoothing);
            }
            else
            {
                smoothX = x;
                smoothY = y;
            }

            lookAngle += smoothX * turnSpeed;

            if (lookAngle > 360)
                lookAngle = 0;
            if (lookAngle < -360)
                lookAngle = 0;

            transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

            tiltAngle -= smoothY * turnSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMax);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
        }
    }
}
