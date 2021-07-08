using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Destructables
{
    [SerializeField] float inSeconds;
    public override void Die()
    {
        base.Die();
        //print("we died");
        GameManager.Instance.Respawner.Despawn(gameObject,inSeconds);

    }
    private void OnEnable()
    {
        Reset();
    }
    public override void TakeDamage(float amount)
    {
        
        base.TakeDamage(amount);
        //print("Remaining" + HitPointsRemaining);
    }
}
    