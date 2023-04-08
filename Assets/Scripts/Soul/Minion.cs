using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour
{
    MinionTroop myTroop;
    MinionAI myAI;
    NavMeshAgent myagent;
    SoundManager mySoundManager;

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

    // Minion Size
    public int minionSize = 1; // only can be maxTroopCapacity(PlayerHealthBar) or 1

    private void Awake()
    {
        myAI = GetComponent<MinionAI>();
        myagent= GetComponent<NavMeshAgent>();
        initaldamage = myAI.attackDamage;
    }

    private void Start()
    {
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        playerHealthBar = PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        shaker = GetComponent<Shaker>();
        mySoundManager= SoundManager.Instance;
    }

    private void Update()
    {
        myAnimator.SetFloat("MovingSpeed", myagent.velocity.magnitude);
    }

    private void OnMouseEnter(){
        if (myTroop != null){
            myTroop.SellectAllMember();
        }
    }
    private void OnMouseExit(){
        if (myTroop != null){
            myTroop.UnsellectAllMember();
        }
    }

    //*********************************************************Set Troop & Get Troop*******************************************************
    public void SetTroop(MinionTroop Troop)
    {
        myTroop = Troop;
    }
    public MinionTroop GetTroop(){
        return myTroop;
    }
    //******************************************************combate*********************************************************
    public void SetDealDamageRate(float rate){
        myAI.attackDamage = initaldamage * rate;
    }
    
    public void SprintToPos(Vector3 pos){
        myAI.SprinteToPos(pos);
    }

    public void SprintToEnemy(Transform enemy )
    {
        myAI.SprintToEnemy(enemy);
    }

    public void TakeDamage(float damage, Transform damageDealer){
        if (myTroop != null && myTroop.GetPresentHP() > 0){
            shaker.AddImpact(transform.position - damageDealer.position, damage, false);
            myTroop.TakeDamage(damage * getDamageRate);

            mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 1, 100, 100);
        }
    }
    //*****************************************************Change Minion State******************************************
    public void SetActiveDelay(float delay)
    {
        if (!isActive){
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
        myTroop = null;
        gameObject.layer = LayerMask.NameToLayer("Minion");

        // play recall animation
        if (needRecallEffect){
            GameObject effect = Instantiate(recallingMinion, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(PlayerManager.instance.player.transform);
            // play sound
            mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Release", false, false, 1, 1, 100, 100);
        }
        
        rebirthIcon.SetActive(true);
        myAnimator.SetBool("Dying", true);
        myAnimator.SetBool("Rebirth", false);
        DeactivateSeleted();

        isActive = false;
    }
    public bool CanAssign()
    {
        return myAI.CanAssign();
    }

    // ************************************************ Select Phase ***********************************************
    public void ActivateSelected()
    {
        cursorManager.ActivateRecallCursor();
        SelectEffect.SetActive(true);
        // mark recall troop
        playerHealthBar.MarkRegainTarget(transform);
    }

    public void DeactivateSeleted(){
        cursorManager.ActivateDefaultCursor();
        SelectEffect.SetActive(false);
        // unmark recall troop
        playerHealthBar.MarkRegainTarget(null);
    }

}
