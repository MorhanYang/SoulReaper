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
    [SerializeField] GameObject rebirthIcon;
    [SerializeField] GameObject SelectEffect;

    public bool isActive = false;

    float initaldamage;

    CursorManager cursorManager;
    PlayerHealthBar playerHealthBar;
    Shaker shaker;

    private void Awake()
    {
        myAI = GetComponent<MinionAI>();
        initaldamage = myAI.attackDamage;
    }

    private void Start()
    {
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        playerHealthBar = PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        shaker = GetComponent<Shaker>();
    }

    private void OnMouseEnter()
    {
        cursorManager.ActivateRecallCursor();
        SelectEffect.SetActive(true);
        // mark recall troop
        playerHealthBar.MarkTroop(myTroop);
    }
    private void OnMouseExit()
    {
        cursorManager.ActivateDefaultCursor();
        SelectEffect.SetActive(false);
        // unmark recall troop
        playerHealthBar.MarkTroop(null);
    }

    //*********************************************************Method*******************************************************
    public void SetTroop(MinionTroop Troop)
    {
        myTroop = Troop;
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

    public void TakeDamage(float damage, Transform damageDealer){
        if (myTroop != null && myTroop.GetPresentHP() > 0){
            shaker.AddImpact(transform.position - damageDealer.position, damage, false);
            myTroop.TakeDamage(damage * getDamageRate);
        }
    }
    //*****************************************************Change Minion State******************************************

    public void SetActiveDelay(float delay)
    {
        Invoke("ActiveMinion", delay);
        isActive = true;

        // play recall animation
        rebirthIcon.SetActive(false);
        // get a recalling minion from player
        GameObject effect = Instantiate(recallingMinion, PlayerManager.instance.player.transform.position, transform.rotation);
        effect.GetComponent<RecallingMinion>().AimTo(transform);

        myAnimator.SetBool("Rebirth", true);
        myAnimator.SetBool("Dying", false);
    }
    void ActiveMinion()
    {
        if (isActive){
            gameObject.layer = LayerMask.NameToLayer("MovingMinion");
            myAI.ActivateMinion();
        }
    }

    public void SetInactive(bool needRecallEffect)
    {
        myAI.InactiveMinion();
        gameObject.layer = LayerMask.NameToLayer("Minion");

        // play recall animation
        if (needRecallEffect){
            GameObject effect = Instantiate(recallingMinion, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(PlayerManager.instance.player.transform);
        }
        
        rebirthIcon.SetActive(true);
        myAnimator.SetBool("Dying", true);
        myAnimator.SetBool("Rebirth", false);

        isActive = false;
    }

}
