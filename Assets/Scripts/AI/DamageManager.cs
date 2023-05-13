using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager instance;
    [SerializeField] LayerMask attackMask;

    private void Awake(){
        instance= this;
    }

    //******************************************************************** Function *******************************************************************
    public void DealSingleDamage(Transform attacker, Vector3 attackerPos, Transform reciever, float damage) // Reciever is for specific attack target. for normal attack recieve is null.
    {
        // *****************find reciever
        if (reciever == null){

            Collider[] hitedEnemy = Physics.OverlapSphere(attackerPos, 0.3f, attackMask);

            // no attacker
            if (attacker == null)
            {
                return;
            }
            // attacker is player or minion
            else
            {
                if (attacker.GetComponent<PlayerHealthBar>() != null || attacker.GetComponent<Minion>() != null)
                {
                    for (int i = 0; i < hitedEnemy.Length; i++)
                    {
                        if (hitedEnemy[i].GetComponent<Enemy>() != null)
                        {
                            reciever = hitedEnemy[i].transform;
                            continue;
                        }
                    }
                }
                // Attacker is enemy
                else if (attacker.GetComponent<Enemy>() != null)
                {
                    for (int i = 0; i < hitedEnemy.Length; i++)
                    {
                        if (hitedEnemy[i].GetComponent<PlayerHealthBar>() != null || hitedEnemy[i].GetComponent<Minion>() != null)
                        {
                            reciever = hitedEnemy[i].transform;
                            continue;
                        }
                    }
                }
            }
        }   


        // **************identify damage reciever
        if (reciever != null)
        {
            //1. Minion
            if (reciever.GetComponent<Minion>() != null && reciever.gameObject.IsDestroyed())
            {
                Minion myMinion = reciever.GetComponent<Minion>();
                if (myMinion.isActive) myMinion.TakeDamage(damage, attacker, attackerPos);
            }

            //2. Enemy
            if (reciever.GetComponent<Enemy>() != null)
            {
                Enemy myEnemy = reciever.GetComponent<Enemy>();
                myEnemy.TakeDamage(damage, attacker, attackerPos);

            }

            //3. player
            if (reciever.GetComponent<PlayerHealthBar>() != null)
            {
                PlayerHealthBar playerHealth = reciever.GetComponent<PlayerHealthBar>();
                playerHealth.TakeDamage(damage, attacker, attackerPos);
            }
        }
    }

    public  void DealRangeDamage(GameObject attacker, Vector3 damagePos, float range, float damage) // if attacker is not a entity, attack everyone
    {
        Collider[] hitedEnemy = Physics.OverlapSphere(damagePos, range, attackMask);
        // indiscriminate attack
        if (attacker = null)// Placeholder
        {
            for (int i = 0; i < hitedEnemy.Length; i++)
            {

            }
        }
    }
}
