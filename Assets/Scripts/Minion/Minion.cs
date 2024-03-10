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


    public enum MinionStyle
    {
        Rats,
        Normal,
        Range,
        Dash,
        Vine,
    }
    public MinionStyle minionStyle = 0; // normal 0, special 1, vines 2; 
    [SerializeField] Animator myAnimator;
    [SerializeField] GameObject recallingMinion;
    [SerializeField] SpriteRenderer headIcon;

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

        headIcon.sprite = HeadIconManager.GetSprite("Revive");
    }

    private void Update(){
        if(myAnimator != null) myAnimator.SetFloat("MovingSpeed", myagent.velocity.magnitude);
    }


    private void OnMouseEnter()
    {
        ActiveMarker();
    }
    private void OnMouseExit()
    {
        StartCoroutine(WaitforMouseReleasetoDeactivateMarker());
    }



    //****************************************************** Marker Control ***********************************************************
    void ActiveMarker()
    {
        // prevent that player is holding mouse but also select the minion
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            if (isActive)// mark as ate Target
            {
                ActivateEatMarker();
            }
            else// Mark as revive Target
            {
                ActivateReviveMarker();
            }
        }
    }
    
    IEnumerator WaitforMouseReleasetoDeactivateMarker()
    {
        while (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            yield return new WaitForSeconds(0.5f);

        }

        if (isActive)// mark as ate Target
        {
            DeactivateEatSeleted();
        }
        else// Mark as revive Target
        {
            DeactivateReviveMarker();
        }
    }
    //************************** Revive Marker
    public void ActivateReviveMarker()
    {
        cursorManager.ActivateRevieveCursor();
        headIcon.sprite = HeadIconManager.GetSprite("ReviveSel");
        // mark revieve Minion
        troopManager.MarkedReviveMinion(transform);
    }

    public void DeactivateReviveMarker()
    {
        cursorManager.ActivateDefaultCursor();
        headIcon.sprite = HeadIconManager.GetSprite("Revive");
        // unmark recall troop
        troopManager.MarkedReviveMinion(null);
    }

    //************************** Health Marker
    public float GetHealthPercentage()
    {
        return presentHp / MaxHp;
    }
    public void SetHealthPercentage(float Percentage)
    {
        presentHp = Percentage * MaxHp;
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
    //******************************************************Take Damage*********************************************************
    public void TakeDamage(float damage, Transform damageDealer, Vector3 attackPos){

        presentHp -= damage;

        // dead
        if (presentHp < 0){
            presentHp = 0;
            troopManager.EnemyKillOneMinion(this);

            headIcon.sprite = HeadIconManager.GetSprite("Revive");
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
        if (state)
        {
            headIcon.sprite = HeadIconManager.GetSprite("ReviveSel");
        }
        else headIcon.sprite = HeadIconManager.GetSprite("Revive");
    }

    public bool SetActiveDelay(float delay, int[] myMinionDataPos)
    {
        minionDataPos = myMinionDataPos;
        if (!isActive){
            Invoke("ActiveMinion", delay);

            // play recall animation
            headIcon.sprite = null;
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
                headIcon.sprite = null;
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
                headIcon.sprite = HeadIconManager.GetSprite("Revive");
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

        headIcon.sprite = HeadIconManager.GetSprite("Revive");
        if (myAnimator != null)
        {
            myAnimator.SetBool("Dying", true);
            myAnimator.SetBool("Rebirth", false);
        }

        DeactivateEatSeleted();

        isActive = false;
    }
    public bool CanAssign()
    {
        return myAI.CanAssign();
    }

    // ************************************************ Select Phase ***********************************************
    public void ActivateEatMarker()
    {
        cursorManager.ActivateRecallCursor();
        headIcon.sprite = HeadIconManager.GetSprite("Select");
        // mark recall troop
        playerHealth.MarkRegainTarget(transform);
    }

    public void DeactivateEatSeleted(){
        cursorManager.ActivateDefaultCursor();
        headIcon.sprite = null;
        // unmark recall troop
        playerHealth.MarkRegainTarget(null);
    }


}
