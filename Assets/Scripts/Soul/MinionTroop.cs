
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class MinionTroop : MonoBehaviour
{
    [SerializeField] Health health; // serializedField to solve reset problem (can get access before end of the frame)
    PlayerHealthBar playerHealthBar;

    List<Minion> TroopMember;

    [SerializeField] GameObject[] minionTemple;
    [SerializeField] TMP_Text memberNum;
    [SerializeField] int MaxMember = 5;
    [SerializeField] GameObject selectSprite;
    // reduce Damge
    int ReduceDamageStateCount = 0;

    // Minion Size
    int TroopSpaceLeft;

    private void Awake()
    {
        TroopMember = new List<Minion>();
        playerHealthBar = PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
    }

    private void Start()
    {
        UpdateMemberNumText();
        // adjust damage rate
        ReduceMinionsDamge();
    }

    //********************************************Reset Troop Info************************************************************
    public void ResetTroopHP(float hp, int maxCapacity)
    {
        health.Maxhealth = hp;
        health.presentHealth = hp;
        MaxMember = maxCapacity;
        TroopSpaceLeft = maxCapacity;
    }

    //************************************************Sprint***************************************************************
    public void AssignOneMinionTowards(Vector3 destination, Minion assignedMinion)
    {
        // find target
        Collider[] hitedEnemy = Physics.OverlapSphere(destination, 0.3f, LayerMask.GetMask("Enemy", "PuzzleTrigger"));
        Transform target = null;

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, destination);
        // if didn't hit a enemy
        else target = null;

        // show destination marker
        GameManager.instance.GenerateMarker(destination, target);

        // Execute sprint action
        if (target == null) // didn't hit any thing
        {
            // find pos at the navmesh
            NavMeshHit hit;
            Vector3 sprintPos = Vector3.zero;
            if (NavMesh.SamplePosition(destination, out hit, 2.5f, NavMesh.AllAreas))
            {
                sprintPos = hit.position;
            }

            // assign minions
            assignedMinion.SprintToPos(sprintPos);

        }
        if (target != null) // Hit something
        {
            // assign minion to attack
            assignedMinion.SprintToEnemy(target);
        }
    }
    public void AssignTroopTowards(Vector3 destination)
    {
        // find target
        Collider[] hitedEnemy = Physics.OverlapSphere(destination, 0.3f, LayerMask.GetMask("Enemy","PuzzleTrigger"));
        Transform target = null;

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, destination);
        // if didn't hit a enemy
        else target = null;

        // show destination marker
        GameManager.instance.GenerateMarker(destination, target);

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
            for (int i = 0; i < TroopMember.Count; i++){
                TroopMember[i].SprintToPos(sprintPos);
            }
        }
        if(target != null) // Hit something
        {
            for (int i = 0; i < TroopMember.Count; i++){
                TroopMember[i].SprintToEnemy(target);
            }
        }
    }

    // ************************************************* Share property ***************************************
    public GameObject GetMinionTemple(int TempleNum)
    {
        return minionTemple[TempleNum];
    }
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
        // special minion
        if (TroopSpaceLeft <=0 && TroopMember.Count <=1){
            memberNum.text = "Special";
        }
        else memberNum.text = (MaxMember - TroopSpaceLeft) + "/" + MaxMember;
    }

    public void AddTroopMember(Minion member) {
        if (member != null)
        {
            TroopMember.Add(member);
            member.SetTroop(this);
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
                member.SetInactive(false);
                playerHealthBar.RemoveTroopFromPlayerHealth(this,true);
            }
            else
            {
                TroopMember.Remove(member);
                TroopSpaceLeft += member.minionSize;
                member.SetInactive(false);

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
                        item.SetInactive(false);
                        health.presentHealth = 0;
                    }
                }

                playerHealthBar.RemoveTroopFromPlayerHealth(this, true);

                TroopMember.Clear();
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
            TroopMember[i].SetInactive(true); 
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
            TroopMember[i].ActivateSelected();
        }

    }
    public void UnsellectAllMember()
    {
        selectSprite.SetActive(false);
        for (int i = 0; i < TroopMember.Count; i++)
        {
            TroopMember[i].DeactivateSeleted();
        }
    }
}
