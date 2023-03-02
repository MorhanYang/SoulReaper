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
    [SerializeField] float SprintSpeed = 2f;
    [SerializeField] float NormalSpeed = 1.2f;

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
        Dead,
        Follow,
        Sprint,
        Attack,
        Roam,
        Wait,
        
    }
    MinionSate minionState;

    private void Start()
    {
        agent= GetComponent<NavMeshAgent>();
        minionSprite = transform.Find("Mimion").GetComponent<SpriteRenderer>();
        player = PlayerManager.instance.player;

        SetUpRoamingPoints();

        minionState = MinionSate.Dead;
        attackTimer = 0;

        agent.stoppingDistance = attackRang;
    }

    private void Update()
    {
        switch (minionState)
        {
            case MinionSate.Dead:
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
                    minionState = MinionSate.Roam;
                }
                else attackTimer += Time.deltaTime;
                break;
            case MinionSate.Sprint:
                SprintFunction();
                break;
            case MinionSate.Attack:
                MeleeAttack();
                break;
            case MinionSate.Roam:
                RoamMove();
                RoamCheckEnemy();
                break;
            default:
                break;
        }

        if (minionState != MinionSate.Dead) FlipMinion();
    }

    //************************************************************* State & get State ****************************************************************************
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackCircle);
    }
    public void ActivateMinion()
    {
        minionState = MinionSate.Roam;
        GetRoamingStartPos();
    }
    public void InactiveMinion()
    {
        minionState = MinionSate.Dead;
        agent.SetDestination(transform.position);
    }

    public bool IsDead(){
        if (minionState == MinionSate.Dead){
            return true;
        }
        else return false;

    }

    public void SetToWait(){
        minionState = MinionSate.Wait;
        agent.SetDestination(transform.position);
    }

    void StartRoam() // it is used for live minion
    {
        if(minionState != MinionSate.Dead)
        {
            minionState = MinionSate.Roam;
            GetRoamingStartPos();
        }
    }
    //***************************************************************** Sprint ***********************************************************************

    public void SpriteToPos(Vector3 aimPos){
        if (minionState != MinionSate.Dead)
        {
            target = null; // ignore prevous target
            // set destination
            if (aimPos == Vector3.zero) sprintPos = transform.position;
            else sprintPos = aimPos;
            // set property
            agent.speed = SprintSpeed;
            minionState = MinionSate.Sprint;
            agent.stoppingDistance = attackRang;
        }
    }
    public void SprintToEnemy(Transform aim){
        if (minionState != MinionSate.Dead)
        {
            // set destination
            target = aim;

            // set property
            agent.speed = SprintSpeed;
            minionState = MinionSate.Sprint;
            agent.stoppingDistance = attackRang;
        }
    }

    void SprintFunction()
    {
        // Don't hit enemy
        if (target == null)
        {
            agent.SetDestination(sprintPos);
            // reach Target:
            if (Vector3.Distance(transform.position, sprintPos) < agent.stoppingDistance)
            {
                agent.speed = NormalSpeed;
                minionState = MinionSate.Wait;
                Invoke("StartRoam", 0.8f);
                GetRoamingStartPos();
            }
        }
        // Hit enemy
        else
        {
            agent.SetDestination(target.position);
            // reach Target:
            if (Vector3.Distance(transform.position, target.position) < agent.stoppingDistance)
            {
                agent.speed = NormalSpeed;
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
            if (agent.stoppingDistance != attackRang) agent.stoppingDistance = attackRang;
        }
    }

    void FlipMinion()
    {
        if (agent.velocity.x < 0 && isFacingRight)
        {
            minionSprite.flipX = true;
            isFacingRight = !isFacingRight;

            attackPoint.localPosition = new Vector3(-0.4f, 0.1f, 0.1f);
        }
        if (agent.velocity.x > 0 && !isFacingRight)
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
            minionState = MinionSate.Roam;
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

        agent.stoppingDistance = 0.1f;
    }
    void RoamMove()
    {
        // move
        agent.SetDestination(roamingPos);

        // when minion reach the start point, after few second it start random moving
        if (roamingPos == InitialRoamingPoint.position && Vector3.Distance(transform.position,roamingPos) <= agent.stoppingDistance)
        {
            if (randomHandler){
                Invoke("GetRandomRoamingPos", Random.Range(3.5f, 5f));
                Invoke("GetRandomRoamingPos", Random.Range(2f, 4f));
                randomHandler = false;
            }
        }
        // when minion reach random moving pos;
        else if (Vector3.Distance(transform.position, InitialRoamingPoint.position) >= 2.5f 
            || Vector3.Distance(transform.position, roamingPos) <= agent.stoppingDistance)
        {
            roamingPos= InitialRoamingPoint.position;
        }
    }
    void GetRandomRoamingPos()
    {
        if (minionState == MinionSate.Roam) // because is a delay function, it would cause movement when minion is inactive
        {
            Debug.Log("random");
            // get random direction
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            roamingPos = InitialRoamingPoint.position + randomDir * Random.Range(1.5f, 2.5f);
            // find nearest point on the navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(roamingPos, out hit, 2.5f, NavMesh.AllAreas)){
                roamingPos = hit.position;
            }
            else roamingPos = InitialRoamingPoint.position;

            randomHandler = true;
        }
    }

    void RoamCheckEnemy()
    {
        Collider[] EnemyFound = Physics.OverlapSphere(transform.position, searchingRange, LayerMask.GetMask("Enemy"));

        if (EnemyFound.Length > 0){
            target = EnemyFound[0].transform;
            minionState = MinionSate.Follow;
        }
    }

}
