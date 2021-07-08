using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PathFiniding))]
[RequireComponent(typeof(EnemyPlayer))]
public class EnemyAnimation : MonoBehaviour
{
    [SerializeField]public Animator animator;
    public static EnemyAnimation ea;
    Vector3 lastPosition;
    PathFiniding pathFinding;
    EnemyPlayer enemyPlayer;
    EnemyPatrol ep;
    EnemyShoot es;
    private void Awake()
    {
        ea = this;
        pathFinding = gameObject.GetComponent<PathFiniding>();
        enemyPlayer = gameObject.GetComponent<EnemyPlayer>();
        es = gameObject.GetComponent<EnemyShoot>();
    }
    public bool playerselect;
    

    private void Update()
    {

        float velocity = ((transform.position - lastPosition).magnitude)/Time.deltaTime;
        lastPosition = transform.position;
        animator.SetBool("IsWalking", enemyPlayer.EnemyState.CurrentMode==Enemystate.EMode.UNAWARE);
        animator.SetFloat("Vertical",velocity / pathFinding.Agent.speed);
        animator.SetBool("IsSprinting", enemyPlayer.EnemyState.CurrentMode == Enemystate.EMode.AWARE);

        if (es.gunreloading)
        {
            animator.SetBool("Shoot", false);
            animator.SetBool("Reload", true);
            es.gunreloading = false;
        }
        if (playerselect)
            animator.SetBool("Shoot", true);
    }
}
