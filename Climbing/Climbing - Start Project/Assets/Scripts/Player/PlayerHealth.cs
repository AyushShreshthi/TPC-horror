using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth :Destructables
{
    [SerializeField] RagdollEnabler ragDoll;
    public override void Die()
    {
        base.Die();
        ragDoll.EnableRagdoll(true);
    }
}
