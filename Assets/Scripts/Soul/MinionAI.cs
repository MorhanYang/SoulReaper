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


    enum MinionSate
    {
        Idle,
        Follow,
        Sprint,
        Attack,
    }
    MinionSate minionState;

    private void OnEnable()
    {
        agent= GetComponent<NavMeshAgent>();
        minionSprite = transform.Find("Mimion").GetComponent<SpriteRenderer>();
        minionState = MinionSate.Idle;

        GetComponent<SphereCollider>().radius = searchingRange;

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
            default:
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (minionState == MinionSate.Idle)
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
        target = null;
        target = enemy;
        sprintPos = aimPos;
        minionState = MinionSate.Sprint;
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
                minionState = MinionSate.Idle;
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
        foreach (Collider item in hitEnemy)
        {
            item.GetComponent<Enemy>().TakeDamage(attackDamage);
        }

        if (target.GetComponent<Enemy>().isDead)
        {
            minionState = MinionSate.Idle;
            // trigger ontrigger stay to see if there is enemy inside the cllider
            target = null;
        }
        else minionState= MinionSate.Follow;
    }

}
