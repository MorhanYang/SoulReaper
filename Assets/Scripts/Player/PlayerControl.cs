using Fungus;
using System;
using System.Collections.Generic;
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
    [SerializeField] MinionTroop[] troopList;
    float shootCount = 0;

    //switch Troop list
    int troopId = 0;

    //Movement
    [SerializeField] float moveSpeed;
    [SerializeField] float actionColdDown = 0.5f;
    float actionTimer = 0;

    //recall
    float recallTimer = 0;
    [SerializeField]float recallDistance = 4f;

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
    SoulList soulList;
    List<Minion> minionsInGame;

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

        minionsInGame = new List<Minion>();

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
                    combateState = CombateState.shooting;
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
                    RebirthEnemy();
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
            combateState = CombateState.recalling;
        }
        if (Input.GetMouseButtonUp(1))
        {
            EndRecallFunction();
        }

        // switch Troop
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            SwitchTroop(Input.GetAxis("Mouse ScrollWheel"));
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SwitchTroop(1);// any number greater than 0
        }

        // heal TroopHP;
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Heal Troop");
            foreach (MinionTroop item in troopList)
            {
                item.HealTroops();
            }
            hp.TakeDamage(10);
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
                AssignSouls();
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


        //Physics.IgnoreLayerCollision(2, 9);
        //Invoke("RegainColider", invincibleDuration);

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
    //void RegainColider(){
    //    Physics.IgnoreLayerCollision(2, 9, false);
    //}

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
    void AssignSouls()
    {
        if (troopList[troopId].GetMinionNumber() > 0)
        {
            rb.velocity = Vector3.zero;

            // find target
            Transform target;
            Vector3 sprintPos;
            Collider[] hitedEnemy = Physics.OverlapSphere(aimPos, 1f, LayerMask.GetMask("Enemy"));

            if (hitedEnemy.Length <= 0)
            {
                sprintPos = aimPos;
                target = null;
            }
            else
            {
                sprintPos = aimPos;
                // other will kill target
                if (hitedEnemy != null)
                {
                    target = GetClosestEnemyTransform(hitedEnemy, aimPos);
                }
                else target = null;
                
            }



            // generate
            if (shootCount >= 0.2f)
            {
                // detract minion from troop
                GameObject GeneratedMinion = troopList[troopId].GenerateMinion(soulGenerator.position);
                Debug.Log(GeneratedMinion);

                GeneratedMinion.GetComponent<MinionAI>().SpriteToEnemy(sprintPos, target);
                if (minionsInGame == null) minionsInGame = new List<Minion>();
                minionsInGame.Add(GeneratedMinion.GetComponent<Minion>());

                //set listid in Minion
                GeneratedMinion.GetComponent<Minion>().SetListId(troopList[troopId]);

                shootCount = 0;
            }
            else shootCount += Time.fixedDeltaTime;
        }
        else Debug.Log("No Minion left");
    }

     private Transform GetClosestEnemyTransform(Collider[] enemyList, Vector3 referencePoint)
    {
        Transform closedEnemy = null;

        for (int i = 0; i < enemyList.Length; i++)
        {
            if (!enemyList[i].GetComponent<Enemy>().isDead)
            {
                Collider testEnemy = enemyList[i];
                if (closedEnemy == null)
                {
                    closedEnemy = testEnemy.transform;
                }
                // test which is closer
                else
                {
                    if (Vector3.Distance(testEnemy.transform.position, referencePoint) > Vector3.Distance(closedEnemy.transform.position, referencePoint))
                    {
                        closedEnemy = testEnemy.transform;
                    }

                }
            }
            
        }
        return closedEnemy;
    }

    //********************************************************************Recalling Function****************************************************************

    private void RecallFunction(){


        // start animation
        characterAnimator.SetBool("IsRecalling", true);
        
        recallTimer += Time.fixedDeltaTime;

        float holdtime = 0.1f;
        //enough time
        if (recallTimer >= holdtime)
        {
            if (minionsInGame.Count > 0)
            {
                if (minionsInGame[0] != null && Vector3.Distance(minionsInGame[0].transform.position,transform.position) <= recallDistance)
                {
                    // if minions excess maxtroop capacity, it will not be recalled.
                    if (troopList[troopId].AddTroopMember(minionsInGame[0])){
                        minionsInGame[0].RecallMinion();
                    }
                }
                minionsInGame.RemoveAt(0);
                recallTimer = 0;
            }
        }
    }
    //reset recall 
    private void EndRecallFunction(){
        // End animation
        characterAnimator.SetBool("IsRecalling", false);

        recallTimer = 0;
        combateState = CombateState.normal;
    }

    //********************************************************************** Shift Troop ******************************************************
    void SwitchTroop(float switchDir) //switchDir > 0: move forward; switchDir < 0 : move backward;
    {
        int lastId = troopList.Length - 1; // id start from 0;

        if (switchDir > 0)
        {
            troopId++;
            if (troopId > lastId) troopId = 0;
        }
        if (switchDir < 0)
        {
            troopId--;
            if (troopId < 0) troopId = lastId;
        }

        FreshMinionInGameList();
        // adopt minions to list
        Debug.Log("TroopId:" + troopId);

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
        if (move.x < 0 && isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = true;
            isFacingRight = !isFacingRight;
            //flip SoulList
            soulList.FlipSoulList();
        }
        if (move.x > 0 && !isFacingRight)
        {
            transform.Find("Character").GetComponent<SpriteRenderer>().flipX = false;
            isFacingRight = !isFacingRight;
            //Flip SoulList
            soulList.FlipSoulList();
        }
    }

    public void AddSoulList(int SoulType)
    {
        soulList.AddSoul(SoulType);
    }

    // *********************************************************************Combat *********************************************
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

    // dead minion
    public void RemoveMinionFromList(Minion minion)
    {
        minionsInGame.Remove(minion);
    }


    //public void Teleport(Vector3 nextPos){

    //    Ray ray = new Ray(transform.position, nextPos - transform.position);
    //    float Distance = Vector3.Distance(transform.position,nextPos);
    //    if (Physics.Raycast(ray, out var hitInfo, Distance, groundMask)){
    //        nextPos = hitInfo.point; 
    //    }

    //    transform.position = nextPos;

    //}

    //****************************************************************Rebirth**********************************************************

    void RebirthEnemy()
    {
        // call minion out
        Collider[] DeadEnemyInCircle = Physics.OverlapSphere(transform.position, 4f, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < DeadEnemyInCircle.Length; i++)
        {
            Enemy DeadEnemy = DeadEnemyInCircle[i].GetComponent<Enemy>();
            if (DeadEnemy.isDead == true){
                DeadEnemy.Rebirth();
            }
        }

        // adopt minions to list
        FreshMinionInGameList();
    }

    void FreshMinionInGameList() {

        
        minionsInGame.Clear();

        GameObject[] allMinion = GameObject.FindGameObjectsWithTag("Minion");
        foreach (var item in allMinion)
        {
            minionsInGame.Add(item.GetComponent<Minion>());
        }

    }

    // *******************************SuperDash
    //public void SuperDash(Vector3 nextPos, float damage)
    //{
    //    Ray dashRay = new Ray(transform.position, (nextPos - transform.position).normalized);

    //    //detect the target
    //    float dashDistance = Vector3.Distance(transform.position, nextPos);
    //    if (Physics.Raycast(dashRay, out var hitInfo, dashDistance, groundMask)){
    //        nextPos = hitInfo.point;
    //    }

    //    // generate a collider box and detect object that touches it
    //    Vector3 Dir = (nextPos - transform.position).normalized;
    //    float distance = Vector3.Distance(nextPos, transform.position);
    //    Vector3 boxCenter = Dir * (distance / 2) + transform.position;
    //    Collider[] EnemiesInLine = Physics.OverlapBox(boxCenter, new Vector3(0.25f, 0.4f, 1.1f), Quaternion.Euler(Dir), LayerMask.GetMask("Enemy"));

    //    for (int i = 0; i < EnemiesInLine.Length; i++){
    //        if (EnemiesInLine[i].transform.GetComponent<Enemy>() != null){
    //            Debug.Log("Hit enemies when dashing");
    //            EnemiesInLine[i].transform.GetComponent<Enemy>().TakeDamage(damage,gameObject);
    //        }
    //    }

    //    Physics.IgnoreLayerCollision(2, 9);
    //    dashTarget = nextPos;
    //    isSuperDashing= true;
    //}

    //public void SuperDashMove()
    //{
    //    if (isSuperDashing){
    //        transform.position = Vector3.MoveTowards(transform.position, dashTarget, 10f * Time.fixedDeltaTime);
    //        if (transform.position == dashTarget)
    //        {
    //            Invoke("RegainColider", 0.4f);
    //            isSuperDashing = false;
    //        }
    //    }
    //}
}
