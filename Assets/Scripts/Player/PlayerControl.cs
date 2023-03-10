using Fungus;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    PlayerHealthBar hp;
    [SerializeField] GameManager gameManager;

    Vector3 move;
    Rigidbody rb;

    Vector3 aimPos;
    Vector3 aimDir;
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
    [SerializeField] GameObject attackEffect;

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
    int assignTroopID = 0;

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
    [SerializeField] float delayBeforeInvincible = 0.1f;
    [SerializeField] float invincibleDuration = 0.4f;

    //soul list ( new )
    bool isFacingRight = true;

    //Animation
    Animator characterAnimator;
    //shacker
    Shaker shacker;

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        hp = GetComponent<PlayerHealthBar>();
        shacker= GetComponent<Shaker>();

        combateState = CombateState.normal;

        presentRollingSpeed = rollingSpeed;

        attackPoint = attackFlipAix.Find("Aim");

        assignTroopID = 0;
    }

    void Update()
    {
        //*****************************************ControlPanel******************************************
        // aim
        MouseAimFunction();

        //Mouse Control Combo
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
            StartCoroutine("ExecuteMouseControl");
        }

        // rolling
        if (actionTimer < actionColdDown){
            actionTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (actionTimer >= actionColdDown){
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
            // Recall Function
            if (mouseInputCount % 10 > 0 && mouseInputCount / 10 > 0){
                // Left & Right Click + Hold
                if (gameManager.IsSpellIsReady(3))
                {
                    //Activate CD UI
                    gameManager.ActivateSpellCDUI(3);
                    // Excute Function
                    hp.RebirthTroop(aimPos, 1.5f);
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
            // Rebirth Fucntion
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

    //**********************************************************Moving Function***************************************************************************
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
        hp.Invincible(delayBeforeInvincible, invincibleDuration);

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

    //**********************************************************************Aiming Function****************************************************
    void MouseAimFunction()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            aimPos = hitInfo.point;
        }
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
            yield return new WaitForEndOfFrame();
        }

        // recall all
        if (recallMinionTimer >= holdTime)
        {
            // recall all troops
            List<MinionTroop> allTroop = hp.GetActivedTroop();
            int recallTimes = allTroop.Count; // allTroop.Count will change after the reacall
            for (int i = 0; i < recallTimes; i++){
                hp.RegainHP();
            }
        }
    }
    //************************************************** Assign Troop *******************************************
    void AssignOneMinion()
    {
        // assign single minion
        List<MinionTroop> Mytroop = hp.GetActivedTroop();
        // check if a minion is left. if so send it out.
        if (Mytroop.Count > 0 && !Mytroop[assignTroopID].AssignOneMinionTowards(aimPos)){
            if ((assignTroopID + 1) >= (Mytroop.Count - 1)){
                assignTroopID = 0;
            } else assignTroopID++;
        }

        assignMinionTimer = 0;
        StartCoroutine("ContinueAssignMinion");

    }

    IEnumerator ContinueAssignMinion()
    {
        float holdTime = 0.4f;
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
    //***************************************************Flip the character*********************************
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

    // *********************************************************************Combat *********************************************
    public void PlayerTakeDamage(float damage, Transform damageDealer)
    {
        hp.TakeDamage(damage);
        shacker.AddImpact(transform.position - damageDealer.position, damage, false);
        hp.Invincible(0f, invincibleDuration);

        if (hp.presentHealth <= 0){
            Debug.Log("You died");
        }
    }
    // melee attack
    void MeleeAttack(float scrollData)
    {
        if (scrollData > 0)
        {
            // upward Attack
            if (upAttackTimer >= attackCD && downAttackTimer >= 0.4f)
            {
                Debug.Log("upward");
                GameObject effect = Instantiate(attackEffect, transform.position + new Vector3(0, 0.1f, 0.1f), transform.rotation, transform);
                if (isFacingRight) effect.GetComponent<Animator>().Play("Upward Attack");
                else effect.GetComponent<Animator>().Play("Upward Attack_Flip");

                Destroy(effect, 0.5f);
                DamageEnemy();
                upAttackTimer = 0;
            }
        }

        if (scrollData < 0)
        {
            // Downward Attack
            if (downAttackTimer >= attackCD && upAttackTimer >= 0.4f)
            {
                Debug.Log("downward");
                GameObject effect = Instantiate(attackEffect, transform.position + new Vector3(0, 0.1f, 0.1f), transform.rotation, transform);
                if(isFacingRight) effect.GetComponent<Animator>().Play("Downward Attack");
                else effect.GetComponent<Animator>().Play("Downward Attack_Flip");

                Destroy(effect, 0.5f); 
                DamageEnemy();
                downAttackTimer = 0;
            }
        }
    }
    void DamageEnemy()
    {
        Collider[] HitedEnemy = Physics.OverlapSphere(attackPoint.position, 0.15f, LayerMask.GetMask("Enemy"));
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
