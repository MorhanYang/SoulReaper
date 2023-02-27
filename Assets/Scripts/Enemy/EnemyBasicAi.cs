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

    [SerializeField] Transform enemySprite;
    [SerializeField] float followDistance = 4.5f;


    // damage
    [SerializeField] float attackInterval = 3f;
    float damageTimer;
    [SerializeField] float myDamage = 5;

    // dash
    [SerializeField] bool canDash = false;
    [SerializeField] GameObject dashIndicator_Axis;
    [SerializeField] float distanceForDash;
    [SerializeField] float dashPrepareTime;
    [SerializeField] float dashCD;
    [SerializeField] float dashSpeed = 2f;
    float dashTimer;
    float dashCDTimer;
    Vector3 dashDir;
    float presentDashSpeed;
    List<Collider> DamagedMinion = new List<Collider>();

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

        action = EnemyAction.idle;

        dashTimer = 0;
        presentDashSpeed = dashSpeed;
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
            case EnemyAction.idle:
                if (targetDistance < followDistance){
                    action = EnemyAction.following;
                }
                break;
            case EnemyAction.following:
                if (targetDistance > followDistance){
                    action = EnemyAction.idle;
                }
                FollowTarget();
                if (canDash){
                    if (dashCDTimer >= dashCD && targetDistance <= distanceForDash) {
                        agent.SetDestination(transform.position);
                        PrepareDash();
                        action = EnemyAction.dashing;
                    }
                }
                // Dash CD timer counting
                if (dashCDTimer <= dashCD) dashCDTimer += Time.deltaTime;
                break;
            case EnemyAction.dashing:
                Dashing(myDamage);
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
                if (other.transform.GetComponent<Minion>() != null && !other.IsDestroyed()) other.transform.GetComponent<Minion>().TakeDamage(myDamage);
                if (other.transform.GetComponent<PlayerControl>() != null) other.transform.GetComponent<PlayerControl>().PlayerTakeDamage(myDamage);
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

    void PrepareDash()
    {
        dashDir = target.position - transform.position;
        dashDir.Normalize();

        //display
        enemySprite.GetComponent<SpriteRenderer>().color = Color.red;
        dashIndicator_Axis.SetActive(true);
        float angle = Mathf.Atan2(dashDir.z, dashDir.x) * Mathf.Rad2Deg;
        Quaternion DashRoatation = Quaternion.Euler(new Vector3(0, -angle, 0));
        dashIndicator_Axis.transform.rotation = DashRoatation;
    }

    void Dashing(float damage)
    {
        // start timer
        dashTimer += Time.deltaTime;
        // start Dashing
        if (dashTimer >= dashPrepareTime){
            // display
            dashIndicator_Axis.SetActive(false);

            // dash movement
            float dashResistance = 1.2f * dashSpeed;
            agent.Move(dashDir * presentDashSpeed * Time.deltaTime);
            presentDashSpeed -= dashResistance * Time.deltaTime;

            // deal damage
            Collider[] hitedObjecct = Physics.OverlapSphere((transform.position + dashDir * 0.2f), 0.14f, LayerMask.GetMask("Player", "MovingMinion"));
            // don't use Minion layermask because the moving minion is not in Minion 
            for (int i = 0; i < hitedObjecct.Length; i++){
                if (hitedObjecct[i].GetComponent<PlayerControl>() != null){
                    hitedObjecct[i].GetComponent<PlayerControl>().PlayerTakeDamage(damage);
                    // recuce damge after hit an object
                    damage = (int)(damage*0.6f);
                }
                else if (!DamagedMinion.Contains(hitedObjecct[i])){
                    hitedObjecct[i].GetComponent<Minion>().TakeDamage(damage);
                    DamagedMinion.Add(hitedObjecct[i]);
                    // recuce damge after hit an object
                    damage = (int)(damage * 0.6f);
                }
            }
        }

        // End dashing
        if (presentDashSpeed <= 1f){

            enemySprite.GetComponent<SpriteRenderer>().color = Color.white;
            action = EnemyAction.following;

            Debug.Log("End Dash");
            // reset the property
            presentDashSpeed = dashSpeed;
            dashTimer = 0;
            dashCDTimer = 0;
            damageTimer= 1f;// prevent deal 2 times
            DamagedMinion.Clear();
        }
    }
    public void SlowDownEnemy(float offset){
        slowDownSpeedOffset = offset;
    }
}
