using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : EnemyScript
{
    [SerializeField] FireBall fireBall;
    bool isCloseToTarget = false;

    protected override void FollowTarget()
    {
        // out of attack range
        if (targetDistance > attackRange)
        {
            Vector3 dir = (transform.position - target.position).normalized;
            Vector3 destination = dir * (attackRange - 0.5f) + target.position; // -0.5 to allow enemy attack plyer once it reach range
            agent.SetDestination(destination);
        }

        // player is too close
        if (targetDistance <= (attackRange / 3))
        {
            Vector3 dir = (transform.position - target.position).normalized;
            Vector3 destination = dir * attackRange + target.position;
            agent.SetDestination(destination);
            isCloseToTarget = true;
        }
        else isCloseToTarget = false;

    }

    protected override void AttackMethod(Transform prey)
    {
        if (!isCloseToTarget)
        {
            FireBall myFireBall = Instantiate(fireBall.gameObject, transform.position, transform.rotation).GetComponent<FireBall>();
            myFireBall.HeadTotargetPos(prey.position);
        }
    }
}
