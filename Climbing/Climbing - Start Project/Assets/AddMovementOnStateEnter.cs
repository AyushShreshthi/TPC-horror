using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMovementOnStateEnter : StateMachineBehaviour
{
    public Vector3 direction;
    public float duration;
    public float speed;

    LastStand lastStand;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (lastStand == null)
        {
            lastStand = animator.transform.GetComponent<LastStand>();

        }

        if (lastStand == null)
            return;

        lastStand.AddMovement(direction, duration, speed);
    }

}
