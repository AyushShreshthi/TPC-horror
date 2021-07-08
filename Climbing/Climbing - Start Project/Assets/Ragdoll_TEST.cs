using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll_TEST : Destructables
{
    public Animator animator;
    private Rigidbody[] bodyParts;

    private void Start()
    {
        bodyParts = transform.GetComponentsInChildren<Rigidbody>();
        EnableRagdoll(false);
    }
    public override void Die()
    {
        base.Die();
        EnableRagdoll(true);
        animator.enabled = false;
    }
    void EnableRagdoll(bool value)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].isKinematic = !value ;
        }
    }
}
