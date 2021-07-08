using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PathFiniding : MonoBehaviour
{
    //[HideInInspector]
    public NavMeshAgent Agent;
    public static PathFiniding pf;
    [SerializeField] float distanceRemainingThreshold;
   public  bool m_destinationReached;
   public bool destinationReached
    {
        get
        {
           return m_destinationReached;
        }
        set
        {
            m_destinationReached = value;
            if (m_destinationReached)
            {
                if(OnDestinationReached!=null)
                    OnDestinationReached();

            }
        }
    }

    public event System.Action OnDestinationReached;
    private void Start()
    {
        pf = this;
        Agent = GetComponent<NavMeshAgent>();
    }
    public void SetTarget(Vector3 target)
    {
        destinationReached = false;
        Agent.SetDestination(target);
    }
    private void Update()
    {
        
        if (destinationReached || !Agent.hasPath)
            return;
        if (Agent.remainingDistance < distanceRemainingThreshold)
            destinationReached = true;


        
    }
}
