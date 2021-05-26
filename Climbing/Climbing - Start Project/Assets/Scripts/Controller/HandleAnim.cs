using UnityEngine;
using System.Collections;

namespace Controller
{
    public class HandleAnim : MonoBehaviour
    {

        StateManager states;
        public Animator anim;

        public void Init(StateManager st)
        {
            states = st;
            anim = GetComponent<Animator>();
            
            Animator[] childAnims = GetComponentsInChildren<Animator>();

            for (int i = 0; i < childAnims.Length; i++)
            {
                if(childAnims[i] != anim)
                {
                    anim.avatar = childAnims[i].avatar;
                    Destroy(childAnims[i]);
                    break;
                }
            }
        }

        public void Tick()
        {
            float animValue = Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical);
            animValue = Mathf.Clamp01(animValue);

            anim.SetFloat("Movement",animValue);
        }
    }
}
