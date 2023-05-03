using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{

    PlayerHealthBar hp;
    [SerializeField] GameManager gameManager;
    [SerializeField] CursorTimer cursorTimer;

    Vector3 move;
    Rigidbody rb;

    Vector3 aimPos;
    Transform aim;
    [SerializeField] Transform[] soulGenerator;

    [SerializeField] LayerMask groundMask;

    // Attack
    [SerializeField] Transform attackFlipAix;
    [SerializeField] float myDamage = 5;
    [SerializeField] float attackCD = 1f;
    Transform attackPoint;
    float upAttackTimer;
    float downAttackTimer;
    [SerializeField] GameObject upAttackEffect;
    [SerializeField] GameObject downAttackEffect;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    // Mouse control
    int mouseInputCount = 0;
    // recall Minion
    float recallMinionTimer = 0;
    // Assign Minion
    float assignMinionTimer = 0;
    // rebirth
    [SerializeField] GameObject rebirthRangeEffect;

    //rolling
    enum CombateState{
        normal,
        rolling,
        teleporting,
    }
    CombateState combateState;
    Vector3 lastMoveDir;
    float presentRollingSpeed;
    [SerializeField] float rollingSpeed = 200f;
    [SerializeField] float rollingResistance = 600f;
    [SerializeField] float invincibleDuration = 0.4f;

    //soul list ( new )
    bool isFacingRight = true;

    //Animation
    Animator characterAnimator;

    // sound 
    SoundManager mySoundManagers;

    // present Level
    [HideInInspector] public string sceneName;
    [HideInInspector] public int landID;

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        hp = GetComponent<PlayerHealthBar>();
        mySoundManagers = SoundManager.Instance;

        combateState = CombateState.normal;

        presentRollingSpeed = rollingSpeed;

        attackPoint = attackFlipAix.Find("Aim");

        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        //*****************************************ControlPanel******************************************
        // aim
        MouseAimFunction();

        //Mouse Control Combo
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()){
            StartCoroutine(ExecuteMouseControl());
        }
        else if (Input.GetMouseButtonDown(1)){
            StartCoroutine(ExecuteMouseControl());
        }

        // rolling
        if (actionTimer < actionColdDown){
            actionTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (actionTimer >= actionColdDown){
                // play sound
                mySoundManagers.PlaySoundAt(transform.position, "PlayerDash", false, false, 1.5f, 1f, 100, 100);

                combateState = CombateState.rolling;
                actionTimer = 0;
            }
        }
       
        // Recover
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            // check Spell CD
            if (gameManager.IsItemIsReady(1)){
                //Activate CD UI
                gameManager.UseItem(1);
                // Excute Function
                hp.ActivateRecover();
            }
        }

        // Melee Combat
        if (upAttackTimer < attackCD) upAttackTimer += Time.deltaTime;
        if (downAttackTimer < attackCD) downAttackTimer+= Time.deltaTime;
        if (Input.GetAxis("Mouse ScrollWheel") != 0){
            MeleeAttack(Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    private void FixedUpdate()
    {
        switch (combateState)
        {
            case CombateState.normal:
                MoveFunction(1f);
                characterAnimator.SetBool("IsRolling", false);// prevent rolling all the time.
                break;
            case CombateState.rolling:
                RollFunction();
                break;
            case CombateState.teleporting:
                break;

        }
    }

    // ******************************************************* Mouse Button Control Function ******************************************************************
    IEnumerator ExecuteMouseControl()
    {
        // left mouse = 1; 
        if (Input.GetMouseButtonDown(0)){
            mouseInputCount += 1;
        }

        // Right mouse = 10;
        if (Input.GetMouseButtonDown(1)){
            mouseInputCount += 10;
        }

        // excute events after delay
        yield return new WaitForSeconds(0.1f);

        if (mouseInputCount != 0){
            // Rebirth Function
            if (mouseInputCount % 10 > 0 && mouseInputCount / 10 > 0){
                // Left & Right Click + Hold
                if (gameManager.IsSpellIsReady(3))
                {
                    //Activate CD UI
                    gameManager.ActivateSpellCDUI(3);
                    // Excute Function
                    StartCoroutine("RebirthTroop");
                    //hp.RebirthTroop(aimPos, 1.5f);
                }
            }
            // Assign Minion
            else if (mouseInputCount % 10 > 0 && mouseInputCount / 10 == 0){
                // left Click
                if (gameManager.IsSpellIsReady(1)){
                    //Activate CD UI
                    gameManager.ActivateSpellCDUI(1);
                    // Excute Function
                    AssignOneMinion();
                }
            }
            // Recall Fucntion
            else if (mouseInputCount % 10 == 0 && mouseInputCount / 10 > 0){
                // Right Click
                if (gameManager.IsSpellIsReady(2))
                {
                    //Activate CD UI
                    gameManager.ActivateSpellCDUI(2);
                    // Excute Function
                    RecallTroops();
                }
            }

            mouseInputCount = 0;
        }
       
    }

    //********************************************************** Moving Function ***************************************************************************
    // Use speedMultiplyer to change speed.
    void MoveFunction(float speedMultiplyer){
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        move = new Vector3(x, rb.velocity.y, z);
        rb.velocity = new Vector3(x * moveSpeed * speedMultiplyer * Time.fixedDeltaTime, 
                                    rb.velocity.y, 
                                    z * moveSpeed * speedMultiplyer * Time.fixedDeltaTime);

        FlipPlayer();
        // last direction for rolling
        if (move != Vector3.zero){
            lastMoveDir = move.normalized;
        }

        //animator control
        characterAnimator.SetBool("IsMoving", move != Vector3.zero);
    }
    void RollFunction() {
        //animation
        characterAnimator.SetBool("IsRolling", true);

        rb.velocity = lastMoveDir * presentRollingSpeed * Time.fixedDeltaTime;
        // invincible time
        hp.Invincible(invincibleDuration);

        // slow down
        presentRollingSpeed -= rollingResistance * Time.fixedDeltaTime;
        if (presentRollingSpeed <= (moveSpeed+ 30f)){

            rb.velocity = Vector3.zero;
            presentRollingSpeed = rollingSpeed;
            
            //Animation
            characterAnimator.SetBool("IsRolling", false);

            combateState = CombateState.normal;
        }
    }

    //********************************************************************** Aiming Function ****************************************************
    void MouseAimFunction()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            aimPos = hitInfo.point;
        }
    }
    // ******************************************************************* Rebirth ********************************************************
    IEnumerator RebirthTroop()
    {
        float timeCount = 0;
        float radius = 1.6f;

        // show range indicator
        GameObject effect = Instantiate(rebirthRangeEffect, aimPos, transform.rotation);
        // control range place
        while (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            yield return new WaitForEndOfFrame();
            effect.transform.position = aimPos;

            timeCount += Time.deltaTime;
            ReviveRangeMark marker = effect.GetComponent<ReviveRangeMark>();

            if (timeCount >= 1f && radius > 0.4f){
                marker.ShrinkMarker();
                //effect.transform.DOScale(effect.transform.localScale * 0.4f, 0.2f);
                //effect.transform.localScale *= 0.6f;
                radius *= 0.45f;

                timeCount = 0;
            }
        }
        //activate rebirth
        hp.ReviveTroopNormal(aimPos, radius);
        Destroy(effect);
    }
    // ************************************************** recall *********************************************
    void RecallTroops()
    {
        hp.RegainHP();
        recallMinionTimer = 0;
        StartCoroutine("ContinueRecallTroops");
    }

    IEnumerator ContinueRecallTroops()
    {
        float holdTime = 0.5f;
        // loop
        while (recallMinionTimer < holdTime && Input.GetMouseButton(1))
        {
            recallMinionTimer += Time.deltaTime;
            if (recallMinionTimer > 0.15f) cursorTimer.ShowCursorTimer(holdTime-0.15f);
            yield return new WaitForEndOfFrame();
        }

        // recall all
        if (recallMinionTimer >= holdTime)
        {
            cursorTimer.HideCursorTimer();
            // recall all troops
            List<MinionTroop> allTroop = hp.GetActivedTroop();
            int recallTimes = allTroop.Count; // allTroop.Count will change after the reacall
            for (int i = 0; i < recallTimes; i++){
                hp.RegainAllTroopHP();
            }
        }else cursorTimer.HideCursorTimer();
    }
    //************************************************** Assign Troop *******************************************
    void AssignOneMinion()
    {
        // assign single minion
        List<MinionTroop> Mytroop = hp.GetActivedTroop();

        // Find closest Minin to the Target
        Minion closestMinion = null;
        for (int i = 0; i < Mytroop.Count; i++){
            List<Minion> minionList = Mytroop[i].GetMinionList();
            for (int j = 0; j < minionList.Count; j++)
            {
                if (closestMinion == null){
                    if (minionList[j].CanAssign()) closestMinion = minionList[j];     
                    
                }else if (Vector3.Distance(minionList[j].transform.position,aimPos) < Vector3.Distance(closestMinion.transform.position, aimPos) && minionList[j].CanAssign())
                {
                    closestMinion = minionList[j];
                }
            }
        }

        // excecute assignment
        if (closestMinion != null){
            closestMinion.GetTroop().AssignOneMinionTowards(aimPos,closestMinion);

            // play sound
            mySoundManagers.PlaySoundAt(transform.position, "AssignMinion", false, false, 1.5f, 1f, 100, 100);
        }

        assignMinionTimer = 0;
        StartCoroutine("ContinueAssignMinion");

    }

    IEnumerator ContinueAssignMinion()
    {
        float holdTime = 0.2f;
        // loop
        while (assignMinionTimer < holdTime && Input.GetMouseButton(0))
        {
            assignMinionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // recall all
        if (assignMinionTimer >= holdTime)
        {
            AssignAllMinions(aimPos);
        }
    }

    void AssignAllMinions(Vector3 destination)
    {
        Debug.Log("Assign All Minions");
        List<MinionTroop> Mytroop = hp.GetActivedTroop();
        if (Mytroop.Count > 0)
        {
            // find every active troops
            for (int i = 0; i < Mytroop.Count; i++)
            {
                Mytroop[i].AssignTroopTowards(destination);
            }
        }
        else Debug.Log("There is no troop");
    }
    //*************************************************** Flip the character *********************************
    void FlipPlayer()
    {
        if (move.x < 0 && isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = true;
            // Flip Attack Point
            attackFlipAix.Rotate(0f,180f,0f);
            isFacingRight = !isFacingRight;
        }
        if (move.x > 0 && !isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = false;
            // Flip Attack Point
            attackFlipAix.Rotate(0f, 180f, 0f);
            isFacingRight = !isFacingRight;
        }
    }
    public void AddSoulList(int SoulType)
    {
        //soulList.AddSoul(SoulType);
    }

    // ********************************************************************* Combat *********************************************
    public void PlayerTakeDamage(float damage, Transform damageDealer)
    {
        hp.TakeDamage(damage, damageDealer);
        hp.Invincible(invincibleDuration);

        if (hp.presentHealth <= 0){
            Debug.Log("You died");
        }
    }
    // melee attack
    void MeleeAttack(float scrollData)
    {
        if (scrollData > 0) {
            // upward Attack
            if (upAttackTimer >= attackCD && downAttackTimer >= 0.4f)
            {
                // sound effect
                mySoundManagers.PlaySoundAt(transform.position, "Swing", false, false, 1.5f, 0.7f, 100, 100);

                if (isFacingRight) Instantiate(upAttackEffect, transform.position + new Vector3(0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
                else Instantiate(upAttackEffect, transform.position + new Vector3(-0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);

                DamageEnemy();
                upAttackTimer = 0;
            }
        }

        if (scrollData < 0){
            // Downward Attack
            if (downAttackTimer >= attackCD && upAttackTimer >= 0.4f)
            {
                if (isFacingRight) Instantiate(downAttackEffect, transform.position + new Vector3(0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
                else Instantiate(downAttackEffect, transform.position + new Vector3(-0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);

                // sound effect
                mySoundManagers.PlaySoundAt(transform.position, "Swing", false, false, 1.5f, 0.7f, 100, 100);

                DamageEnemy();
                downAttackTimer = 0;
            }
        }
    }
    void DamageEnemy()
    {
        Collider[] HitedEnemy = Physics.OverlapSphere(attackPoint.position, 0.5f, LayerMask.GetMask("Enemy"));
        if (HitedEnemy.Length > 0){
            // deal damage to enemies
            for (int i = 0; i < HitedEnemy.Length; i++)
            {
                HitedEnemy[i].GetComponent<Enemy>().TakeDamage(myDamage, transform);
            }
        }
    }

    // ************************************************************* Teleport ***********************************************
    public void SetPlayerToTeleporting(){
        combateState = CombateState.teleporting;
    }
    public void inActivateTeleporting()
    {
        combateState = CombateState.normal;
    }
}
