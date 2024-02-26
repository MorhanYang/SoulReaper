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

    //******************************************************************** Single Attack *******************************************************************
    /* Reciever is for specific attack target. for normal attack recieve is null.
     * Attacker is used to identify the attaker typle, for example <Player>
     */
    public void DealSingleDamage(Transform attacker, Vector3 attackerPos, Transform reciever, float damage) 
    {
        // *****************find reciever
        if (reciever == null){

            Collider[] hitedEnemy = Physics.OverlapSphere(attackerPos, 0.3f, attackMask);

            // no attacker
            if (attacker == null)
            {
                Debug.Log("No attacker && reciever");
                return;
            }
            // attacker is player or minion
            else
            {
                if (attacker.GetComponent<PlayerHealthBar>() != null || attacker.GetComponent<Minion>() != null)
                {
                    for (int i = 0; i < hitedEnemy.Length; i++)
                    {
                        if (hitedEnemy[i].GetComponent<EnemyScript>() != null)
                        {
                            reciever = hitedEnemy[i].transform;
                            continue;
                        }
                    }
                }
                // Attacker is enemy
                else if (attacker.GetComponent<EnemyScript>() != null)
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
            if (reciever.GetComponent<EnemyScript>() != null)
            {
                EnemyScript myEnemy = reciever.GetComponent<EnemyScript>();
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

    //************************************************************************* AOE *************************************************************
    /* Attacker is used to identify the attaker typle, for example <Player>
     * If Attacker is null, it will damage every entity including itself.
    */
    public void DealAOEDamage(Transform attacker, Vector3 damagePos, float range, float damage) // if attacker is not a entity, attack everyone
    {
        Collider[] hitedEnemy = Physics.OverlapSphere(damagePos, range, attackMask);

        if (hitedEnemy.Length > 0)
        {
            // 1. indiscriminate attack
            if (attacker == null)// Placeholder
            {
                for (int i = 0; i < hitedEnemy.Length; i++)
                {
                    DealSingleDamage(null, damagePos, hitedEnemy[i].transform, damage);
                }
            }

            // 2. enemy attack
            if (attacker.GetComponent<Enemy>() != null)
            {
                for (int i = 0; i < hitedEnemy.Length; i++)
                {
                    DealSingleDamage(attacker, attacker.transform.position, hitedEnemy[i].transform, damage);
                }
            }

            //3. Player or minion 
            if (attacker.GetComponent<Minion>() != null || attacker.GetComponent<PlayerHealthBar>() != null)
            {
                for (int i = 0; i < hitedEnemy.Length; i++)
                {
                    DealSingleDamage(attacker, attacker.transform.position, hitedEnemy[i].transform, damage);
                }
            }
        }
    }
}
