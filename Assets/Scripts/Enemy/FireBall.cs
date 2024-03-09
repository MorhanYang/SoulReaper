using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    Vector3 myTargetPos;
    Transform myTargetSub;
    [SerializeField] float speed = 3f;
    [SerializeField] float damage = 5f;

    private void Update()
    {
        if (myTargetPos != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, myTargetPos, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, myTargetPos) < 0.1f)
            {
                DamageManager.instance.DealSingleDamage(transform, transform.position, null, damage);
                SoundManager.Instance.PlaySoundAt(transform.position, "Hurt", false, false, 1, 1f, 100, 100);
                Destroy(gameObject);
            }
        }
        if (myTargetSub != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, myTargetSub.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, myTargetSub.position) < 0.1f)
            {
                DamageManager.instance.DealSingleDamage(transform, transform.position, null, damage);
                SoundManager.Instance.PlaySoundAt(transform.position, "Hurt", false, false, 1, 1f, 100, 100);
                Destroy(gameObject);
            }
        }
    }

    public void HeadTotargetPos( Vector3 targetPos )
    {
        myTargetPos = targetPos;
    }

    public void HeadToTargetSub( Transform target )
    {
        myTargetSub = target;
    }
}
