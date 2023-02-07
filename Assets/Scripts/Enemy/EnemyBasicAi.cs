using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBasicAi : MonoBehaviour
{
    [SerializeField] Transform enemySprite;
    [SerializeField] float followDistance = 4.5f;
    [SerializeField] float followSpeed = 1f;
    [SerializeField] float distanceForDash;
    [SerializeField] float dashPrepareTime;
    [SerializeField] float dashCD;
    [SerializeField] float dashSpeed = 2f;
    [SerializeField] bool canDash = false;
    

    Transform target;
    NavMeshAgent agent;

    // dash
    float distance;
    float dashTimer;
    float dashCDTimer;
    Vector3 dashDir;
    float presentDashSpeed;

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
        target = PlayerManager.instance.player.transform;

        action = EnemyAction.idle;

        dashTimer = 0;
        presentDashSpeed = dashSpeed;
        dashCDTimer = dashCD;
    }
    private void Update()
    {
        distance = Vector3.Distance(transform.position, target.position);

        switch (action)
        {
            case EnemyAction.idle:
                if (distance < followDistance){
                    action = EnemyAction.following;
                }
                break;
            case EnemyAction.following:
                if (distance > followDistance){
                    action = EnemyAction.idle;
                }
                FollowPlayer();
                if (canDash){
                    if (dashCDTimer >= dashCD && distance <= distanceForDash)
                    {
                        action = EnemyAction.dashing;
                    }
                }

                // Dash CD timer counting
                if (dashCDTimer <= dashCD){
                    dashCDTimer += Time.deltaTime;
                }
                break;
            case EnemyAction.dashing:
                Dashing();
                break;
            case EnemyAction.shooting:
                break;
        }

        // recover move speed
        if(slowDownSpeedOffset < 1f){
            slowDownSpeedOffset += 0.8f * Time.deltaTime;
        }
    }

    //***********************Method
    void FollowPlayer()
    {
        Vector3 moveDir = target.position - transform.position;
        moveDir.Normalize();
        agent.Move(moveDir * followSpeed * slowDownSpeedOffset * Time.deltaTime);
    }

    void Dashing()
    {
        enemySprite.GetComponent<SpriteRenderer>().color = Color.red;

        dashTimer += Time.deltaTime;
        //set direction
        if (dashTimer <= dashPrepareTime/2)
        {
            dashDir = target.position - transform.position;
            dashDir.Normalize();
        }

        //start to dash
        if (dashTimer >= dashPrepareTime)
        {
            float dashResistance = 1.5f * dashSpeed;
            // dash movement
            agent.Move(dashDir * presentDashSpeed * Time.deltaTime);
            presentDashSpeed -= dashResistance * Time.deltaTime;
        }

        // End dashing
        if (presentDashSpeed <= 1f)
        {
            enemySprite.GetComponent<SpriteRenderer>().color = Color.white;
            action = EnemyAction.following;
            presentDashSpeed = dashSpeed;

            dashCDTimer = 0;
            dashTimer = 0;
        }
    }
    public void SlowDownEnemy(float offset){
        slowDownSpeedOffset = offset;
    }
}
