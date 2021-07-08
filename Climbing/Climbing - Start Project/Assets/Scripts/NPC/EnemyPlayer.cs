using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathFiniding))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Enemystate))]
public class EnemyPlayer : MonoBehaviour
{
    public PathFiniding pathFinding;

    [SerializeField]public Scanner playerScanner;

    [SerializeField] SwatSoldier settings;

    StateManagerShoot priorityTarget;
    List<StateManagerShoot> myTargets;
    private Animator anim;
    AudioListener al;
    public event System.Action<StateManagerShoot> OnTargetSelected;

    public static EnemyPlayer ep;
    private EnemyHealth m_EnemyHealth;
    public EnemyHealth EnemyHealth
    {
        get
        {
            if (m_EnemyHealth == null)
                m_EnemyHealth = GetComponent<EnemyHealth>();
            return m_EnemyHealth;
        }
    }

    private Enemystate m_EnemyState;
    public Enemystate EnemyState
    {
        get
        {
            if (m_EnemyState == null)
                m_EnemyState = GetComponent<Enemystate>();
            return m_EnemyState;
        }
    }
    EnemyAnimation enemyanim;
    private void Start()
    {
        ep = this;
        al = gameObject.GetComponent<AudioListener>();
        pathFinding = gameObject.GetComponent<PathFiniding>();
        enemyanim = gameObject.GetComponent<EnemyAnimation>();
        pathFinding.Agent.speed = settings.WalkSpeed;
        anim =gameObject.GetComponentInChildren<Animator>();
        playerScanner.OnScanReady += Scanner_OnScanReady;
        Scanner_OnScanReady();

        EnemyHealth.OnDeath += EnemyHealth_OnDeath;
        EnemyState.OnModeChanged += EnemyState_OnModeChanged;
        OnTargetSelected += EnemyPlayer_OnTargetSelected;
    }
    private void EnemyPlayer_OnTargetSelected(StateManagerShoot obj)
    {
        anim.SetLayerWeight(1, 0);
        anim.SetLayerWeight(2, 0);
        enemyanim.playerselect = true;
        pathFinding.SetTarget(obj.transform.position);
        pathFinding.Agent.stoppingDistance = 2f;
    }
    private void EnemyState_OnModeChanged(Enemystate.EMode state)
    {
        pathFinding.Agent.speed = settings.WalkSpeed;
        
        if(state==Enemystate.EMode.AWARE)
        {
            pathFinding.Agent.speed = settings.RunSpeed;
        }
    }

    private void EnemyHealth_OnDeath()
    {


    }

    public void Scanner_OnScanReady()
    {
        if (priorityTarget != null)
            return;

        myTargets = playerScanner.ScanForTarget<StateManagerShoot>();

        if (myTargets.Count == 1)
            priorityTarget = myTargets[0];
        else
            SelectClosestTarget();

        if (priorityTarget != null)
        {
            
            if (OnTargetSelected != null)
            {
               OnTargetSelected(priorityTarget);
                
            }
        }
    }

    
    private void SelectClosestTarget()
    {
        float closestTarget = playerScanner.ScanRange;
        foreach(var possibleTarget in myTargets )
        {
            if (Vector3.Distance(transform.position, possibleTarget.transform.position) < closestTarget)
                priorityTarget = possibleTarget;
              
        }
    }
    GameObject playobj;
    private void Update()
    {
        playobj = AudioListener.FindObjectOfType<StateManagerShoot>().gameObject;
        if (Vector3.Distance(transform.position, playobj.transform.position) <= 34)
        { 
            if(playobj.GetComponent<CharacterAudioManager>().runFoley.volume>=0.5 && enemyanim.playerselect!=true)
            {
                pathFinding.SetTarget(playobj.transform.position);
            }
        }
        if (priorityTarget == null)
        {
            return;
        }
        transform.LookAt(priorityTarget.transform.position);
    }
}
