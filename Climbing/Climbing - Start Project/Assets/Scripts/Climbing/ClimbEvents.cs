using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class ClimbEvents : MonoBehaviour
    {
        ClimbBehaviour cb;

        private void Start()
        {
            cb = transform.root.GetComponentInChildren<ClimbBehaviour>();
        }

        public void EnableRootMovement(float t)
        {
            StartCoroutine(Enable(t));
        }
        IEnumerator Enable(float t)
        {
            yield return new WaitForSeconds(t);
            cb.enableRootMovement = true;
        }
    }
}
