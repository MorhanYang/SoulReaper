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
                if (attacker.tag == "Player" || attacker.tag == "Minion" || attacker.tag == "MinionAmmo")
                {
                    for (int i = 0; i < hitedEnemy.Length; i++)
                    {
                        if (hitedEnemy[i].GetComponent<EnemyScript>() != null || hitedEnemy[i].GetComponent<Breakable>() != null || hitedEnemy[i].GetComponent<RangeEnemy>() != null)
                        {
                            reciever = hitedEnemy[i].transform;
                            break;
                        }
                    }
                }
                // Attacker is enemy
                else if (attacker.tag == "Enemy" || attacker.tag == "EnemyAmmo")
                {
                    for (int i = 0; i < hitedEnemy.Length; i++)
                    {
                        if (hitedEnemy[i].GetComponent<PlayerHealth>() != null || hitedEnemy[i].GetComponent<Minion>() != null)
                        {
                            reciever = hitedEnemy[i].transform;
                            break;
                        }
                    }
                }
            }
        }

        // **************identify damage reciever
        if (reciever != null)
        {
            //1. Minion
            if (reciever.GetComponent<Minion>() != null && !reciever.gameObject.IsDestroyed())
            {
                Minion myMinion = reciever.GetComponent<Minion>();
                if (myMinion.isActive) myMinion.TakeDamage(damage, attacker, attackerPos);
            }

            //2. Enemy
            if (reciever.GetComponent<BasicEnemy>() != null)
            {
                BasicEnemy myEnemy = reciever.GetComponent<BasicEnemy>();
                myEnemy.TakeDamage(damage, attacker, attackerPos);

            }

            //3. player
            if (reciever.GetComponent<PlayerHealth>() != null)
            {
                PlayerHealth playerHealth = reciever.GetComponent<PlayerHealth>();
                playerHealth.TakeDamage(damage, attacker, attackerPos);
            }

            //4. breakable things
            if (reciever.GetComponent<Breakable>() != null)
            {
                Breakable myBreakable = reciever.GetComponent<Breakable>();
                myBreakable.TakeDamage(damage, attacker, attackerPos);
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


        List<Transform> avalibleList = new List<Transform>();
        // reorganize the HitedEnemyList
        if (hitedEnemy.Length > 0)
        {
            // identify attacker:
            // Player or Minion attack
            if (attacker.tag == "Player" || attacker.tag == "Minion" || attacker.tag == "MinionAmmo")
            {
                for (int i = 0; i < hitedEnemy.Length; i++)
                {
                    if (hitedEnemy[i].GetComponent<EnemyScript>() != null || hitedEnemy[i].GetComponent<Breakable>() != null || hitedEnemy[i].GetComponent<RangeEnemy>() != null)
                    {
                        avalibleList.Add(hitedEnemy[i].transform);
                    }
                }
            }
            // Enemy Attack
            else if (attacker.tag == "Enemy" || attacker.tag == "EnemyAmmo")
            {
                for (int i = 0; i < hitedEnemy.Length; i++)
                {
                    if (hitedEnemy[i].GetComponent<PlayerHealth>() != null || hitedEnemy[i].GetComponent<Minion>() != null)
                    {
                        avalibleList.Add(hitedEnemy[i].transform);
                    }
                }
            }

            // deal damage to each target:
            for (int i = 0; i < avalibleList.Count; i++)
            {
                DealSingleDamage(attacker, damagePos, avalibleList[i].transform, damage);
            }
        }
    }
}
