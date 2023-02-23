using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    MinionTroop myTroop;
    MinionAI myAI;

    [SerializeField] Animator myAnimator;
    [SerializeField] GameObject recallingMinion;
    [SerializeField] float getDamageRate = 0.5f;

    public int MinionType = 0; // refer to MinionTroop;
    public bool isActive = false;

    float initaldamage;

    private void Awake()
    {
        myAI = GetComponent<MinionAI>();
        initaldamage = myAI.attackDamage;

    }

    //*********************************************************Method*******************************************************
    public void SetTroop(MinionTroop Troop)
    {
        myTroop = Troop;
    }

    public void RecallMinion()
    {
        Instantiate(recallingMinion,transform.position,transform.rotation);
        SetInactive();
    }

    //******************************************************combate*********************************************************
    public void SetDealDamageRate(float rate){
        myAI.attackDamage = initaldamage * rate;
    }
    
    public void SprintToPos(Vector3 pos){
        myAI.SpriteToPos(pos);
    }

    public void SprintToEnemy(Transform enemy )
    {
        myAI.SprintToEnemy(enemy);
    }

    public void TakeDamage(float damage){
        if (myTroop != null && myTroop.GetPresentHP() > 0)
        {
            myTroop.TakeDamage(damage * getDamageRate);
        }
    }

    //*****************************************************Change Minion State******************************************

    public void SetActiveDelay(float delay)
    {
        Invoke("ActiveMinion", delay);
        gameObject.layer = 0;

        // play recall animation
        myAnimator.SetBool("Rebirth", true);
        myAnimator.SetBool("Dying", false);
    }
    void ActiveMinion()
    {
        myAI.ActiveMinion();
        isActive = true;
    }

    public void SetInactive()
    {
        myAI.InactiveMinion();
        gameObject.layer = 11;

        // play recall animation
        myAnimator.SetBool("Dying", true);
        myAnimator.SetBool("Rebirth", false);

        isActive = false;
    }

}
