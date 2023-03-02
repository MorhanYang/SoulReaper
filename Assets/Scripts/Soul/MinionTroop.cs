
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class MinionTroop : MonoBehaviour
{
    //Health health;
    Health health;
    PlayerHealthBar playerHealthBar;

    List<Minion> assignedTroopMember;

    [SerializeField] GameObject[] minionTemple;
    [SerializeField] TMP_Text memberNum;
    int MaxMember = 6;
    // reduce Damge
    int ReduceDamageStateCount = 0;

    private void Awake()
    {
        health = GetComponent<Health>();
        assignedTroopMember = new List<Minion>();
        playerHealthBar = PlayerManager.instance.player.GetComponent<PlayerHealthBar>();

        UpdateMemberNumText();
    }

    //********************************************Reset Troop Info************************************************************
    public void ResetTroopHP(float hp, int maxMember)
    {
        health.Maxhealth = hp;
        health.presentHealth = hp;
        MaxMember = maxMember;
        // adjust damage rate
        ReduceMinionsDamge();
    }

    //************************************************Sprint***************************************************************
    public void AssignTroopTowards(Vector3 destination)
    {
        // find target
        Collider[] hitedEnemy = Physics.OverlapSphere(destination, 0.3f, LayerMask.GetMask("Enemy","PuzzleTrigger"));
        Transform target = null;

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, destination);
        // if didn't hit a enemy
        else target = null;

        // Execute sprint action
        if (target == null) // didn't hit any thing
        {
            // find pos at the navmesh
            NavMeshHit hit;
            Vector3 sprintPos = Vector3.zero;
            if (NavMesh.SamplePosition(destination, out hit, 2.5f, NavMesh.AllAreas)){
                sprintPos = hit.position;
            }
            // assign minions
            for (int i = 0; i < assignedTroopMember.Count; i++){
                assignedTroopMember[i].SprintToPos(sprintPos);
            }
        }
        if(target != null) // Hit something
        {
            for (int i = 0; i < assignedTroopMember.Count; i++){
                assignedTroopMember[i].SprintToEnemy(target);
            }
        }
    }

    // ************************************************* Share property ***************************************
    public GameObject GetMinionTemple(int TempleNum)
    {
        return minionTemple[TempleNum];
    }
    public int GetTroopSize()
    {
        return assignedTroopMember.Count;
    }
    public List<Minion> GetMinionList()
    {
        return assignedTroopMember;
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
            else if (enemyList[i].GetComponent<Enemy>())
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
        memberNum.text = assignedTroopMember.Count + "/" + MaxMember;
    }

    public void AddTroopMember(Minion member) {
        if (member != null)
        {
            assignedTroopMember.Add(member);
            member.SetTroop(this);

            UpdateMemberNumText();
        }
    }

    // ******************************************* Combat ********************************************************************
    public void TakeDamage(float damage)
    {
        if (health.presentHealth > 0) // prevent executing method for many times
        {
            health.TakeDamage(damage);

            if (health.presentHealth <= 0 && assignedTroopMember.Count > 0)
            {
                foreach (Minion item in assignedTroopMember){
                    if (item != null){
                        item.SetInactive(false);
                        health.presentHealth = 0;
                    }
                }

                playerHealthBar.RemoveTroopFromPlayerHealth(this);

                assignedTroopMember.Clear();
                // reset health
                health.HealthUpdate();
            }
            // adjust damage rate
            ReduceMinionsDamge();
        } 
    }

    public float GetPresentHP()
    {
        return health.presentHealth;
    }

    void ReduceMinionsDamge()
    {
        float healthRate = health.presentHealth / health.Maxhealth;

        // 0.9
        if (healthRate > 0.9f && ReduceDamageStateCount == 0){
            for (int i = 0; i < assignedTroopMember.Count; i++){
                assignedTroopMember[i].SetDealDamageRate(1.2f);
            }
            ReduceDamageStateCount++;
        }
        // 0.4 - 0.9
        else if(healthRate <= 0.9f && healthRate > 0.4 && ReduceDamageStateCount == 1)
        {
            for (int i = 0; i < assignedTroopMember.Count; i++)
            {
                assignedTroopMember[i].SetDealDamageRate(1f);
            }
            ReduceDamageStateCount++;
        }
        // 0.4 or less
        else if (healthRate<= 0.4f && ReduceDamageStateCount == 2)
        {
            for (int i = 0; i < assignedTroopMember.Count; i++)
            {
                assignedTroopMember[i].SetDealDamageRate(0.6f);
            }
            ReduceDamageStateCount++;
        }
    }

    // *********************************************** recall ******************************************
    public void ExecuteMinionRecall()
    {
        for (int i = 0; i < assignedTroopMember.Count; i++)
        {
            assignedTroopMember[i].SetInactive(true);
        }
    }
}
