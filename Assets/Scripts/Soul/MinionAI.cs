using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;
    Vector3 sprintPos;

    [SerializeField] float attackRang = 1f;
    [SerializeField] float SprintSpeed = 1f;

    // find enemy & movement
    [SerializeField] float searchingRange = 2f;
    SpriteRenderer minionSprite;
    bool isFacingRight = true;

    //Melee Attack
    [SerializeField] float attackDamage;
    [SerializeField] float attackCD;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackCircle;
    float attackTimer = 0;

    // roaming
    Vector3 roamingPos;

    enum MinionSate
    {
        Idle,
        Follow,
        Sprint,
        Attack,
        roam,
    }
    MinionSate minionState;

    private void OnEnable()
    {
        agent= GetComponent<NavMeshAgent>();
        minionSprite = transform.Find("Mimion").GetComponent<SpriteRenderer>();
        minionState = MinionSate.roam;

        GetComponent<SphereCollider>().radius = searchingRange;

        attackTimer = 0;

        GetRoamingPostion();

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
        if (minionState == MinionSate.Idle || minionState == MinionSate.roam)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackCircle);
    }

    //*****************************************************************Sprint***********************************************************************

    public void SpriteToEnemy(Vector3 aimPos, Transform enemy){
        // don't change target because of collision
        agent.isStopped = true;

        target = enemy;
        sprintPos = aimPos;

        Invoke("ChangeToSprint", 0.7f);
    }
    void ChangeToSprint()
    {
        minionState = MinionSate.Sprint;
        agent.isStopped = false;
    }

    void SprintFunction()
    {
        // play Sprint ainamtion

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
        Collider[] hitEnemy = Physics.OverlapSphere(attackPoint.position, attackCircle, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < hitEnemy.Length; i++)
        {
            hitEnemy[i].GetComponent<Enemy>().TakeDamage(attackDamage, gameObject);
        }
        minionState = MinionSate.Follow;

        // if kill the enemy
        if (target.GetComponent<Enemy>().isDead)
        {
            minionState = MinionSate.roam;
            // trigger ontrigger stay to see if there is enemy inside the cllider
            target = null;
        }
        
    }

    //**************************************************Radom Roaming***************************************
    private void GetRoamingPostion()
    {
        // get random direction
        Vector3 startPos = PlayerManager.instance.player.transform.position;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        roamingPos = startPos + randomDir * Random.Range(1f,2f);
    }

    void RoamMove()
    {
        agent.SetDestination(roamingPos);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Invoke("GetRoamingPostion", 1.5f);
        }
    }

}
