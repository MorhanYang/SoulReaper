
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinionTroop : MonoBehaviour
{
    Health health;
    List<int> troopMembers;
    List<Minion> assignedTroopMember;

    [SerializeField] GameObject[] minionTemple;
    [SerializeField] TMP_Text memberNum;
    public int MaxMember = 8;

    private void Start()
    {
        health= GetComponent<Health>();
        troopMembers= new List<int>();
        assignedTroopMember= new List<Minion>();

        UpdateMemberNumText();
    }
    //********************************************Add & Remove Members********************************************************
    public GameObject GetMinionTemple(int TempleNum)
    {
        return minionTemple[TempleNum];
    }

    public float GetMinionNumber()
    {
        return troopMembers.Count;
    }

    void UpdateMemberNumText()
    {
        memberNum.text = troopMembers.Count + "/" + MaxMember;
    }

    public bool AddTroopMember(Minion member) {

        bool canAdd;

        if (assignedTroopMember.Contains(member))
        {
            int memberTypeID = member.MinionType;
            troopMembers.Add(memberTypeID);
            assignedTroopMember.Remove(member);

            canAdd = true;
        }
        else if ((troopMembers.Count + assignedTroopMember.Count) < MaxMember)
        {
            // if member is not in the assignedTroopMember
            int memberTypeID = member.MinionType;
            troopMembers.Add(memberTypeID);
            
            health.Maxhealth += member.maxHealth;
            health.presentHealth += member.presentHealth;

            health.HealthUpdate();

            canAdd = true;
        }
        else canAdd = false;

        UpdateMemberNumText();
        return canAdd;
    }

    public GameObject GenerateMinion(Vector3 pos)
    {
        GameObject removedMember = minionTemple[troopMembers[0]];
        troopMembers.RemoveAt(0);
        // generate a minion
        GameObject soul = Instantiate(removedMember, pos, Quaternion.Euler(Vector3.zero));
        assignedTroopMember.Add(soul.GetComponent<Minion>());

        UpdateMemberNumText();
        return soul;
    }


    // ******************************************* Combat ********************************************************************
    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);

        if (health.presentHealth < 0 && assignedTroopMember.Count >0 )
        {
            foreach (Minion item in assignedTroopMember)
            {
                PlayerManager.instance.player.GetComponent<PlayerControl>().RemoveMinionFromList(item);

                if (item != null)
                {
                    item.GetComponent<Collider>().enabled = false;
                    Destroy(item.gameObject);
                }
            }

            assignedTroopMember.Clear();
            // reset health
            health.presentHealth= 1;
            health.Maxhealth = 1;
            health.HealthUpdate();
        }
    }

    public float GetPresentHP()
    {
        return health.presentHealth;
    }

    public void HealTroops()
    {
        health.presentHealth = health.Maxhealth;
        health.HealthUpdate();
    }
}
