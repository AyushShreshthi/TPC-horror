using UnityEngine;
using System.Collections;

public class OnExitCloseBool : StateMachineBehaviour
{

    public string boolName;
    public bool status;

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        animator.SetBool(boolName, status);

    }


}
	
