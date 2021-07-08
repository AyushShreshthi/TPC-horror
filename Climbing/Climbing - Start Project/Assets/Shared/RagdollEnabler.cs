﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollEnabler : MonoBehaviour
{
    public Animator animator;
    private Rigidbody[] bodyParts;

    private void Start()
    {
        bodyParts = transform.GetComponentsInChildren<Rigidbody>();
        EnableRagdoll(false);
    }
    public void EnableRagdoll(bool value)
    {
        animator.enabled = !value;
        for(int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].isKinematic = !value;
        }
    }
}
