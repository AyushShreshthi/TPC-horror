using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Destructables
{
    public static EnemyHealth eh;
    
    [SerializeField] RagdollEnabler ragDoll;
    PathFiniding pf;
    EnemyAnimation ea;

    private void Awake()
    {
        eh = this;
        pf = gameObject.GetComponent<PathFiniding>();
        ea = gameObject.GetComponent<EnemyAnimation>();
    }
    public override void Die()
    {
        base.Die();
        ragDoll.EnableRagdoll(true);
    }
    public int health = 5;
    public bool getinCoverEnemy;
    public List<CoverPosition> allcovers = new List<CoverPosition>();

    float closestcoverdis=0f;
    GameObject closestcover;
    private void Update()
    {
        if (getinCoverEnemy)
        {
            foreach(var cover in allcovers)
            {
                float coverdis = Vector3.Distance(transform.position, cover.transform.position);
                if (coverdis < closestcoverdis)
                {
                    closestcover = cover.gameObject;
                    print(closestcover.gameObject);
                    pf.Agent.speed = 4;
                    ea.animator.SetBool("IsWalking", false);
                    break;
                }
                closestcoverdis = coverdis;
            }
            pf.SetTarget(closestcover.transform.position);
            
        }
        if (health <= 0)
        {
            Die();
        }
    }

}
