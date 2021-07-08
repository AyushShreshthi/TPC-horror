using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFiniding))]
[RequireComponent(typeof(EnemyPlayer))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] WayPointController wayPointController;

    [SerializeField] float waitTimeMin, waitTimeMax;

    PathFiniding pathFinding;

    private EnemyPlayer m_EnemyPlayer;
    public EnemyPlayer Enemyplayer
    {
        get
        {
            if (m_EnemyPlayer == null)
                m_EnemyPlayer = GetComponent<EnemyPlayer>();
            return m_EnemyPlayer;
        }
    }

    private void Start()
    {
        wayPointController.SetNextWayPoint();

    }
    private void Awake()
    {
        pathFinding = gameObject.GetComponent<PathFiniding>();
         Enemyplayer.EnemyHealth.OnDeath += EnemyHealth_OnDeath;
        Enemyplayer.OnTargetSelected += Enemyplayer_OnTargetSelected;
    }

    private void Enemyplayer_OnTargetSelected(StateManagerShoot obj)
    {
       // pathFinding.Agent.Stop();
        pathFinding.Agent.isStopped = true;
        //targetSelected = true;
    }

    private void OnEnable()
    {
        pathFinding.OnDestinationReached += PathFinding_OnDestinationReached;
        wayPointController.OnWayPointChanged += WayPointController_OnWayPointChanged;


    }

    private void EnemyHealth_OnDeath()
    {
        pathFinding.Agent.isStopped = true;
    }

    private void WayPointController_OnWayPointChanged(WayPoint waypoint)
    {
        pathFinding.SetTarget(waypoint.transform.position);
        
    }
    public bool targetSelected = false;
    private void PathFinding_OnDestinationReached()
    {
        //assume we are patrolling
        if (!targetSelected)
        {
            GameManager.Instance.Timer.Add(wayPointController.SetNextWayPoint, UnityEngine.Random.Range(waitTimeMin, waitTimeMax));
        }
    }

}
