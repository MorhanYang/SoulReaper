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
    [SerializeField] float soulSpeed;

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

        playerState = PlayerState.normal;
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
            combateState = CombateState.recalling;
        }
        if (Input.GetMouseButtonUp(0))
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
        move = new Vector3(x,0,z);
        rb.velocity = move * moveSpeed * speedMultiplyer * Time.fixedDeltaTime;

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
            switch (soulType){
                case -1:
                    Debug.Log("Don't have soul but you shoot one");
                    break;

                // normal soul
                case 0:
                    GameObject soul0 = Instantiate(soulTemp[0], soulGenerator.position, Quaternion.Euler(Vector3.zero));
                    soul0.GetComponent<Soul>().ShootSoul(aimDir, soulSpeed);
                    break;

                // special soul
                case 1:
                    GameObject soul1 = Instantiate(soulTemp[1], soulGenerator.position, Quaternion.Euler(Vector3.zero));
                    soul1.GetComponent<Soul>().ShootSoul(aimDir, soulSpeed);
                    break;
                // don't know what happended
                default:
                    break;
            }
  
        }

    }

    //***************************Recalling Function
    private void RecallFunction()
    {

        // start animation
        characterAnimator.SetBool("IsRecalling", true);
        
        recallTimer += Time.fixedDeltaTime;

        float holdtime = 0.5f;
        //enough time
        if (recallTimer >= holdtime)
        {
            // End animation
            characterAnimator.SetBool("IsRecalling", false);

            recallTimer = 0;

            //execute recall function
            souls = GameObject.FindGameObjectsWithTag("SoulNormal");
            for (int i = 0; i < souls.Length; i++)
            {
                souls[i].GetComponent<Soul>().RecallFunction();
            }

            combateState = CombateState.normal;

            
        }

    }
    //reset recall 
    private void EndRecallFunction(){
        // End animation
        characterAnimator.SetBool("IsRecalling", false);

        recallTimer = 0;
        if (souls != null){
            for (int i = 0; i < souls.Length; i++){
                if (souls[i] != null){
                    souls[i].GetComponent<Soul>().ResetRecall();
                }
            }

            combateState = CombateState.normal;
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


}
