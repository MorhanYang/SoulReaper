using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] Transform enemySprite;
    [SerializeField] GameObject enemySoul;

    // AI
    protected Transform target;
    protected NavMeshAgent agent;
    GameObject player;
    public enum EnemyAction
    {
        idle,
        following,
        dashing,
        Recovering,
    }
    [HideInInspector] public EnemyAction action;
    //player distance
    protected float targetDistance;
    [SerializeField] float followDistance = 4.5f;
    public float moveSpeed = 0.6f;

    // flip
    protected SpriteRenderer enemySpriteRender;
    
    // health
    Health health;
    BasicEnemy basicEnemy;
    bool haveSoul;

    // attack
    DamageManager myDamageManager;
    public float attackRange = 0.5f;
    [SerializeField] float attackInterval = 3f;
    float damageTimer;
    public float myDamage = 5;
    float playerInRangeTimer = 0;
    [SerializeField] float UnsafeDistance = 1f;
    [SerializeField] GameObject alertnessIcon;


    // effect & Sound
    Shaker shaker;
    [SerializeField] GameObject attackEffect;
    SoundManager mySoundManager;

    // death trigger
    [SerializeField] SpikeGate blocker;
    [SerializeField] SoundTrigger previousSoundTrigger;

    float showHealthBarTimer = 0f;
    bool isDying = false;
    

    protected virtual void Start()
    {
        player = PlayerManager.instance.player;
        health = GetComponent<Health>();
        basicEnemy = GetComponent<BasicEnemy>();
        enemySpriteRender = enemySprite.GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        myDamageManager = DamageManager.instance;

        shaker = GetComponent<Shaker>();
        mySoundManager = SoundManager.Instance;

        // initialize
        action = EnemyAction.idle;
        health.HideHPUI();
        agent.speed = moveSpeed;

        if (enemySoul != null)
        {
            haveSoul = true;
        }
        else haveSoul = false;
    }

    private void Update()
    {
        // health bar
        if (showHealthBarTimer >= 0)
        {
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f)
            {
                health.HideHPUI();
            }
        }

        // Choose Target
        if (target == null)
        {
            StartCoroutine(ShowAlertnessIcon());
            target = player.transform;
        }
        else if (target.GetComponent<Minion>() != null && !target.GetComponent<Minion>().isActive)
        {
            StartCoroutine(ShowAlertnessIcon());
            target = player.transform;
        }

        // player distance
        targetDistance = Vector3.Distance(transform.position, target.position);


        // Behavior Tree
        BehaviorFunction();
    }

    // ******************************************************* Take Damage *****************************************************

    public void ChangeTargetToAttacker(Transform subject)
    {
        // change target if player is far away
        if (target == player.transform && playerInRangeTimer <= 0) //playerInRangeTimer is use to count time when player inside the unsafe area.
        {
            target = subject;
        }
    }
    public void BecomeMinion()
    {
        if (haveSoul)
        {
            GameObject minion = Instantiate(enemySoul, transform.position, transform.rotation);
            haveSoul = false;
            Destroy(gameObject);
        }
    }
    // **************************************************** Attack ************************************************************
    protected virtual void AttackMethod(Transform prey)
    {
        // sound
        mySoundManager.PlaySoundAt(mySoundManager.transform.position, "Hurt", false, false, 1, 1f, 100, 100);
        // animation
        if (basicEnemy.isFacingRight) Instantiate(attackEffect, transform.position + new Vector3(0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
        else Instantiate(attackEffect, transform.position + new Vector3(-0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);
        // deal damage
        myDamageManager.DealSingleDamage(transform, transform.position, prey.transform, myDamage);
    }
    //******************************************************* AI Related ********************************************************
    public void SetEnemyAction(EnemyAction newAction)
    {
        action = newAction;
    }

    void BehaviorFunction()
    {
        switch (action)
        {
            // idle
            case EnemyAction.idle:
                agent.SetDestination(transform.position);

                if (targetDistance < followDistance)
                {
                    StartCoroutine(ShowAlertnessIcon());
                    action = EnemyAction.following;
                }
                break;

            // following
            case EnemyAction.following:

                if (targetDistance > followDistance)
                {
                    // stop following
                    action = EnemyAction.idle;
                }

                // change Target to player
                if ( target != player.transform && Vector3.Distance(transform.position, player.transform.position) < UnsafeDistance)
                {
                    playerInRangeTimer += Time.deltaTime;
                    if (playerInRangeTimer > 3f)
                    {
                        StartCoroutine(ShowAlertnessIcon());
                        target = player.transform;
                    }
                }
                else if (playerInRangeTimer > 0)
                {
                    playerInRangeTimer -= Time.deltaTime;
                }

                // follow target
                FollowTarget();

                // Attack Trigger
                if (targetDistance <= attackRange){
                    if (damageTimer >= attackInterval)
                    {
                        AttackMethod(target);
                        damageTimer = 0;
                    }
                    else damageTimer += Time.deltaTime;
                }

                //// can dash enemy
                //if (canDash)
                //{
                //    if (dashCDTimer >= dashCD && targetDistance <= distanceForDash)
                //    {
                //        dashScript.PrepareDash(target);
                //        action = EnemyAction.dashing;
                //    }
                //}

                //// Dash CD timer counting
                //if (dashCDTimer <= dashCD) dashCDTimer += Time.deltaTime;
                break;

                //// dasing
                //case EnemyAction.dashing:
                //    // start timer
                //    dashTimer += Time.deltaTime;
                //    // start Dashing, when it ends reset property
                //    if (!dashScript.EnemyDashing(myDamage))
                //    {
                //        action = EnemyAction.following;
                //        target = player.transform;
                //        dashTimer = 0;
                //        dashCDTimer = 0;
                //        damageTimer = 1f;// prevent deal 2 times
                //    }
                //break;

                case EnemyAction.Recovering:
                // stop move and wait for reocver
                agent.SetDestination(transform.position);
                // show Abosorb Icon
                break;
                
        }
    }

    protected virtual void FollowTarget()
    {
        agent.SetDestination(target.position);
    }

    // *********************Flip
   
    public void ManualFlip()
    {
        enemySpriteRender.flipX = !enemySpriteRender.flipX;
        basicEnemy.isFacingRight = !basicEnemy.isFacingRight;
    }
    // Alertness Icon
    IEnumerator ShowAlertnessIcon()
    {
        alertnessIcon.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        alertnessIcon.SetActive(false);
    }
}
