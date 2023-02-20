using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionTroop : MonoBehaviour
{
    Health health;
    float troopHealth;
    List<int> troopMembers;

    [SerializeField] GameObject[] minionTemple;
    [SerializeField] Text memberNum;

    private void Start()
    {
        health= GetComponent<Health>();
        troopMembers= new List<int>();
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

    public void AddTroopMember(Minion member) {

        int memberTypeID = member.MinionType;
        troopMembers.Add(memberTypeID);

        health.Maxhealth += troopHealth;
        Debug.Log(troopMembers.Count);
    }

    public GameObject DetractTroopMember()
    {
        GameObject removedMember = minionTemple[troopMembers[0]];
        troopMembers.RemoveAt(0);

        return removedMember;
    }

    // ******************************************* Combat ********************************************************************
    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
        if (health.presentHealth <= 0)
        {
            Debug.Log("Troop Gone");
        }
    }
}
