using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour
{
    MinionAI myAI;
    NavMeshAgent myagent;
    AbsorbableMark absorbableMark;
    CursorManager cursorManager;
    PlayerHealth playerHealth;
    TroopManager troopManager;

    public int minionType = 0; // normal 0, special 1, vines 2; 
    [SerializeField] Animator myAnimator;
    [SerializeField] GameObject recallingMinion;
    [SerializeField] float getDamageRate = 0.5f;
    [SerializeField] GameObject rebirthIcon;
    [SerializeField] GameObject RebirthIcon_Select;
    [SerializeField] GameObject SelectEffect;


    public bool isActive = false;

    // Combat
    float initaldamage;
    public float MaxHp = 30;
    public float presentHp = 30f;

    // Position on Troop and Minion list
    int[] minionDataPos;
    public int[] GetMinionDataPos() { return minionDataPos; }
    public void SetMinionDataPos( int x, int y ) { minionDataPos[0] = x; minionDataPos[1] = y; }

    // Minion Size
    public int minionSize = 1; // only can be maxTroopCapacity(PlayerHealthBar) or 1

    // extra control
    [SerializeField] bool isTrigger;
    [SerializeField] Puzzle_Bridge myBridge;
    [SerializeField] Puzzle_Vine myVines;

    //effect 
    Shaker shaker;
    SoundManager mySoundManager;

    private void Awake()
    {
        myAI = GetComponent<MinionAI>();
        myagent= GetComponent<NavMeshAgent>();
        initaldamage = myAI.attackDamage;
    }

    private void Start()
    {
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        playerHealth = PlayerManager.instance.player.GetComponent<PlayerHealth>();
        absorbableMark = GetComponent<AbsorbableMark>();
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();
        shaker = GetComponent<Shaker>();
        mySoundManager = SoundManager.Instance.GetComponent<SoundManager>();
    }

    private void Update(){
        if(myAnimator != null) myAnimator.SetFloat("MovingSpeed", myagent.velocity.magnitude);
    }


    private void OnMouseEnter()
    {
        ActivateSelected();
    }
    private void OnMouseExit()
    {
        DeactivateSeleted();
    }

    //******************************************************combate*********************************************************
    public void SetDealDamageRate(float rate){
        if (!isTrigger) myAI.attackDamage = initaldamage * rate;
    }
    
    public void SprintToPos(Vector3 pos){
        myAI.SprinteToPos(pos);
    }

    public void SprintToEnemy(Transform enemy )
    {
        myAI.SprintToEnemy(enemy);
    }

    public void TakeDamage(float damage, Transform damageDealer, Vector3 attackPos){

        presentHp -= damage;

        // dead
        if (presentHp < 0){
            presentHp = 0;
            troopManager.EnemyKillOneMinion(this);
        }
        // alive
        else
        {
            troopManager.RefreshOneMinionInfo(this);
        }

        //knock back
        shaker.AddImpact((transform.position - attackPos), damage, false);

        // play sound 
        mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 0.5f, 100, 100);
    }
    //*****************************************************Revieve or recall Minion******************************************

    public void SetRebirthSelect(bool state){ 
        RebirthIcon_Select.SetActive(state);
    }

    public bool SetActiveDelay(float delay, int[] myMinionDataPos)
    {
        minionDataPos = myMinionDataPos;
        if (!isActive){
            Invoke("ActiveMinion", delay);

            // play recall animation
            rebirthIcon.SetActive(false);
            SetRebirthSelect(false);
            // get a recalling minion from player
            GameObject effect = Instantiate(recallingMinion, PlayerManager.instance.player.transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(transform);

            if (myAnimator != null){
                myAnimator.SetBool("Rebirth", true);
                myAnimator.SetBool("Dying", false);
            }

            isActive = true;

            return true;
        }
        return false;
    }
    void ActiveMinion()
    {
        if (isActive){
            // normal minion
            if (!isTrigger)
            {
                gameObject.layer = LayerMask.NameToLayer("MovingMinion");
                myAI.ActivateMinion();
                absorbableMark.enabled = true;

                // move minion to cant destory set
                transform.parent = UsefulItems.instance.minionSet;
            }

            // trigger
            if (isTrigger)
            {
                if (myBridge != null) myBridge.AddObject(1);
                if (myVines != null) myVines.AddObject(1);
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        else{
            if (myAnimator != null)
            {
                rebirthIcon.SetActive(true);

                myAnimator.SetBool("Rebirth", false);
                myAnimator.SetBool("Dying", true);
            }
        }
    }

    internal bool SetActiveDelay(object rebirthDelay)
    {
        throw new NotImplementedException();
    }

    public void SetInactive()
    {
        // normal minion
        if (!isTrigger)
        {
            myAI.InactiveMinion();
            // remove minion from can't destroy set
            transform.parent = null;
        }
        // trigger
        if (isTrigger)
        {
            if (myBridge != null) myBridge.DetractObject(1);
            if (myVines) myVines.DetractObject(1);
        }
        // set data
        gameObject.layer = LayerMask.NameToLayer("Minion");

        rebirthIcon.SetActive(true);
        if (myAnimator != null)
        {
            myAnimator.SetBool("Dying", true);
            myAnimator.SetBool("Rebirth", false);
        }

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
        playerHealth.MarkRegainTarget(transform);
    }

    public void DeactivateSeleted(){
        cursorManager.ActivateDefaultCursor();
        SelectEffect.SetActive(false);
        // unmark recall troop
        playerHealth.MarkRegainTarget(null);
    }

    //******************************************* Health *********************************************************
    public float GetHealthPercentage(){
        return presentHp / MaxHp;
    }
    public void SetHealthPercentage( float Percentage )
    {
        presentHp = Percentage * MaxHp;
    }
}
