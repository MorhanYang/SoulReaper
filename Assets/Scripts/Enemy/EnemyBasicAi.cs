using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBasicAi : MonoBehaviour
{
    public Transform target;
    NavMeshAgent agent;
    GameObject player;
    AI_Dash dashScript;

    [SerializeField] Transform enemySprite;
    [SerializeField] float followDistance = 4.5f;


    // damage
    [SerializeField] float attackInterval = 3f;
    float damageTimer;
    [SerializeField] float myDamage = 5;

    // dash
    [SerializeField] bool canDash = false;
    [SerializeField] float distanceForDash;
    [SerializeField] float dashPrepareTime;
    [SerializeField] float dashCD;
    float dashTimer;
    float dashCDTimer;

    //player distance
    float targetDistance;

    // recieve damage slow down
    float slowDownSpeedOffset;

    enum EnemyAction {
        idle,
        following,
        dashing,
        shooting,
    }
    EnemyAction action;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = PlayerManager.instance.player;
        target = player.transform;
        dashScript = GetComponent<AI_Dash>();

        action = EnemyAction.idle;

        dashTimer = 0;
        dashCDTimer = dashCD;
    }
    private void Update()
    {
        targetDistance = Vector3.Distance(transform.position, target.position);

        // target is missing set new target
        if (target == null){
            target = player.transform;
        }
        else if (target.GetComponent<Minion>() != null && !target.GetComponent<Minion>().isActive){
            target = player.transform;
        }

        switch (action)
        {
            // idle
            case EnemyAction.idle:
                if (targetDistance < followDistance){
                    action = EnemyAction.following;
                }
                break;

            // following
            case EnemyAction.following:
                // stop following
                if (targetDistance > followDistance){
                    action = EnemyAction.idle;
                }
                // follow enemy
                FollowTarget();
                // can dash enemy
                if (canDash){
                    if (dashCDTimer >= dashCD && targetDistance <= distanceForDash){
                        dashScript.PrepareDash(target);
                        action = EnemyAction.dashing;
                    }
                }

               
                // Dash CD timer counting
                if (dashCDTimer <= dashCD) dashCDTimer += Time.deltaTime;
                break;

            // dasing
            case EnemyAction.dashing:
                // start timer
                dashTimer += Time.deltaTime;
                // start Dashing, when it ends reset property
                if (!dashScript.EnemyDashing(myDamage))
                {
                    action = EnemyAction.following;
                    target = player.transform;
                    dashTimer = 0;
                    dashCDTimer = 0;
                    damageTimer = 1f;// prevent deal 2 times
                }
                break;
        }

        // recover move speed
        if(slowDownSpeedOffset < 1f){
            slowDownSpeedOffset += 0.8f * Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // colide with target
        if (other.transform == target && action == EnemyAction.following){
            if (damageTimer >= attackInterval){
                if (other.transform.GetComponent<Minion>() != null && !other.IsDestroyed()) other.transform.GetComponent<Minion>().TakeDamage(myDamage,transform);
                if (other.transform.GetComponent<PlayerControl>() != null) other.transform.GetComponent<PlayerControl>().PlayerTakeDamage(myDamage,transform);
                damageTimer = 0f;
            }
            else damageTimer += Time.deltaTime;
        }
    }


    //**************************************************************************Method******************************************************
    void FollowTarget()
    {
        agent.SetDestination(target.position);
    }

  
    public void SlowDownEnemy(float offset){
        slowDownSpeedOffset = offset;
    }
}
