using Fungus;
using System.Collections.Generic;
using UnityEditor;
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
    [SerializeField]Transform[] soulGenerator;

    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject aimPivot;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    //recall
    float recallTimer = 0;

    //rolling
    enum CombateState{
        normal,
        shooting,
        rolling,
        recalling,
        superdashing,
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

    public enum PlayerState{
        normal,
        combat,
    }
    [HideInInspector] public PlayerState playerState;

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        aim = aimPivot.transform.Find("Aim");
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        hp = GetComponent<PlayerHealthBar>();

        playerState = PlayerState.combat;
        combateState = CombateState.normal;

        presentRollingSpeed = rollingSpeed;


    }

    void Update()
    {
        //*****************************************ControlPanel******************************************
        // shooting and rolling
        switch (playerState)
        {
            case PlayerState.normal:
                if (aim.gameObject.activeSelf){
                    aim.gameObject.SetActive(false);
                }

                // recover Action CD;
                if (actionTimer <= actionColdDown)
                {
                    actionTimer += Time.fixedDeltaTime;
                }
                break;
            case PlayerState.combat:
                // aim
                MouseAimFunction();
                // shoot
                if (Input.GetMouseButtonDown(0)){
                    AssignSouls(aimPos);
                }
                if (Input.GetMouseButtonUp(0)){
                    combateState = CombateState.normal;
                }
                // rolling
                if (Input.GetKeyDown(KeyCode.Space)){

                    if (actionTimer >= actionColdDown){
                        // stop recalling if player is reclling
                        if (combateState == CombateState.recalling){
                            EndRecallFunction();
                        }
                        combateState = CombateState.rolling;
                        actionTimer = 0;
                    }
                }
                // Rebirth Enemy
                if (Input.GetKeyDown(KeyCode.Alpha1)){
                    hp.RebirthTroop(aimPos , 2f);
                }
                break;
        }
        // if player talk to anyone, force player to become normal state
        if (gameManager.fungusFlowchart.HasExecutingBlocks()){
            playerState = PlayerState.normal;
        }
        else{
            //switch combat state
            if (Input.GetKeyDown(KeyCode.M)){
                SwitchPlayerState();
            }
        }

        //recall control
        if (Input.GetMouseButtonDown(1) && combateState == CombateState.normal)
        {
            combateState = CombateState.recalling;
        }
        if (Input.GetMouseButtonUp(1))
        {
            EndRecallFunction();
        }

    }

    private void FixedUpdate()
    {
        switch (combateState)
        {
            case CombateState.normal:
                MoveFunction(1f);

                characterAnimator.SetBool("IsRolling", false);// prevent rolling all the time.
                // recover CD;
                if (actionTimer <= actionColdDown){
                    actionTimer += Time.fixedDeltaTime;
                }
                break;
            case CombateState.shooting:
                break;
            case CombateState.rolling:
                RollFunction();
                break;
            case CombateState.recalling:
                MoveFunction(0.5f);
                RecallFunction();
                break;
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
        // enable aim
        if (!aim.gameObject.activeSelf){
            aim.gameObject.SetActive(true);
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            aimPos = hitInfo.point;
        }

        //Get the aim direction and convert it to degree angle
        aimDir = aimPos - aimPivot.transform.position;
        aimDir= aimDir.normalized;
        float angle = Mathf.Atan2(aimDir.z, aimDir.x) * Mathf.Rad2Deg;
        aimPivot.transform.eulerAngles = new Vector3(0, -angle, 0);
        // aim won't rotate
        aim.eulerAngles = new Vector3(0, 0, 0);
    }

    // generate soul and shoot it towards mouse
    void AssignSouls(Vector3 destination)
    {
        List<MinionTroop> Mytroop = hp.GetActivedTroop();
        if (Mytroop.Count > 0){
            rb.velocity = Vector3.zero;

            // find every active troops
            for (int i = 0; i < Mytroop.Count; i++)
            {
                Mytroop[i].AssignTroopTowards(destination);
            }
        }
        else Debug.Log("There is no troop");
    }

    //********************************************************************Recalling Function****************************************************************

    private void RecallFunction(){
        // start animation
        characterAnimator.SetBool("IsRecalling", true);
        
        recallTimer += Time.fixedDeltaTime;

        float holdtime = 0.2f;
        //enough time
        if (recallTimer >= holdtime)
        {
            hp.RegainHP();
            recallTimer = 0;
        }
    }
    //reset recall 
    private void EndRecallFunction(){
        // End animation
        characterAnimator.SetBool("IsRecalling", false);

        recallTimer = 0;
        combateState = CombateState.normal;
    }

  
    //*******************************State
    void SwitchPlayerState() {
        if (playerState == PlayerState.combat){
            playerState = PlayerState.normal;
            //change cursor
            CursorManager.instance.ActivateDefaultCursor();
        }
        else if (playerState == PlayerState.normal){
            playerState = PlayerState.combat;
            //change cursor
            CursorManager.instance.ActivateCombatCursor();
        }
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
}
