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

    // self destory
    float selfDestroyTimer = 0;
    [SerializeField] float lifespan = 15f;


    private void Awake()
    {
        presentHealth = maxHealth;
        myAI = GetComponent<MinionAI>();
    }

    private void Update()
    {
        // destory itself if it is not added to troop
        if (myTroop == null)
        {
            selfDestroyTimer += Time.deltaTime;
            if (selfDestroyTimer > lifespan){
                Destroy(gameObject);
            }
        }
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
