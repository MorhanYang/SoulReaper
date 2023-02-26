using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;
    Vector3 sprintPos;
    GameObject player;

    [SerializeField] float attackRang = 1f;
    [SerializeField] float SprintSpeed = 1f;

    // find enemy & movement
    [SerializeField] float searchingRange = 2f;
    SpriteRenderer minionSprite;
    bool isFacingRight = true;

    //Melee Attack
    public float attackDamage = 2f;
    [SerializeField] float attackCD;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackCircle;
    float attackTimer = 0;

    // roaming
    List<Transform> roamingPosList;
    Transform InitialRoamingPoint;
    Vector3 roamingPos;
    bool randomHandler = true; // prevent random many times 

    enum MinionSate
    {
        Idle,
        Follow,
        Sprint,
        Attack,
        roam,
    }
    MinionSate minionState;

    private void Start()
    {
        agent= GetComponent<NavMeshAgent>();
        minionSprite = transform.Find("Mimion").GetComponent<SpriteRenderer>();
        player = PlayerManager.instance.player;

        SetUpRoamingPoints();

        minionState = MinionSate.Idle;
        GetComponent<SphereCollider>().radius = searchingRange;
        attackTimer = 0;
    }

    private void Update()
    {
        switch (minionState)
        {
            case MinionSate.Idle:
                break;
            case MinionSate.Follow:
                FollowEnemy();
                if (attackTimer >= attackCD) {
                    if (target != null && Vector3.Distance(transform.position, target.position) <= attackRang){
                        minionState = MinionSate.Attack;
                    }
                    attackTimer = 0;
                }
                if (target == null){
                    minionState = MinionSate.roam;
                }
                else attackTimer += Time.deltaTime;
                break;
            case MinionSate.Sprint:
                SprintFunction();
                break;
            case MinionSate.Attack:
                MeleeAttack();
                break;
            case MinionSate.roam:
                RoamMove();
                break;
            default:
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (minionState == MinionSate.roam)
        {
            if (other.GetComponent<Enemy>() != null && !other.GetComponent<Enemy>().isDead)
            {
                if (target == null)
                {
                    target = other.transform;
                }
                else if (Vector3.Distance(transform.position, other.transform.position) < Vector3.Distance(transform.position, target.position))
                {
                    target = other.transform;
                }
                minionState = MinionSate.Follow;
            }
        }
    }

    //************************************************************* State & get State ****************************************************************************
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackCircle);
    }
    public void ActiveMinion()
    {
        minionState = MinionSate.roam;
        GetRoamingStartPos();
    }
    public void InactiveMinion()
    {
        minionState = MinionSate.Idle;
        agent.SetDestination(transform.position);
    }

    public bool IsIdle()
    {
        if (minionState == MinionSate.Idle){
            return true;
        }
        else return false;

    }
    //***************************************************************** Sprint ***********************************************************************

    public void SpriteToPos(Vector3 aimPos){
        target = null; // ignore prevous target
        sprintPos = aimPos;
        minionState = MinionSate.Sprint;
    }
    public void SprintToEnemy(Transform aim)
    {
        target = aim;
        minionState = MinionSate.Sprint;
    }

    void SprintFunction()
    {
        // Don't hit enemy
        if (target == null)
        {
            Vector3 moveDir = sprintPos - transform.position;
            moveDir.Normalize();
            agent.Move(moveDir * SprintSpeed * Time.deltaTime);
            // reach Target:
            if (Vector3.Distance(transform.position, sprintPos) <= 0.1f)
            {
                minionState = MinionSate.roam;
            }
        }
        // Hit enemy
        else
        {
            Vector3 moveDir = target.position - transform.position;
            moveDir.Normalize();
            agent.Move(moveDir * SprintSpeed * Time.deltaTime);
            // reach Target:
            if (Vector3.Distance(transform.position, target.position) <= attackRang)
            {
                minionState = MinionSate.Follow;
            }
        }
    }

    // *******************************************************Automatically find enemy & move *****************************************************

    void FollowEnemy()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            // when roaming the strop distance become 0.1f;
            //if (agent.stoppingDistance != 0.4f) agent.stoppingDistance = 0.4f;

            // flip
            FlipMinion();
        }
    }

    void FlipMinion()
    {
        Vector3 targetDir = target.position - transform.position;
        if (targetDir.x < 0 && isFacingRight)
        {
            minionSprite.flipX = true;
            isFacingRight = !isFacingRight;

            attackPoint.localPosition = new Vector3(-0.4f, 0.1f, 0.1f);
        }
        if (targetDir.x > 0 && !isFacingRight)
        {
            minionSprite.flipX = false;
            isFacingRight = !isFacingRight;

            attackPoint.localPosition = new Vector3(0.4f, 0.1f, 0.1f);
        }
    }
    //****************************************************Attack*****************************************
    void MeleeAttack()
    {
        Collider[] hitEnemy = Physics.OverlapSphere(attackPoint.position, attackCircle, LayerMask.GetMask("Enemy", "PuzzleTrigger"));
        for (int i = 0; i < hitEnemy.Length; i++)
        {
            // enemy
            if (hitEnemy[i].GetComponent<Enemy>() != null){
                hitEnemy[i].GetComponent<Enemy>().TakeDamage(attackDamage, gameObject);
            }
            //puzzle trigger
            if (hitEnemy[i].GetComponent<PuzzleTrigger>() != null){
                hitEnemy[i].GetComponent<PuzzleTrigger>().TakeDamage(attackDamage, gameObject);
            }
        }
        minionState = MinionSate.Follow;

        // if kill the enemy
        if (target.GetComponent<Health>().presentHealth < 0)
        {
            GetRoamingStartPos();
            minionState = MinionSate.roam;
            // trigger ontrigger stay to see if there is enemy inside the cllider
            target = null;
        }
    }

    //**************************************************Radom Roaming***************************************
    void SetUpRoamingPoints(){
        // setup roamingPoints
        Transform roamingPointSet = player.transform.Find("RoamingPosSet");
        roamingPosList = new List<Transform>();
        foreach (Transform item in roamingPointSet)
        {
            roamingPosList.Add(item);
        }
    }
    void GetRoamingStartPos()
    {
        // random a roaming position
        int IDforRoamingPoint = Random.Range(0,roamingPosList.Count);
        InitialRoamingPoint = roamingPosList[IDforRoamingPoint];
        roamingPos = InitialRoamingPoint.position;
        randomHandler = true;
    }
    void RoamMove()
    {
        // move
        agent.SetDestination(roamingPos);

        // when minion reach the start point, after few second it start random moving
        if (roamingPos == InitialRoamingPoint.position && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (randomHandler){
                Invoke("GetRandomRoamingPos", Random.Range(3.5f, 5f));
                Invoke("GetRandomRoamingPos", Random.Range(2f, 4f));
                randomHandler = false;
            }
        }
        // when minion reach random moving pos;
        else if (agent.remainingDistance <= agent.stoppingDistance)
        {
            roamingPos= InitialRoamingPoint.position;
        }
    }
    void GetRandomRoamingPos()
    {
        if (minionState == MinionSate.roam) // because is a delay function, it would cause movement when minion is inactive
        {
            // get random direction
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            roamingPos = InitialRoamingPoint.position + randomDir * Random.Range(2.5f, 3.5f);
            randomHandler = true;
        }
    }

}
