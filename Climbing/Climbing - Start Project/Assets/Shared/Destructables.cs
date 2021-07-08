using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class Destructables : MonoBehaviour
{
    [SerializeField] float hitPoints;
    public event System.Action OnDeath;
    public event System.Action OnDamageRecieved;

    float damageTaken;
    public float HitPointsRemaining
    {
        get
        {
           return  hitPoints - damageTaken;
        }
    }
    public bool IsAlive
    {
        get
        {
            return HitPointsRemaining > 0;
        }
    }
    public virtual void Die()
    {
        if (OnDeath != null)
            OnDeath();
    }

    public virtual void TakeDamage(float amount)
    {
        damageTaken += amount;
        if (OnDamageRecieved != null)
            OnDamageRecieved();

        if (HitPointsRemaining <= 0)
            Die();
        print(HitPointsRemaining);
    }
    public void Reset()
    {
            damageTaken = 0;
    }
    
}
