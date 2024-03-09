using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : EnemyScript
{
    [SerializeField] FireBall fireBall;
    bool isLeaving = false;
    bool canLeave = false;
    float runTimer = 0;
    float runInterval = 2f;

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
            // start counting time
            if (!canLeave && runTimer < runInterval)
            {
                runTimer += Time.deltaTime;
                if (runTimer >= runInterval){
                    canLeave = true;
                }
            }

            // can leave
            if (canLeave)
            {
                Vector3 dir = (transform.position - target.position).normalized;
                Vector3 destination = dir * attackRange + target.position;
                agent.speed = moveSpeed * 1.5f;
                agent.SetDestination(destination);
                isLeaving = true;

                runTimer -= Time.deltaTime;
                if (runTimer <= 0){
                    canLeave = false;
                    agent.speed = moveSpeed;
                    agent.SetDestination(transform.position);
                    ManualFlip();// change sprite face dir
                }
            }
            else isLeaving = false;
        }
        else isLeaving = false;

    }

    protected override void AttackMethod(Transform prey)
    {
        if (!isLeaving)
        {
            FireBall myFireBall = Instantiate(fireBall.gameObject, transform.position, transform.rotation).GetComponent<FireBall>();
            myFireBall.HeadTotargetPos( prey.position, transform, myDamage);
        }
    }
}
