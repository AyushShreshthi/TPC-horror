using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Riot_Shield : MonoBehaviour
{
    Animator anim;
    StateManagerShoot states;

    public Transform leftShoulder;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        states.GetComponentInParent<StateManagerShoot>();
    }

    private void OnEnable()
    {
        if (states == null)
        {
            states = GetComponentInParent<StateManagerShoot>();
        }

        states.dontRun = true;
    }

    private void OnDisable()
    {
        states.dontRun = false;
    }
    private void Update()
    {
        if (leftShoulder == null)
        {
            leftShoulder = transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftShoulder);

        }

        transform.position = leftShoulder.position;
        anim.SetBool("Aim", states.aiming);
    }
}
