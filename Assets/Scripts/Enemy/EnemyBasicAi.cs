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
    SpriteRenderer enemySpriteRender;

    [SerializeField] Transform enemySprite;
    [SerializeField] float followDistance = 4.5f;


    // damage
    [SerializeField] float attackInterval = 3f;
    float damageTimer;
    [SerializeField] float myDamage = 5;
    [SerializeField] GameObject attackEffect;

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

    // flip
    bool isFacingRight;
    // sound
    SoundManager mySoundManagers;

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
        enemySpriteRender = enemySprite.GetComponent<SpriteRenderer>();
        mySoundManagers = SoundManager.Instance;

        action = EnemyAction.idle;

        dashTimer = 0;
        dashCDTimer = dashCD;
    }
    private void Update()
    {
        // target is missing set new target
        if (target == null){
            target = player.transform;
        }
        else if (target.GetComponent<Minion>() != null && !target.GetComponent<Minion>().isActive){
            target = player.transform;
        }

        targetDistance = Vector3.Distance(transform.position, target.position);
        // flip
        FlipMinion();

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
        if (other.transform == target && action == EnemyAction.following)
        {
            AttackMethod(other.gameObject);
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
    // ******************************************************Flip**********************************************************
    void FlipMinion()
    {
        //Enemies face right when moving right
        if (agent.velocity.x < 0)
        {
            enemySpriteRender.flipX = true;

        }
        //face left when facing left
        else if (agent.velocity.x > 0)
        {
            enemySpriteRender.flipX = false;
        }
        //or remain its direction when static
    }
    // ***************************************************Attack************************************************************
    void AttackMethod(GameObject prey)
    {
        if (damageTimer >= attackInterval)
        {
            // sound
            mySoundManagers.PlaySoundAt(mySoundManagers.transform.position, "Hurt", false, false, 1, 1f, 100, 100);
            // animation
            if (isFacingRight) Instantiate(attackEffect, transform.position + new Vector3(0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
            else Instantiate(attackEffect, transform.position + new Vector3(-0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);
            // deal damage
            if (prey.transform.GetComponent<Minion>() != null && !prey.IsDestroyed()) prey.transform.GetComponent<Minion>().TakeDamage(myDamage, transform);
            if (prey.transform.GetComponent<PlayerControl>() != null) prey.transform.GetComponent<PlayerControl>().PlayerTakeDamage(myDamage, transform);
            damageTimer = 0f;
        }
        else damageTimer += Time.deltaTime;

    }
}
