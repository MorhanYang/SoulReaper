
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class MinionTroop : MonoBehaviour
{
    [SerializeField] Health health; // serializedField to solve reset problem (can get access before end of the frame)
    //PlayerHealthBar playerHealthBar;
    [SerializeField]MinionBarIcon minionIconManager;

    List<Minion> TroopMember;

    [SerializeField] int MaxMember = 5;
    [SerializeField] GameObject selectSprite;
    [SerializeField] MinionMoverMarker myMarker;
    // reduce Damge
    int ReduceDamageStateCount = 0;

    // Minion Size
    int TroopSpaceLeft;

    private void Awake()
    {
        TroopMember = new List<Minion>();
    }

    private void Start()
    {
        UpdateMemberNumText();
        // adjust damage rate
        ReduceMinionsDamge();
    }

    //******************************************** Reset Troop Info ************************************************************
    public void ResetTroopHP(float presentHP, float MaxHP, int maxCapacity)
    {
        health.Maxhealth = MaxHP;
        health.presentHealth = presentHP;
        MaxMember = maxCapacity;
        TroopSpaceLeft = maxCapacity;

        health.HealthUpdate();
    }

    // ************************************************* Share property ***************************************
    public int GetTroopEmptySpace()
    {
        return TroopSpaceLeft;
    }
    public List<Minion> GetMinionList()
    {
        return TroopMember;
    }
    //********************************************Add & Remove Members********************************************************
    private Transform GetClosestEnemyTransform(Collider[] enemyList, Vector3 referencePoint)
    {
        Transform closedEnemy = null;

        for (int i = 0; i < enemyList.Length; i++)
        {
            // get trigger
            if (enemyList[i].GetComponent<PuzzleTrigger>()!= null)
            {
                closedEnemy = enemyList[i].transform;
                return closedEnemy;
            }
            // get enemy
            else if (enemyList[i].GetComponent<EnemyScript>())
            {
                Collider testEnemy = enemyList[i];
                if (closedEnemy == null)
                {
                    closedEnemy = testEnemy.transform;
                }
                // test which is closer
                else if (Vector3.Distance(testEnemy.transform.position, referencePoint) > Vector3.Distance(closedEnemy.transform.position, referencePoint))
                {
                    closedEnemy = testEnemy.transform;
                } 
            }
        }
        return closedEnemy;
    }

    void UpdateMemberNumText()
    {
        // special & normal 
        minionIconManager.UpdateMinionIcon(TroopMember[0].minionType, MaxMember - TroopSpaceLeft);
    }

    public void AddTroopMember(Minion member) {
        if (member != null && !TroopMember.Contains(member))
        {
            TroopMember.Add(member);
            //member.SetTroop(this);
            TroopSpaceLeft -= member.minionSize;

            UpdateMemberNumText();
        }
    }

    public void RemoveTroopMember(Minion member)
    {
        if (member != null)
        {
            //if don't have minions left in the troop
            if (TroopMember.Count <= 1)
            {
                TroopMember.Remove(member);
                //member.SetInactive(false);
                //playerHealthBar.RemoveTroopFromPlayerHealth(this,true);
            }
            else
            {
                TroopMember.Remove(member);
                TroopSpaceLeft += member.minionSize;
                //member.SetInactive(false);

                UpdateMemberNumText();
            }
        }
    }

    // ******************************************* Combat ********************************************************************
    public void TakeDamage(float damage)
    {
        if (health.presentHealth > 0) // prevent executing method for many times
        {
            health.TakeDamage(damage);

            if (health.presentHealth <= 0 && TroopMember.Count > 0)
            {
                foreach (Minion item in TroopMember){
                    if (item != null){
                        //item.SetInactive(false);
                        health.presentHealth = 0;
                    }
                }

                //playerHealthBar.RemoveTroopFromPlayerHealth(this, true);

                TroopMember.Clear();
                // reset health
                health.HealthUpdate();
            }
            // adjust damage rate
            ReduceMinionsDamge();
        } 
    }

    public float HealTroop( float healNum )
    {
        float leftHealth = healNum - (health.Maxhealth - health.presentHealth);

        if (leftHealth < 0) {
            health.presentHealth += healNum;
            health.HealthUpdate();
            return 0;
        }
        else { 
            health.presentHealth = health.Maxhealth;
            health.HealthUpdate();
            return leftHealth;
        }

        
    }

    public float GetPresentHP()
    {
        return health.presentHealth;
    }
    public float GetMaxHP()
    {
        return health.Maxhealth;
    }

    void ReduceMinionsDamge()
    {
        float healthRate = health.presentHealth / health.Maxhealth;

        // 0.9
        if (healthRate > 0.9f && ReduceDamageStateCount == 0){
            for (int i = 0; i < TroopMember.Count; i++){
                TroopMember[i].SetDealDamageRate(1.2f);
            }
            ReduceDamageStateCount++;
        }
        // 0.4 - 0.9
        else if(healthRate <= 0.9f && healthRate > 0.4 && ReduceDamageStateCount == 1)
        {
            for (int i = 0; i < TroopMember.Count; i++)
            {
                TroopMember[i].SetDealDamageRate(1f);
            }
            ReduceDamageStateCount++;
        }
        // 0.4 or less
        else if (healthRate<= 0.4f && ReduceDamageStateCount == 2)
        {
            for (int i = 0; i < TroopMember.Count; i++)
            {
                TroopMember[i].SetDealDamageRate(0.6f);
            }
            ReduceDamageStateCount++;
        }
    }

    // *********************************************** recall ******************************************
    public void ExecuteMinionRecall()
    {
        for (int i = 0; i < TroopMember.Count; i++)
        {
            //TroopMember[i].SetInactive(true); 
        }
        // change cursor
        GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();

        TroopSpaceLeft = MaxMember;
    }
    public void SellectAllMember()
    {
        selectSprite.SetActive(true);
        for (int i = 0; i < TroopMember.Count ; i++)
        {
            TroopMember[i].ActivateEatMarker();
        }

    }
    public void UnsellectAllMember()
    {
        selectSprite.SetActive(false);
        for (int i = 0; i < TroopMember.Count; i++)
        {
            TroopMember[i].DeactivateEatSeleted();
        }
    }
}