using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBasicAi : MonoBehaviour
{
    [SerializeField] Transform enemySprite;
    [SerializeField] float followSpeed = 1f;
    [SerializeField] float distanceForDash;
    [SerializeField] float dashPrepareTime;
    [SerializeField] float dashCD;
    [SerializeField] float dashSpeed = 2f;
    

    Transform target;
    NavMeshAgent agent;

    // dash
    float distance;
    float dashTimer;
    float dashCDTimer;
    Vector3 dashDir;
    float presentDashSpeed;

    enum EnemyAction {
        following,
        dashing,
        shooting,
        dead,
    }
    EnemyAction action;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.player.transform;

        dashTimer= 0;
        presentDashSpeed = dashSpeed;
        dashCDTimer = dashCD;
    }
    private void Update()
    {
        distance = Vector3.Distance(transform.position, target.position);

        switch (action)
        {
            case EnemyAction.following:
                FollowPlayer();
                if (dashCDTimer >= dashCD && distance <= distanceForDash)
                {
                    action = EnemyAction.dashing;
                    enemySprite.GetComponent<SpriteRenderer>().color = Color.red;
                    Debug.Log("Dash");
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
            case EnemyAction.dead:
                break;
        }

    }

    //***********************Method
    void FollowPlayer()
    {
        Vector3 moveDir = target.position - transform.position;
        moveDir.Normalize();
        agent.Move(moveDir * followSpeed * Time.deltaTime);
    }

    void DashPreparation()
    {
        enemySprite.GetComponent<SpriteRenderer>().color = Color.red;
        dashDir = target.position - transform.position;
        dashDir.Normalize();

        //set presentDashSpeed for Dashing;
        presentDashSpeed = dashSpeed;

        action = EnemyAction.dashing;
    }

    void Dashing()
    {
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
}
