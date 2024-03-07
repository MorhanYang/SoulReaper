using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
    PlayerHealth playerHealth;
    TroopManager troopManager;
    [SerializeField] GameManager gameManager;

    Vector3 move;
    Rigidbody rb;
    bool isFacingRight = true;

    Vector3 aimPos;

    [SerializeField] Transform[] soulGenerator;
    [SerializeField] Transform Character;
    [SerializeField] LayerMask groundMask;

    // Attack
    [SerializeField] Transform attackFlipAix;
    [SerializeField] float myDamage = 5;
    [SerializeField] float attackCD = 1f;
    Transform attackPoint;
    float attackTimer;
    [SerializeField] GameObject upAttackEffect;
    [SerializeField] GameObject downAttackEffect;
    DamageManager myDamageManager;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    // Mouse control
    [SerializeField] GameObject mouseMenu;
    [SerializeField] Canvas followMouseCanvas;
    GameObject GeneratedMenu = null;
    int mouseInputCount = 0;
    float clickTimer = 0;
    bool isShowingMouseMenu = false;

    float holdTime = 0.15f;
    float radius = 2f;
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




    //Animation
    Animator characterAnimator;

    // sound 
    SoundManager mySoundManagers;
    [SerializeField]AudioSource foodstepSound;

    // present Level
    [HideInInspector] public string sceneName;
    [HideInInspector] public int landID;

    // Ability Unlock
    public bool canMove = true;
    public bool canMeleeAttack = true;
    public bool canRightSpecialAction = true;
    public bool canLeftSpecialAction = true;


    void Start()
    {
        rb= GetComponent<Rigidbody>();
        characterAnimator = transform.Find("Character").GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        troopManager = GetComponent<TroopManager>();
        mySoundManagers = SoundManager.Instance;
        myDamageManager = DamageManager.instance;

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

        //Mouse Control
        CheckLeftMouseControl();
        CheckRightMouseControl();

        //// rolling
        //if (actionTimer < actionColdDown){
        //    actionTimer += Time.deltaTime;
        //}

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (actionTimer >= actionColdDown){
        //        // play sound
        //        mySoundManagers.PlaySoundAt(transform.position, "PlayerDash", false, false, 1.5f, 1f, 100, 100);

        //        combateState = CombateState.rolling;
        //        actionTimer = 0;
        //    }
        //}

        // Melee Combat
        if (attackTimer < attackCD) attackTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)){
            if (canMeleeAttack)
            {
                MeleeAttack();
            } 
        }

        // footstep sound
        if (move != Vector3.zero && !foodstepSound.isPlaying && Time.timeScale >= 1) { foodstepSound.Play(); }
        if (move == Vector3.zero && foodstepSound.isPlaying) { foodstepSound.Stop(); }
        if (Time.timeScale < 1) foodstepSound.Stop();
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
                //RollFunction();
                break;
            case CombateState.teleporting:
                break;

        }
    }

    // ******************************************************* Mouse Button Control Function ******************************************************************
    public void CheckLeftMouseControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickTimer = 0;
        }
        // only count timer as it didn't hit a UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                clickTimer += Time.deltaTime;
            }
        }

        // create menu if hold enough time
        if (!isShowingMouseMenu && canLeftSpecialAction)
        {
            // show menu
            if (Input.GetMouseButton(0) && clickTimer >= holdTime)
            {
                // create a menu
                GeneratedMenu = Instantiate(mouseMenu, followMouseCanvas.transform);
                GeneratedMenu.GetComponent<MouseControlUI>().InitializeMouseUI(followMouseCanvas);

                isShowingMouseMenu = true;
            }
        }

        // realse -> check if this is hold
        if (Input.GetMouseButtonUp(0) && clickTimer < holdTime & !EventSystem.current.IsPointerOverGameObject())// it is a click
        {   // assign one minion
            troopManager.AssignOneMinion(aimPos);
        }

        if (GeneratedMenu != null)
        {
            if (Input.GetMouseButtonUp(0) && clickTimer >= holdTime)// it is a hold
            {
                // execute different fucntion depending on type
                MouseControlUI.Action actionType = GeneratedMenu.GetComponent<MouseControlUI>().GetControlUIAction();
                switch (actionType)
                {
                    case MouseControlUI.Action.LeftClickSpecial1:
                        // assign one minion
                        troopManager.AssignOneMinion(aimPos);
                        break;

                    case MouseControlUI.Action.LeftClickSpecial2:
                        //assign all minion
                        Debug.Log("assign all minion");
                        troopManager.AssignAllMinions(aimPos);
                        break;

                    default:
                        break;
                }

                // remove Indicator
                GeneratedMenu.GetComponent<MouseControlUI>().CleanIndicator();
                // delet menue
                if (GeneratedMenu != null) Destroy(GeneratedMenu);
                isShowingMouseMenu = false;
            }
        }
    }

    public void CheckRightMouseControl()
    {
        float holdTime = 0.15f;

        if (Input.GetMouseButtonDown(1))
        {
            clickTimer = 0;
        }
        // only count timer as it didn't hit a UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(1))
            {
                clickTimer += Time.deltaTime;
            }
        }

        // create menu if hold enough time
        if (!isShowingMouseMenu && canRightSpecialAction)
        {
            if (Input.GetMouseButton(1) && clickTimer >= holdTime)
            {
                // create a menu
                GeneratedMenu = Instantiate(mouseMenu, followMouseCanvas.transform);
                GeneratedMenu.GetComponent<MouseControlUI>().InitializeMouseUI(followMouseCanvas);

                isShowingMouseMenu = true;
            }
        }

        // realse -> check if this is hold
        if (Input.GetMouseButtonUp(1) && clickTimer < holdTime && !EventSystem.current.IsPointerOverGameObject())// it is a click
        {
            // eat a minion
            playerHealth.AbsorbOthers();
        }

        if (GeneratedMenu != null)
        {
            if (Input.GetMouseButtonUp(1) && clickTimer >= holdTime)// it is a hold
            {
                // execute different fucntion depending on type
                MouseControlUI.Action actionType = GeneratedMenu.GetComponent<MouseControlUI>().GetControlUIAction();
                switch (actionType)
                {
                    case MouseControlUI.Action.RightClickSpecial1:
                        troopManager.ReviveSingleMinion();
                        break;

                    case MouseControlUI.Action.RightClickSpecial2:
                        //execute function
                        troopManager.ReviveTroopNormal(aimPos, radius);

                        ////check CD
                        //if (gameManager.IsSpellIsReady(3))
                        //{
                        //    //Activate CD UI
                        //    //gameManager.ActivateSpellCDUI(3);
                        //}
                        break;

                    case MouseControlUI.Action.RightClickSpecial3:
                        playerHealth.AbsorbOthers();
                        break;

                    case MouseControlUI.Action.RightClickSpecial4:
                        troopManager.EatTroopToRecover();
                        break;

                    default:
                        break;
                }

                // remove Indicator
                GeneratedMenu.GetComponent<MouseControlUI>().CleanIndicator();
                // delet menue
                if (GeneratedMenu != null) Destroy(GeneratedMenu);
                isShowingMouseMenu = false;
            }
        }
    }


    //********************************************************** Moving Function ***************************************************************************
    // Use speedMultiplyer to change speed.
    void MoveFunction(float speedMultiplyer){
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (canMove){
            move = new Vector3(x, rb.velocity.y, z);
            rb.velocity = new Vector3(x * moveSpeed * speedMultiplyer * Time.fixedDeltaTime,
                                    rb.velocity.y,
                                    z * moveSpeed * speedMultiplyer * Time.fixedDeltaTime);
        }
        else{
            move = Vector3.zero;
        }
        

        FlipPlayer();
        // last direction for rolling
        if (move != Vector3.zero){
            lastMoveDir = move.normalized;
        }

        //animator control
        characterAnimator.SetBool("IsMoving", move != Vector3.zero);
    }
    //void RollFunction() {
    //    //animation
    //    characterAnimator.SetBool("IsRolling", true);

    //    move = Vector3.zero;
    //    rb.velocity = lastMoveDir * presentRollingSpeed * Time.fixedDeltaTime;
    //    // invincible time
    //    playerHealth.Invincible(invincibleDuration);

    //    // slow down
    //    presentRollingSpeed -= rollingResistance * Time.fixedDeltaTime;
    //    if (presentRollingSpeed <= (moveSpeed+ 30f)){

    //        rb.velocity = Vector3.zero;
    //        presentRollingSpeed = rollingSpeed;
            
    //        //Animation
    //        characterAnimator.SetBool("IsRolling", false);

    //        combateState = CombateState.normal;
    //    }
    //}

    //********************************************************************** Aiming Function ****************************************************
    void MouseAimFunction()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            aimPos = hitInfo.point;
        }
    }
    //*************************************************** Flip the character *********************************
    void FlipPlayer()
    {
        if (move.x < 0 && isFacingRight)
        {
            Character.localScale = new Vector3(-Character.localScale.x, Character.localScale.y, Character.localScale.z);
            // Flip Attack Point
            attackFlipAix.Rotate(0f,180f,0f);
            isFacingRight = !isFacingRight;
        }
        if (move.x > 0 && !isFacingRight)
        {
            Character.localScale = new Vector3(-Character.localScale.x, Character.localScale.y, Character.localScale.z);
            // Flip Attack Point
            attackFlipAix.Rotate(0f, 180f, 0f);
            isFacingRight = !isFacingRight;
        }
    }

    // ********************************************************************* Combat *********************************************

    //----------------Temp for Dash
    public void PlayerTakeDamage(float damage, Transform damageDealer)
    {
        playerHealth.TakeDamage(damage, damageDealer,damageDealer.position);
        playerHealth.Invincible(invincibleDuration);
    }

    // melee attack
    void MeleeAttack()
    {
        if (attackTimer >= attackCD)
        {
            if (isFacingRight) Instantiate(upAttackEffect, transform.position + new Vector3(0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(45f, 0, 0)), transform);
            else Instantiate(upAttackEffect, transform.position + new Vector3(-0.3f, 0.35f, 0), Quaternion.Euler(new Vector3(-45f, -180f, 0)), transform);

            myDamageManager.DealSingleDamage(transform, attackPoint.position, null, myDamage);
            attackTimer = 0;

            // sound effect
            mySoundManagers.PlaySoundAt(transform.position, "Swing", false, false, 1.5f, 0.7f, 100, 100);
        }
    }

    // ************************************************************* Teleport ***********************************************
    public void SetPlayerToTeleporting(){
        move = Vector3.zero;
        combateState = CombateState.teleporting;
    }
    public void inActivateTeleporting()
    {
        combateState = CombateState.normal;
    }
}
