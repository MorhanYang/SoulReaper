using Fungus;
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
    bool recallHandler = true;

    //rolling
    enum CombateState{
        normal,
        rolling,
        recalling,
        superdashing,
    }
    CombateState combateState;
    Vector3 lastMoveDir;
    float presentRollingSpeed;
    int idForSouls = 0;
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

    //Dash
    bool isSuperDashing = false;
    Vector3 dashTarget = Vector3.zero;


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
            if (Input.GetKeyDown(KeyCode.F))
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

        //Shuffle Souls
        if (Input.GetAxis("Mouse ScrollWheel") <= -0.1f || Input.GetKeyDown(KeyCode.LeftShift))
        {
            soulList.ShuffleSouls();
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
                MoveFunction(0.5f);
                RecallFunction();
                SuperDashMove();
                break;
        }
    }


    //****************************Method****************************
    //*********************Moving Function
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


        Physics.IgnoreLayerCollision(2, 9);
        Invoke("RegainColider", invincibleDuration);

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
        Physics.IgnoreLayerCollision(2, 9, false);
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
        aimDir= aimDir.normalized;
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
                Vector3 shootDir = new Vector3(aimDir.x, transform.position.y, aimDir.z).normalized;
                soul.GetComponent<SoulManager>().ShootSoul(shootDir);
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
            if (souls.Length != 0 && idForSouls <= souls.Length - 1)
            {
                if (recallHandler){
                    //Debug.Log("leghth:" + souls.Length + "ID" + idForSouls);
                    if (souls[idForSouls] != null) souls[idForSouls].GetComponent<SoulManager>().RecallFunction();
                    recallHandler= false;
                }
                
                //every 0.1s recall one more soul
                if (recallTimer - holdtime > 0.1f)
                {
                    idForSouls++;
                    recallHandler = true;
                }
            }
        }
    }
    //reset recall 
    private void EndRecallFunction(){
        // End animation
        characterAnimator.SetBool("IsRecalling", false);

        recallHandler = true;
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
            // clean the hovering items of SoulList
            soulList.CleanHoverItem();
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
        //aviod enemy's collision
        Physics.IgnoreLayerCollision(2, 9);
        Invoke("RegainColider", invincibleDuration);

        if (hp.presentHealth <= 0){
            Debug.Log("You died");
        }
    }

    public void Teleport(Vector3 nextPos){

        Ray ray = new Ray(transform.position, nextPos - transform.position);
        float Distance = Vector3.Distance(transform.position,nextPos);
        if (Physics.Raycast(ray, out var hitInfo, Distance, groundMask)){
            nextPos = hitInfo.point; 
        }

        transform.position = nextPos;

    }
    // *******************************SuperDash
    public void SuperDash(Vector3 nextPos, float damage)
    {
        Ray dashRay = new Ray(transform.position, (nextPos - transform.position).normalized);

        //detect the target
        float dashDistance = Vector3.Distance(transform.position, nextPos);
        if (Physics.Raycast(dashRay, out var hitInfo, dashDistance, groundMask)){
            nextPos = hitInfo.point;
        }

        // generate a collider box and detect object that touches it
        Vector3 Dir = (nextPos - transform.position).normalized;
        float distance = Vector3.Distance(nextPos, transform.position);
        Vector3 boxCenter = Dir * (distance / 2) + transform.position;
        Collider[] EnemiesInLine = Physics.OverlapBox(boxCenter, new Vector3(0.25f, 0.4f, 1.1f), Quaternion.Euler(Dir), LayerMask.GetMask("Enemy"));

        for (int i = 0; i < EnemiesInLine.Length; i++){
            if (EnemiesInLine[i].transform.GetComponent<Enemy>() != null){
                Debug.Log("Hit enemies when dashing");
                EnemiesInLine[i].transform.GetComponent<Enemy>().TakeDamage(damage);
            }
        }

        Physics.IgnoreLayerCollision(2, 9);
        dashTarget = nextPos;
        isSuperDashing= true;
    }

    public void SuperDashMove()
    {
        if (isSuperDashing){
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, 10f * Time.fixedDeltaTime);
            if (transform.position == dashTarget)
            {
                Invoke("RegainColider", 0.4f);
                isSuperDashing = false;
            }
        }
    }
}
