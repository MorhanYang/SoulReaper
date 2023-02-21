using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    MinionTroop myTroop;
    MinionAI myAI;

    [SerializeField] GameObject recallingMinion;

    public int MinionType = 0; // refer to MinionTroop;
    public float maxHealth = 30;
    public float presentHealth;


    private void Awake()
    {
        presentHealth = maxHealth;
        myAI = GetComponent<MinionAI>();
    }

    //*********************************************************Method*******************************************************
    public void SetListId(MinionTroop Troop )
    {
        myTroop = Troop;
    }

    public void RecallMinion()
    {
        Instantiate(recallingMinion,transform.position,transform.rotation);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (myTroop != null && myTroop.GetPresentHP() > 0) {
            myTroop.TakeDamage(damage);
        } 
        else presentHealth -= damage;

        if (presentHealth <= 0) Destroy(gameObject);

        Debug.Log("deal Damage to Troop");
    }

    public void SetRebirthDelay(float delay)
    {
        myAI.enabled= false;
        Invoke("RecoverMinionAI", delay);
    }
    void RecoverMinionAI()
    {
        myAI.enabled = true;
    }

}
