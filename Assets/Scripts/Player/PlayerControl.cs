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
    [SerializeField] Transform attackPoint;
    [SerializeField] float myDamage = 5;
    [SerializeField] float attackCD = 1f;
    float upAttackTimer;
    float downAttackTimer;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    // Mouse control
    int mouseInputCount = 0;
    bool holdMouse = false;

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

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        hp = GetComponent<PlayerHealthBar>();

        combateState = CombateState.normal;

        presentRollingSpeed = rollingSpeed;


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
                    AssignSouls(aimPos);
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
        if (presentRollingSpeed <= 90f){
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

    // generate soul and shoot it towards mouse
    void AssignSouls(Vector3 destination)
    {
        List<MinionTroop> Mytroop = hp.GetActivedTroop();
        if (Mytroop.Count > 0){

            // find every active troops
            for (int i = 0; i < Mytroop.Count; i++)
            {
                Mytroop[i].AssignTroopTowards(destination);
            }
        }
        else Debug.Log("There is no troop");
    }

    // ************************************************** recall *********************************************
    void RecallTroops()
    {
        hp.RegainHP();
        StartCoroutine("ContinueRecallTroops");
    }

    IEnumerator ContinueRecallTroops()
    {
        // hold at the begining
        if (Input.GetMouseButton(1))
        {
            holdMouse = true;
        }

        yield return new WaitForSeconds(0.3f);

        // hold at the begining and now
        if (Input.GetMouseButton(1))
        {
            // recall all troops
            Debug.Log("recall All Troops");
            List<MinionTroop> allTroop = hp.GetActivedTroop();
            int recallTimes = allTroop.Count; // allTroop.Count will change after the reacall
            for (int i = 0; i < recallTimes; i++){
                hp.RegainHP();
            }
        }

        holdMouse = false;
    }

    //***************************************************Flip the character*********************************
    void FlipPlayer()
    {
        if (move.x < 0 && isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = true;
            isFacingRight = !isFacingRight;
        }
        if (move.x > 0 && !isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = false;
            isFacingRight = !isFacingRight;
        }
    }

    public void AddSoulList(int SoulType)
    {
        //soulList.AddSoul(SoulType);
    }

    // *********************************************************************Combat *********************************************
    public void PlayerTakeDamage(float damage)
    {
        hp.TakeDamage(damage);
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
                DamageEnemy();
                downAttackTimer = 0;
            }
        }
    }
    void DamageEnemy()
    {
        Collider[] HitedEnemy = Physics.OverlapSphere(attackPoint.position, 0.3f, LayerMask.GetMask("Enemy"));
        if (HitedEnemy.Length > 0){
            // deal damage to enemies
            for (int i = 0; i < HitedEnemy.Length; i++)
            {
                HitedEnemy[i].GetComponent<Enemy>().TakeDamage(myDamage, gameObject);
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
