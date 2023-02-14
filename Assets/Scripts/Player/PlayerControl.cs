using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    Health hp;
    [SerializeField] GameManager gameManager;

    Vector3 move;
    Rigidbody rb;

    Vector3 aimPos;
    Vector3 aimDir;
    Transform aim;
    Transform soulGenerator;

    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject aimPivot;

    //generate soul
    [SerializeField] GameObject[] soulTemp;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    //recall
    float recallTimer = 0;
    GameObject[] souls;

    //rolling
    enum CombateState{
        normal,
        rolling,
        recalling,
    }
    CombateState combateState;
    Vector3 lastMoveDir;
    float presentRollingSpeed;
    [SerializeField] float rollingSpeed = 200f;
    [SerializeField] float rollingResistance = 600f;
    [SerializeField] float delayBeforeInvincible = 0.1f;
    [SerializeField] float invincibleDuration = 0.4f;
    int idForSouls = 0;

    //soul list
    bool IsfacingRight = true;
    SoulList soulList;

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
        soulGenerator = aimPivot.transform.Find("SoulGenerator");
        soulList = GetComponent<SoulList>();
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        hp = GetComponent<Health>();
        hp.ShowHPUI();// For testing!!!!!!!!!!!!!!!!!!!!!!!!!

        playerState = PlayerState.combat;
        combateState = CombateState.normal;

        presentRollingSpeed = rollingSpeed;

    }

    void Update()
    {
        //*****************ControlPanel

        // shooting and rolling
        switch (playerState)
        {
            case PlayerState.normal:
                if (aim.gameObject.activeSelf){
                    aim.gameObject.SetActive(false);
                }
                soulList.HoverSoulItem();
                soulList.ClickSoulTiem();

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
                    ShootSoul();
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
                break;
        }
        // if player talk to anyone, force player to become normal state
        if (gameManager.fungusFlowchart.HasExecutingBlocks()){
            playerState = PlayerState.normal;
            // hide hp bar
            hp.HideHPUI();
        }
        else{
            //switch combat state
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SwitchPlayerState();
            }
        }


        //recall control
        if (Input.GetMouseButtonDown(1) && combateState == CombateState.normal)
        {
            CheckAllActivedSouls();
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
                // recover CD;
                if (actionTimer <= actionColdDown){
                    actionTimer += Time.fixedDeltaTime;
                }
                break;
            case CombateState.rolling:
                RollFunction();
                break;
            case CombateState.recalling:
                MoveFunction(0.3f);
                RecallFunction();
                break;
        }
    }


    //****************************Method****************************
    //*********************Moving Function
    // Use speedMultiplyer to change speed.
    void MoveFunction(float speedMultiplyer)
    {
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

        Ray ray = new Ray(transform.position, move);
        float dodgeRayDistance = 1.5f;
        if (!Physics.Raycast(ray, out var hitInfo, dodgeRayDistance, groundMask)){
            GetComponent<Collider>().enabled = false;
            Invoke("RegainColider", invincibleDuration);
        }

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
    void RegainColider(){
        GetComponent<Collider>().enabled= true;
    }

    //************************Aiming Function
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
        aimDir = aimDir.normalized;
        float angle = Mathf.Atan2(aimDir.z, aimDir.x) * Mathf.Rad2Deg;
        aimPivot.transform.eulerAngles = new Vector3(0, -angle, 0);
        // aim won't rotate
        aim.eulerAngles = new Vector3(0, 0, 0);
    }

    // generate soul and shoot it towards mouse
    void ShootSoul()
    {
        if (soulList.soulNum> 0) {

            // remove the soulItem and know what kind of soul it is
            int soulType = soulList.UseSoul();
            // choose soulTemp;
            if (soulType <0){
                Debug.Log("Don't have soul but you shoot one");
            }else
            {
                GameObject soul = Instantiate(soulTemp[soulType], soulGenerator.position, Quaternion.Euler(Vector3.zero));
                soul.GetComponent<SoulManager>().ShootSoul(aimDir);
            }
        }

    }

    //***************************Recalling Function

    void CheckAllActivedSouls()
    {
        souls = GameObject.FindGameObjectsWithTag("SoulNormal");
    }
    private void RecallFunction(){
        // start animation
        characterAnimator.SetBool("IsRecalling", true);
        
        recallTimer += Time.fixedDeltaTime;

        float holdtime = 0.2f;
        //enough time
        if (recallTimer >= holdtime)
        {
            if (souls.Length != 0){

                //Debug.Log("leghth:" + souls.Length + "ID" + idForSouls);
                if (souls[idForSouls]!= null) souls[idForSouls].GetComponent<SoulManager>().RecallFunction();

                //every 0.1s recall one more soul
                if (recallTimer - holdtime > 0.1f)
                {
                    if (idForSouls < souls.Length - 1) idForSouls++;
                    recallTimer = holdtime;
                }
            }
        }
    }
    //reset recall 
    private void EndRecallFunction(){
        // End animation
        characterAnimator.SetBool("IsRecalling", false);

        recallTimer = 0;
        idForSouls = 0;
        combateState = CombateState.normal;

        if (souls.Length != 0){
            for (int i = 0; i < souls.Length; i++){
                if (souls[i] != null){
                    souls[i].GetComponent<SoulManager>().ResetRecall();
                }
            } 
        }

    }

    //*******************************CombateState
    void SwitchPlayerState() {
        if (playerState == PlayerState.combat){
            playerState = PlayerState.normal;
            // hide hp bar
            hp.HideHPUI();
            //change cursor
            CursorManager.instance.ActivateDefaultCursor();
        }
        else if (playerState == PlayerState.normal){
            playerState = PlayerState.combat;
            // show hp bar
            hp.ShowHPUI();
            // clean the hovering items of SoulList
            soulList.CleanHoverItem();
            //change cursor
            CursorManager.instance.ActivateCombatCursor();
        }
    }

    //*******************************List Method
    //Flip the character
    void FlipPlayer()
    {
        if (move.x < 0 && IsfacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = true;
            IsfacingRight = !IsfacingRight;
            //flip SoulList
            soulList.FlipSoulList();
        }
        if (move.x > 0 && !IsfacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = false;
            IsfacingRight = !IsfacingRight;
            //Flip SoulList
            soulList.FlipSoulList();
        }
    }

    public void AddSoulList(int SoulType)
    {
        soulList.AddSoul(SoulType);
    }

    // ************************Combat 
    public void PlayerTakeDamage(float damage)
    {
        hp.TakeDamage(damage);
        hp.Invincible(0f, invincibleDuration);

        if (hp.presentHealth <= 0){
            Debug.Log("You died");
        }
    }

}
