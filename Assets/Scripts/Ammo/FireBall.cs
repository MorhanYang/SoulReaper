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

    public enum DamageType
    {
        Single,
        AOE,
    }
    public DamageType myDamageType;

    private void OnTriggerEnter(Collider other)
    {
        // according to attacker to decide damage target
        if (myAttacker.tag == "Player" ||
            myAttacker.tag == "Minion")
        {
            if (other.transform.tag == "Enemy")
            {
                DealDamage();
            }
        }

        if (myAttacker.tag == "Enemy")
        {
            if (other.transform.tag == "Player")
            {
                DealDamage();
            }
            if (other.transform.tag == "Minion" && other.GetComponent<Minion>().isActive)
            {
                DealDamage();
            }

        }
    }

    private void Update()
    {
        if (myTargetPos != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, myTargetPos, speed * Time.deltaTime);

            // didn't hite something
            if (Vector3.Distance(transform.position,myTargetPos) <= 0.1f)
            {
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

    void DealDamage()
    {
        switch (myDamageType)
        {
            case DamageType.Single:
                DamageManager.instance.DealSingleDamage(myAttacker, transform.position, null, myDamage);
                break;
            case DamageType.AOE:
                DamageManager.instance.DealAOEDamage(myAttacker, transform.position, 0.8f, myDamage);
                break;
        }
        SoundManager.Instance.PlaySoundAt(transform.position, "Hurt", false, false, 1, 1f, 100, 100);
        Destroy(gameObject);
    }

    public void HeadTotargetPos(Vector3 targetPos, Transform attacker,  float damage)
    {
        // keep flying for a few minutes
        myTargetPos = (targetPos - attacker.transform.position).normalized * 10f + attacker.transform.position;
        myDamage = damage;
        myAttacker = attacker;
    }

    public void HeadToTargetSub( Transform target , Transform attacker, float damage)
    {
        myTargetSub = target;
        myDamage = damage;
        myAttacker = attacker;
    }
}
