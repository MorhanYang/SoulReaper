using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeMinionAI : MinionAI
{
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] FireBall fireBall;

    protected override void FollowFunction()
    {
        // out of attack range
        if (targetDistance > attackRange)
        {
            Vector3 dir = (transform.position - target.position).normalized;
            Vector3 destination = dir * (attackRange - 0.5f) + target.position; // -0.5 to allow enemy attack plyer once it reach range
            agent.SetDestination(destination);
        }
    }

    protected override void SprintFunctionForEnemy()
    {
        // out of attack range
        if (targetDistance > attackRange)
        {
            Vector3 dir = (transform.position - target.position).normalized;
            Vector3 destination = dir * (attackRange - 0.5f) + target.position; // -0.5 to allow enemy attack plyer once it reach range
            agent.SetDestination(destination);
        }
        else agent.SetDestination(transform.position);

        // end sprint and start follow
        if (targetDistance < agent.stoppingDistance)
        {
            agent.speed = NormalSpeed;
            minionState = MinionSate.Follow;

            //hide AssignIcon
            headIcon.sprite = null;
        }
    }

    protected override void Attack()
    {
        Debug.Log("Attack");
        FireBall myFireBall = Instantiate(fireBall.gameObject, transform.position, transform.rotation).GetComponent<FireBall>();
        myFireBall.HeadTotargetPos(target.position , transform, attackDamage);
    }

}
