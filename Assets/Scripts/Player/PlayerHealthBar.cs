using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]GameManager gameManager;

    //items
    [SerializeField] Item_PlayerHealth itemHPTemp;
    [SerializeField] MinionTroop itemMinionHPTemp;
    [SerializeField] int initalCellNum = 3;
    [HideInInspector]public int cellNum = 3;

    // troop
    List<MinionTroop> activedTroopList = new List<MinionTroop>();
    MinionTroop troopPresent = null;
    [SerializeField] int maxTroopCapacity = 5;

    // hp
    [SerializeField] GameObject hpUI;
    List<Image> hpBarsList = new List<Image>();
    [SerializeField] Image playerInitialHealthBar;
    public float extraHealthMax = 100;
    public float presentExtraHealth = 100;

    //knock back
    Shaker shacker;

    // invincible
    float invincibleTimer = 0;
    bool isInvicible = false; // reset the collision state

    // HealthBar Switch
    Image barPresent;
    int barPresentId;
    float indiviualMaxValue;
    float HpInPresentBar;

    // healing & recovering
    float healingTimer = 0f;
    [SerializeField] float healingValue = 5f;
    [SerializeField] float healingInterval = 10f;
    bool isActiveRecover = false;
    float recoverTime = 0;

    //recall
    Transform MarkedSubject;

    // Effect
    float rebirthDelay = 0.6f;

    // sound
    SoundManager mySoundManager;


    private void Awake()
    {
        cellNum = initalCellNum;
        InitiatePlayerHPBar();
    }

    private void Start()
    {
        shacker = GetComponent<Shaker>();
        mySoundManager = SoundManager.Instance;

        indiviualMaxValue = 20;
        barPresent = hpBarsList[cellNum - 1];

        HealthHPReset();
    }

    private void Update()
    {
        //Timer to prevent players getting multiple Damage;
        if (isInvicible)
        {
            if (invincibleTimer > 0) {
                invincibleTimer -= Time.deltaTime;
            }
            else
            {
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
                isInvicible= false;
            }
        }

        // recover through time
        if (isActiveRecover){
            if (recoverTime <= 12 ){
                HealingAfterSeconds(healingValue, healingInterval);
                recoverTime += Time.deltaTime;
            }else isActiveRecover= false;
        }

        // test
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddHealthMax();
        }
    }
    //****************************************************Initialize & property*************************************************************
    public void InitiatePlayerHPBar()
    {
        itemHPTemp.gameObject.SetActive(true);

        // clean previous cell
        if (hpBarsList.Count > 0)
        {
            for (int i = 0; i < hpBarsList.Count; i++){
                Destroy(hpBarsList[i].transform.parent.parent.gameObject);// destory the hp slot instead of hp bar.
            }
            hpBarsList.Clear();
        }

        //generate new cells
        for (int i = 0; i < cellNum; i++)
        {
            Item_PlayerHealth hpCell = Instantiate(itemHPTemp, hpUI.transform);
            hpBarsList.Add(hpCell.playerHPUI);
        }

        itemHPTemp.gameObject.SetActive(false);
    }
    public List<MinionTroop> GetActivedTroop(){
        return activedTroopList;
    }
    public void CleanTroopList()
    {
        activedTroopList.Clear();
        troopPresent = null;
    }

    // save & load
    public void SetPlayerHealth(int hpCell, float hpMax, float hp)
    {
        cellNum = hpCell;
        extraHealthMax= hpMax;
        presentExtraHealth = hp;
        HealthHPReset();
    }

    //*********************************************************** HP Display Refresh *****************************************************
    void HealthHPReset()
    {
        // clean all bars
        for (int i = 0; i < hpBarsList.Count; i++){
            hpBarsList[i].fillAmount = 0f;
        }

        // count full bars
        int fullBarNum = 0;
        fullBarNum = (int)((presentExtraHealth - 0.1f) / indiviualMaxValue);
        if (fullBarNum < 0) fullBarNum = 0;

        // set full bars
        for (int i = 0; i < fullBarNum; i++)
        {
            hpBarsList[i].fillAmount = 1f;
        }

        // set the half bar
        HpInPresentBar = presentExtraHealth - (indiviualMaxValue * fullBarNum);
        if (HpInPresentBar > 0)
        {
            hpBarsList[fullBarNum].fillAmount = HpInPresentBar/indiviualMaxValue;
        }

        // property
        barPresentId = fullBarNum;
        barPresent = hpBarsList[fullBarNum];

    }

    //*********************************************************** Damage **********************************************************
    public void TakeDamage(float damage, Transform damageDealer)
    {
        if (invincibleTimer <= 0)
        {
            // check if the player is using initial player health bar
            if (presentExtraHealth == 0){
                PlayerInitalHealthBarGetdamage();
                return;
            }

            // real damage
            if (damage < HpInPresentBar){
                presentExtraHealth -= damage;
                HpInPresentBar -= damage;
                barPresent.fillAmount = HpInPresentBar/indiviualMaxValue;

                // knock back
                if(damageDealer != null) shacker.AddImpact((transform.position - damageDealer.position), damage, false);

                // become invincible
                Invincible(0.2f);
            }
            // reduce a cell of bar
            else{

                if (barPresentId <= 0)// last extra health bar
                {
                    //display
                    HpInPresentBar= 0;
                    barPresent.fillAmount = 0;

                    presentExtraHealth = 0;
                }
                else // not last health bar
                {
                    presentExtraHealth -= indiviualMaxValue;
                    barPresent.fillAmount = 0;
                    float passedDamage = damage - indiviualMaxValue;
                    // next bar
                    barPresentId--;
                    barPresent = hpBarsList[barPresentId];
                    TakeDamage(passedDamage, null);
                }
            }
        }
    }

    void PlayerInitalHealthBarGetdamage()
    {
        gameManager.LoadPlayerData();
    }

    //************************************************* Recover *****************************************
    public void ActivateRecover()
    {
        isActiveRecover= true;
        recoverTime = 0;
    }

    void HealingAfterSeconds(float healingValue, float interval){
        if (healingTimer > interval){
            Healing(healingValue);
            healingTimer = 0;
        }
        else healingTimer += Time.deltaTime;
    }

    void Healing(float healingValue)
    {
        if (healingValue + presentExtraHealth > extraHealthMax)
        {
            // get extra health
            float healthLeft = healingValue + presentExtraHealth - extraHealthMax;
            // player healing value
            healingValue = extraHealthMax - presentExtraHealth;

            // check troops' health
            for (int i = 0; i < activedTroopList.Count; i++){
                float remainHealthOfTroop = activedTroopList[i].HealTroop(healthLeft);
                healthLeft = remainHealthOfTroop;
            } 
        }

        float BarSpaceLeft = indiviualMaxValue - HpInPresentBar;
        if (healingValue <= BarSpaceLeft){
            presentExtraHealth += healingValue;
            HpInPresentBar += healingValue;
            if(barPresent!= null) barPresent.fillAmount = HpInPresentBar / indiviualMaxValue;
        }
        else{
            presentExtraHealth += BarSpaceLeft;
            if(barPresent != null) barPresent.fillAmount = 1f;
            float passedHP = healingValue - BarSpaceLeft;
            // load prevous bar
            if (barPresentId < (cellNum - activedTroopList.Count - 1)) barPresentId++;
            HpInPresentBar = 0;
            barPresent = hpBarsList[barPresentId];

            Healing(passedHP);
        }
    }

    public void AddHealthMax()
    {
        cellNum++;
        presentExtraHealth += indiviualMaxValue;
        extraHealthMax += indiviualMaxValue;

        // generate new health bar unit
        Item_PlayerHealth myItem = Instantiate(itemHPTemp, hpUI.transform);
        myItem.gameObject.SetActive(true);
        myItem.GetComponent<CanvasGroup>().alpha = 0f;
        // change new item hierarchy to 0
        myItem.transform.SetSiblingIndex(0);

        // display
        myItem.GetComponent<CanvasGroup>().DOFade(1, 1f);

        // adjust present bar
        hpBarsList.Insert(0,myItem.playerHPUI);
        barPresentId++;
        barPresent = barPresent = hpBarsList[barPresentId];


        HealthHPReset();
    }

    //*************************************************************** recall Or troop die ******************************************
    public void MarkRegainTarget(Transform target)
    {
        MarkedSubject = target;
    }
    public void RegainHP()
    {
        // marked a absorbable object 
        if (MarkedSubject != null && MarkedSubject.GetComponent<Absorbable>() != null){
            RegainAbsorbableHP();
        }
        // marked a troop or don't mark anything
        else if (activedTroopList.Count > 0){
            RegainSelectedTroopHP();
        }
    }
    void RegainSelectedTroopHP()
    {
        MinionTroop TargetTroop = null;

        // player select a troop
        if (MarkedSubject != null)
        {
            TargetTroop = MarkedSubject.GetComponent<Minion>().GetTroop();
            // play recall effect
            TargetTroop.ExecuteMinionRecall();

            RemoveTroopFromPlayerHealth(TargetTroop,true);

            // reset property
            MarkedSubject = null;
        }
    }
    public void RegainAllTroopHP()
    {
        if (activedTroopList.Count > 0 && MarkedSubject == null)
        {
            for (int i = 0; i < activedTroopList.Count; i++)
            {
                activedTroopList[i].ExecuteMinionRecall();
                RemoveTroopFromPlayerHealth(activedTroopList[i],true);
            }
        }
    }
    public void RemoveTroopFromPlayerHealth(MinionTroop troop, bool normalRemove) {

        // set HP
        if (normalRemove)
        {
            presentExtraHealth += troop.GetPresentHP();
            extraHealthMax += indiviualMaxValue;
        }

        //remove 
        activedTroopList.Remove(troop);
        Destroy(troop.gameObject, 0.53f);

        // genrate new item 
        if (normalRemove)
        {
            Item_PlayerHealth myItem = Instantiate(itemHPTemp, hpUI.transform);
            myItem.gameObject.SetActive(true);
            myItem.GetComponent<CanvasGroup>().alpha = 0f;
            // change new item hierarchy to 0
            myItem.transform.SetSiblingIndex(0);

            // display
            Sequence mysequence = DOTween.Sequence();
            mysequence.Join(troop.GetComponent<RectTransform>().DOLocalMoveY(-70f, 0.3f))
                .Join(troop.GetComponent<CanvasGroup>().DOFade(0, 0.3f))
                .Join(myItem.GetComponent<CanvasGroup>().DOFade(1, 1f));

            // add it to list
            hpBarsList.Insert(0, myItem.playerHPUI);
        }

        //reset parameter
        HealthHPReset();
    }
    void RegainAbsorbableHP()
    {
        Absorbable myAbsorbabl = MarkedSubject.GetComponent<Absorbable>();

        // check troop hp state
        bool troopNeedhealth = false;
        for (int i = 0; i < activedTroopList.Count; i++){
            if (activedTroopList[i].GetPresentHP() < activedTroopList[i].GetMaxHP()){
                troopNeedhealth= true;
                break;
            }
        }

        // if health is not full
        if (presentExtraHealth < extraHealthMax || myAbsorbabl.addHealthMax || troopNeedhealth)
        {
            float healValue;
            healValue = myAbsorbabl.TakeLife();
            // recovering
            // normal
            if (healValue > 0) Healing(healValue);
            // add healthMax
            if (healValue < 0){
                AddHealthMax();
                // play sound
                mySoundManager.PlaySoundAt(transform.position, "Upgrade", false, false, 4f, 1f, 100, 100);
            }
        }
        else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Health is full");
    }
    //*************************************************************** Rebirth Troop ***************************************************
    public void ReviveTroopNormal( Vector3 pointedPos , float radius)
    {
        // check if present troop have spacea and have a troop
        bool troopHaveSpace;
        if (troopPresent != null && troopPresent.GetTroopEmptySpace() > 0) 
            troopHaveSpace = true; 
        else troopHaveSpace = false;

        // have health to rebirth troop
        if (presentExtraHealth > 0 || troopHaveSpace)
        {
            Collider[] MinionInCircle = Physics.OverlapSphere(pointedPos, radius, LayerMask.GetMask("Minion"));// when rebirth minion, the layer will change

            if (MinionInCircle.Length > 0)
            {
                // play sound
                mySoundManager.PlaySoundAt(mySoundManager.transform.position, "Release", false, false, 1.5f, 1, 100, 100);

                // get minion list
                List<Minion> minionSet = new List<Minion>();
                for (int i = 0; i < MinionInCircle.Length; i++)
                {
                    if (MinionInCircle[i] != null)
                    {
                        if (MinionInCircle[i].GetComponent<Minion>().minionType == 1)// Special minion
                        {
                            // only add special minion
                            if (presentExtraHealth > 0){
                                // clean the list and stop loop
                                minionSet.Clear();
                                minionSet.Add(MinionInCircle[i].GetComponent<Minion>());

                                break;
                            }
                            else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Not enough Health");
                        }
                        else if (MinionInCircle[i].GetComponent<Minion>().minionType == 0){
                            minionSet.Add(MinionInCircle[i].GetComponent<Minion>());

                            continue;
                        }
                        else if (MinionInCircle[i].GetComponent<Minion>().minionType == 2 && MinionInCircle.Length <= 1){
                            // only add special minion
                            if (presentExtraHealth > 0) {
                                // clean the list and stop loop
                                minionSet.Clear();
                                minionSet.Add(MinionInCircle[i].GetComponent<Minion>());
                                break;
                            }
                            else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Not enough Health");
                        }
                    }
                }

                // revive minion
                ReviveTroopFunction(minionSet);
            }

        }
        else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Not enough Health");        
    }

    void ReviveTroopFunction(List<Minion> minionSet)
    {
        // reorder minions from highest to lowest
        //minionSet.Sort(SortByMinionSize);
        //minionSet.Reverse();

        if (minionSet.Count > 0)
        {
            // ********special minion
            if (minionSet[0].minionSize == maxTroopCapacity && presentExtraHealth >0 )
            {
                MinionTroop mytroop = troopPresent;
                GenerateNewTroop();

                // revieve minion
                if (minionSet[0].GetComponent<Minion>().SetActiveDelay(rebirthDelay))
                {
                    //add to troop list
                    Debug.Log("maxTroopCapacity" + maxTroopCapacity);
                    troopPresent.AddTroopMember(minionSet[0].GetComponent<Minion>());
                }
                // reset the present troop
                troopPresent = mytroop;// prevent troop present change trooppresent for small minion;

                // remove item
                minionSet.RemoveAt(0);
                return;
            }

            //*******Normal minion
            else
            {
                // if present troop is full
                if (troopPresent == null || troopPresent.GetTroopEmptySpace() <= 0)
                {
                    GenerateNewTroop();
                }

                // fill the troop
                if (troopPresent.GetTroopEmptySpace() >= minionSet.Count) // enough slot for minion
                {
                    // call all minions out
                    for (int i = 0; i < minionSet.Count; i++)
                    {
                        minionSet[i].SetActiveDelay(rebirthDelay);
                        //add to troop list
                        troopPresent.AddTroopMember(minionSet[i]);

                    }
                }
                else if (troopPresent.GetTroopEmptySpace() < minionSet.Count) // too much minions
                {
                    int leftSlots = troopPresent.GetTroopEmptySpace();
                    for (int i = 0; i < leftSlots; i++)
                    {
                        minionSet[i].SetActiveDelay(rebirthDelay);
                        //add to troop list
                        troopPresent.AddTroopMember(minionSet[i]);
                    }
                }

            }
        }
    }

    public void ReviveTroopLoading(List<Minion> reviveMinions)
    {
        ReviveTroopFunction(reviveMinions);
    }

    void GenerateNewTroop()
    {
        float troopHP;
        // update data
        if (presentExtraHealth < indiviualMaxValue){
            troopHP = presentExtraHealth;
            presentExtraHealth = 0;
            HpInPresentBar = 0;
        }
        else{
            presentExtraHealth -= indiviualMaxValue;
            troopHP = indiviualMaxValue;
            
        }
        extraHealthMax -= indiviualMaxValue;

        //Destory First HP 
        Transform hpBarRemoving = hpBarsList[0].transform.parent.parent;
        Destroy(hpBarRemoving.gameObject, 0.53f);// delay for display
        hpBarsList.RemoveAt(0);

        // Generate Troop Bar
        troopPresent = Instantiate(itemMinionHPTemp, hpUI.transform);
        troopPresent.GetComponent<CanvasGroup>().alpha = 0f;
        troopPresent.gameObject.SetActive(true);
        troopPresent.GetComponent<MinionTroop>().ResetTroopHP(troopHP, indiviualMaxValue, maxTroopCapacity);
        if (barPresentId > 0) { 
            barPresentId--;
            barPresent = hpBarsList[barPresentId];
        }
        else{
            if (hpBarsList.Count > 0){
                barPresentId = 0;
                barPresent = hpBarsList[barPresentId];
            }
            else{
                barPresentId = -1;
                barPresent = null;
            }

        }

        // display
        Sequence mysequence = DOTween.Sequence();
        mysequence.Join(hpBarRemoving.GetComponent<RectTransform>().DOLocalMoveY(-70f, 0.3f))
            .Join(hpBarRemoving.GetComponent<CanvasGroup>().DOFade(0, 0.5f))
            .Append(troopPresent.GetComponent<CanvasGroup>().DOFade(1f, 1f));

        // add it to count list
        activedTroopList.Add(troopPresent);

    }

    //****************************************************** Invincible ******************************************************
    public void Invincible(float duration)
    {
        // set invincible time;
        invincibleTimer = duration;
        isInvicible = true;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"));

    }
}
