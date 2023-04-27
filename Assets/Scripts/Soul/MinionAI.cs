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
    [SerializeField]SpriteRenderer minionSprite;
    bool isFacingRight = true;

    //Assign
    [SerializeField] GameObject assignIcon;
    float sprintTimer;

    //Melee Attack
    public float attackDamage = 0.4f;
    [SerializeField] float attackCD;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackCircle;
    float attackTimer = 0;
    [SerializeField] GameObject attackEffect;

    // roaming
    List<Transform> roamingPosList;
    Transform InitialRoamingPoint;
    Vector3 roamingPos;
    bool randomHandler = true; // prevent random many times 

    // dash
    AI_Dash dashScript;
    [SerializeField] bool canDash = false;
    [SerializeField] float dashDistance = 2f;
    [SerializeField] float IntervalDashing = 10f;
    float dashCD = 0;

    // Faint
    float FaintTimer = 0;
    [SerializeField] GameObject faintEffect;

    // sound
    SoundManager mySoundManagers;

    public enum MinionSate
    {
        Dead,
        Follow,
        Sprint,
        Dash,
        Roam,
        Bait,
        Wait,
        Faint,
    }
    public MinionSate minionState;

    private void Start()
    {
        agent= GetComponent<NavMeshAgent>();
        player = PlayerManager.instance.player;
        if (canDash) dashScript = GetComponent<AI_Dash>();
        mySoundManagers = SoundManager.Instance;

        SetUpRoamingPoints();

        minionState = MinionSate.Dead;
        attackTimer = 0;
        FaintTimer= 0;

        agent.stoppingDistance = attackRang;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetToFaint();
        }
        switch (minionState)
        {
            case MinionSate.Dead:
                break;
            case MinionSate.Follow:
                FollowEnemy();
                // Attack
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCD && target != null) {
                    // dash attack 
                    if (canDash && (Time.time - dashCD) > IntervalDashing && Vector3.Distance(transform.position, target.position) <= dashDistance){
                        //dash
                        Debug.Log("Minion_Dash");
                        dashScript.PrepareDash(target);
                        minionState = MinionSate.Dash;
                    }
                    // normal Attack
                    if (Vector3.Distance(transform.position, target.position) <= attackRang){
                        MeleeAttack();
                    }
                    attackTimer = 0;
                }
                break;

            case MinionSate.Sprint:
                SprintFunction();
                break;

            case MinionSate.Dash:
                // start Dashing, when it ends reset property
                if (!dashScript.MinionDashing(attackDamage * 1.5f))
                {
                    minionState = MinionSate.Follow;
                    dashCD = Time.time;
                }
                break;

            case MinionSate.Roam:
                RoamMove();
                RoamCheckEnemy();
                break;

            case MinionSate.Faint:
                if(FaintTimer <= 4)FaintTimer += Time.deltaTime;
                if (FaintTimer >= 4f)
                {
                    FaintTimer = 0;
                    if (minionState != MinionSate.Dead) { 
                        ActivateMinion();
                        faintEffect.SetActive(false);
                    }
                }
                break;
            case MinionSate.Bait:
                // if player run too far, follow the player
                if (Vector3.Distance(player.transform.position,transform.position)>5f){
                    minionState = MinionSate.Roam;
                }
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
        if(agent != null)agent.SetDestination(transform.position);

        faintEffect.SetActive(false);
        assignIcon.SetActive(false);

        if(canDash) dashScript.CancelDashing();

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

        assignIcon.SetActive(false);
    }
    public void SetToFaint(){
        if (minionState != MinionSate.Dead)
        {
            minionState = MinionSate.Faint;
            agent.SetDestination(transform.position);

            // display 
            Debug.Log("Faint");
            faintEffect.SetActive(true);
            assignIcon.SetActive(false);
            if(canDash) dashScript.CancelDashing();
        }
    }
    public void SetToBait()
    {
        minionState = MinionSate.Bait;
        assignIcon.SetActive(false);
        if (canDash) dashScript.CancelDashing();
    }

    public bool CanAssign()
    {
        if (minionState == MinionSate.Follow ||
            minionState == MinionSate.Roam)
        {
            return true;
        }
        else return false;
    }

    public void StartRoam() // it is used for live minion
    {
        agent.speed = NormalSpeed;
        if (minionState != MinionSate.Dead && minionState != MinionSate.Bait)
        {
            minionState = MinionSate.Roam;
            GetRoamingStartPos();
        }
    }
    //***************************************************************** Sprint ***********************************************************************

    public void SprinteToPos(Vector3 aimPos){
        if (minionState != MinionSate.Dead && minionState != MinionSate.Faint)
        {
            target = null; // ignore prevous target
            // set destination
            if (aimPos == Vector3.zero) sprintPos = transform.position;
            else sprintPos = aimPos;

            // set property
            agent.speed = SprintSpeed;
            minionState = MinionSate.Sprint;
            agent.stoppingDistance = attackRang;
            // set roaming pos. prevent minion from move back to previous roaming Pos
            roamingPos= aimPos;

            //show AssignIcon
            assignIcon.SetActive(true);

            sprintTimer = 0;
        }
    }
    public void SprintToEnemy(Transform aim){
        if (minionState != MinionSate.Dead && minionState != MinionSate.Faint)
        {
            // set destination
            target = aim;

            // set property
            agent.speed = SprintSpeed;
            minionState = MinionSate.Sprint;
            agent.stoppingDistance = attackRang;
            roamingPos = aim.position;

            //show AssignIcon
            assignIcon.SetActive(true);

            sprintTimer = 0;
        }
    }

    void SprintFunction()
    {
        if (sprintTimer > 5.5f){
            agent.speed = NormalSpeed;
            StartRoam();
            GetRoamingStartPos();
        }
        else sprintTimer += Time.deltaTime;

        // Don't hit enemy
        if (target == null)
        {
            agent.SetDestination(sprintPos);
            // reach Target:
            if (Vector3.Distance(transform.position, sprintPos) < agent.stoppingDistance)
            {
                agent.speed = NormalSpeed;
                minionState = MinionSate.Wait;
                if (minionState != MinionSate.Faint) Invoke("StartRoam", 1.2f); // prevent if from coming back from the platform trigger
                GetRoamingStartPos();

                //hide AssignIcon
                assignIcon.SetActive(false);
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

                //hide AssignIcon
                assignIcon.SetActive(false);
            }
        }
    }

    // *******************************************************Automatically find enemy & move *****************************************************

    void FollowEnemy()
    {
        // follow
        if (target != null)
        {
            agent.SetDestination(target.position);
            // when roaming the strop distance become 0.1f;
            if (agent.stoppingDistance != attackRang) agent.stoppingDistance = attackRang;
        }
        // roaming
        else if (target == null)
        {
            minionState = MinionSate.Roam;
        }
    }

    void FlipMinion()
    {
        if (agent.velocity.x < -0.3 && isFacingRight) // minion will wave their heads if it is 0
        {
            minionSprite.flipX = true;
            isFacingRight = !isFacingRight;

            attackPoint.localPosition = new Vector3(-0.4f, 0.1f, 0.1f);
        }
        if (agent.velocity.x > 0.3 && !isFacingRight)
        {
            minionSprite.flipX = false;
            isFacingRight = !isFacingRight;

            attackPoint.localPosition = new Vector3(0.4f, 0.1f, 0.1f);
        }
    }
    //****************************************************Attack*****************************************
    void MeleeAttack()
    {
        // animation
        if (isFacingRight) Instantiate(attackEffect, transform.position + new Vector3(0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
        else Instantiate(attackEffect, transform.position + new Vector3(-0.125f, 0.125f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);
        // play sound
        mySoundManagers.PlaySoundAt(mySoundManagers.transform.position, "Swing", false, false, 1, 0.5f, 100, 100);
        // aoe
        //Collider[] hitEnemy = Physics.OverlapSphere(attackPoint.position, attackCircle, LayerMask.GetMask("Enemy", "PuzzleTrigger"));
        //for (int i = 0; i < hitEnemy.Length; i++)
        //{
        //    // enemy
        //    if (hitEnemy[i].GetComponent<Enemy>() != null){
        //        hitEnemy[i].GetComponent<Enemy>().TakeDamage(attackDamage, transform);
        //    }
        //    //puzzle trigger
        //    if (hitEnemy[i].GetComponent<PuzzleTrigger>() != null){
        //        hitEnemy[i].GetComponent<PuzzleTrigger>().TakeDamage(attackDamage, gameObject);
        //    }
        //}
        if (Vector3.Distance(transform.position,target.position)<= attackRang + 0.2f)
        {
            //enemy
                if (target.GetComponent<Enemy>() != null)
            {
                target.GetComponent<Enemy>().TakeDamage(attackDamage, transform);
            }
            //puzzle trigger
            if (target.GetComponent<PuzzleTrigger>() != null)
            {
                target.GetComponent<PuzzleTrigger>().TakeDamage(attackDamage, gameObject);
            }
        }

        // if kill the enemy
        if (target.GetComponent<Health>() != null)
        {
            if (target.GetComponent<Health>().presentHealth < 0)
            {
                GetRoamingStartPos();
                minionState = MinionSate.Roam;
                // trigger ontrigger stay to see if there is enemy inside the cllider
                target = null;
            }
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
        assignIcon.SetActive(false);

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
