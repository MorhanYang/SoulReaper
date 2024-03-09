using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    Vector3 myTargetPos;
    Transform myTargetSub;
    Transform myAttacker;
    [SerializeField] float speed = 3f;
    [SerializeField] float myDamage = 5f;
    [SerializeField] GameObject fireballMarkerTmp;
    GameObject myFireball;

    private void Update()
    {
        if (myTargetPos != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, myTargetPos, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, myTargetPos) < 0.1f)
            {
                DamageManager.instance.DealSingleDamage(myAttacker, transform.position, null, myDamage);
                SoundManager.Instance.PlaySoundAt(transform.position, "Hurt", false, false, 1, 1f, 100, 100);
                Destroy(myFireball);
                Destroy(gameObject);
            }
        }
        if (myTargetSub != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, myTargetSub.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, myTargetSub.position) < 0.1f)
            {
                DamageManager.instance.DealSingleDamage(myAttacker, transform.position, null, myDamage);
                SoundManager.Instance.PlaySoundAt(transform.position, "Hurt", false, false, 1, 1f, 100, 100);
                Destroy(gameObject);
            }
        }
    }

    public void HeadTotargetPos(Vector3 targetPos, Transform attacker,  float damage)
    {
        myTargetPos = targetPos;
        myDamage = damage;
        myAttacker = attacker;
        myFireball = Instantiate(fireballMarkerTmp, targetPos, transform.rotation);
    }

    public void HeadToTargetSub( Transform target , Transform attacker, float damage)
    {
        myTargetSub = target;
        myDamage = damage;
        myAttacker = attacker;
    }
}
