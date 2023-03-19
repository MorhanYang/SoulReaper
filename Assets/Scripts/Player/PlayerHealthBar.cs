using DG.Tweening;
using System.Collections.Generic;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthBar : MonoBehaviour
{

    //items
    [SerializeField] Item_PlayerHealth itemHPTemp;
    [SerializeField] MinionTroop itemMinionHPTemp;
    [SerializeField] int cellNum = 5;

    // troop
    List<MinionTroop> activedTroopList = new List<MinionTroop>();
    MinionTroop troopPresent = null;
    [SerializeField] int maxTroopCapacity = 5;

    // hp
    [SerializeField] GameObject hpUI;
    List<RectTransform> hpBarsList = new List<RectTransform>();
    float initialBarWidth;
    public float Maxhealth = 100;
    public float presentHealth = 100;

    //knock back
    Shaker shacker;

    // invincible
    float invincibleTimer = 0;
    bool isInvicible = false; // reset the collision state

    // HealthBar Switch
    RectTransform barPresent;
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


    private void Awake()
    {
        InitiatePlayerHPBar();
    }

    private void Start()
    {
        shacker = GetComponent<Shaker>();
        indiviualMaxValue = Maxhealth / hpBarsList.Count;
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
        
        if (Input.GetKeyDown(KeyCode.T)) {
            TakeDamage(30f,null);
        }

    }
    //****************************************************Initialize & property*************************************************************
    void InitiatePlayerHPBar()
    {
        itemHPTemp.gameObject.SetActive(true);
        initialBarWidth = itemHPTemp.troopHPUI.sizeDelta.x;
        for (int i = 0; i < cellNum; i++)
        {
            Item_PlayerHealth hpCell = Instantiate(itemHPTemp, hpUI.transform);
            hpBarsList.Add(hpCell.troopHPUI);
        }

        itemHPTemp.gameObject.SetActive(false);
    }

    public List<MinionTroop> GetActivedTroop(){
        return activedTroopList;
    }

    Vector3 BarWidthSize(Vector3 previousSize, float Hpinbar){
        return new Vector3((Hpinbar / indiviualMaxValue * initialBarWidth), previousSize.y, previousSize.z);
    }
    //*********************************************************** HP Display Refresh *****************************************************
    void HealthHPReset()
    {
        // clean all bars
        for (int i = 0; i < hpBarsList.Count; i++)
        {
            hpBarsList[i].sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);
        }

        // count full bars
        int fullBarNum = 0;
        fullBarNum = (int)((presentHealth - 0.1f) / indiviualMaxValue);
        if (fullBarNum < 0) fullBarNum = 0;

        // set full bars
        for (int i = 0; i < fullBarNum; i++)
        {
            hpBarsList[i].sizeDelta = BarWidthSize(barPresent.sizeDelta, indiviualMaxValue);
        }

        // set the half bar
        HpInPresentBar = presentHealth - (indiviualMaxValue * fullBarNum);
        if (HpInPresentBar > 0)
        {
            hpBarsList[fullBarNum].sizeDelta = BarWidthSize(barPresent.sizeDelta, HpInPresentBar);
        }

        // property
        barPresent = hpBarsList[fullBarNum];
        barPresentId = fullBarNum;
    }

    //*********************************************************** Damage **********************************************************
    public void TakeDamage(float damage, Transform damageDealer)
    {
        if (invincibleTimer <= 0)
        {
            // real damage
            if (damage < HpInPresentBar)
            {
                presentHealth -= damage;
                HpInPresentBar -= damage;
                barPresent.sizeDelta= BarWidthSize(barPresent.sizeDelta,HpInPresentBar);

                // knock back
                if(damageDealer != null) shacker.AddImpact((transform.position - damageDealer.position), damage, false);

                // become invincible
                Invincible(0.2f);
            }
            // reduce a cell of bar
            else
            {
                presentHealth -= indiviualMaxValue;
                barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);
                float passedDamage = damage - indiviualMaxValue;
                // next bar
                if (barPresentId > 0) barPresentId--;
                barPresent = hpBarsList[barPresentId];
                TakeDamage(passedDamage , null);
            }
        }

        // player dead
        if (presentHealth < 0){
            GameManager.Restart();
        }
    }

    //*************************************************Recover*****************************************
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
        if (healingValue + presentHealth > Maxhealth)
        {
            healingValue = Maxhealth - presentHealth;
        }
        float BarSpaceLeft = indiviualMaxValue - HpInPresentBar;
        if (healingValue <= BarSpaceLeft){
            presentHealth += healingValue;
            HpInPresentBar += healingValue;
            if(barPresent!= null) barPresent.sizeDelta =BarWidthSize(barPresent.sizeDelta, HpInPresentBar);
        }
        else{
            presentHealth += BarSpaceLeft;
            if(barPresent != null) barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, indiviualMaxValue);
            float passedHP = healingValue - BarSpaceLeft;
            // load prevous bar
            if (barPresentId < (cellNum - activedTroopList.Count - 1)) barPresentId++;
            HpInPresentBar = 0;
            barPresent = hpBarsList[barPresentId];

            Healing(passedHP);
        }
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
            RegainTroopHP();
        }

    }
    void RegainTroopHP()
    {
        MinionTroop TargetTroop = null;

        // player didn't select a troop
        if (MarkedSubject == null){
            // find troop with lowest HP
            MinionTroop lowHPTroop = null;
            for (int i = 0; i < activedTroopList.Count; i++){
                if (lowHPTroop == null){
                    lowHPTroop = activedTroopList[i];
                }
                else if (activedTroopList[i].GetPresentHP() < lowHPTroop.GetPresentHP()){
                    lowHPTroop = activedTroopList[i];
                }
            }
            TargetTroop = lowHPTroop;
        }
        // player select a troop
        else{
            TargetTroop = MarkedSubject.GetComponent<Minion>().GetTroop();
        }

        // play recall effect
        TargetTroop.ExecuteMinionRecall();

        //remove lowest hp Troop
        RemoveTroopFromPlayerHealth(TargetTroop);

        // reset property
        MarkedSubject = null;
    }
    public void RemoveTroopFromPlayerHealth(MinionTroop troop) {

        // set HP
        presentHealth += troop.GetPresentHP();
        Maxhealth += indiviualMaxValue;

        //remove 
        activedTroopList.Remove(troop);
        Destroy(troop.gameObject, 0.53f);

        // genrate new item 
        Item_PlayerHealth myItem = Instantiate(itemHPTemp, hpUI.transform);
        myItem.gameObject.SetActive(true);
        myItem.GetComponent<CanvasGroup>().alpha = 0f;
        // change new item hierarchy to 0
        myItem.transform.SetSiblingIndex(0);

        // display
        Sequence mysequence = DOTween.Sequence();
        mysequence.Join(troop.transform.DOMoveY(-10, 0.3f))
            .Join(troop.GetComponent<CanvasGroup>().DOFade(0, 0.3f))
            .Join(myItem.GetComponent<CanvasGroup>().DOFade(1, 1f));

        // add it to list
        hpBarsList.Insert(0, myItem.troopHPUI);

        //reset parameter
        HealthHPReset();
    }
    void RegainAbsorbableHP()
    {
        Absorbable myAbsorbabl = MarkedSubject.GetComponent<Absorbable>();
        float healValue;
        healValue = myAbsorbabl.TakeLife();
        if (healValue != 0) Healing(healValue);
    }
    //*************************************************************** Rebirth Troop ***************************************************
    public void RebirthTroop( Vector3 pointedPos , float radius)
    {
        // have health to rebirth troop
        if (presentHealth > indiviualMaxValue)
        {

            Collider[] MinionInCircle = Physics.OverlapSphere(pointedPos, radius, LayerMask.GetMask("Minion"));// when rebirth minion, the layer will change

            if (MinionInCircle.Length > 0)
            {
                // get minion list
                List<Minion> minionSet = new List<Minion>();
                for (int i = 0; i < MinionInCircle.Length; i++){
                    if (MinionInCircle[i]!= null){
                        minionSet.Add(MinionInCircle[i].GetComponent<Minion>());
                    }
                }
                // reorder minions from highest to lowest
                //minionSet.Sort(SortByMinionSize);
                //minionSet.Reverse();

                // ********special minion
                for (int i = 0; i < minionSet.Count; i++)
                {
                    if (minionSet[i].minionSize == maxTroopCapacity)
                    {
                        MinionTroop mytroop = troopPresent;
                        GenerateNewTroop();

                        // revieve minion
                        minionSet[i].GetComponent<Minion>().SetActiveDelay(rebirthDelay);
                        //add to troop list
                        Debug.Log("maxTroopCapacity" + maxTroopCapacity);
                        troopPresent.AddTroopMember(minionSet[i].GetComponent<Minion>());

                        // reset the present troop
                        troopPresent = mytroop;// prevent troop present change trooppresent for small minion;

                        // remove item
                        minionSet.RemoveAt(i);
                        i--;
                        return;
                    }
                }

                //*******Normal minion
                if (minionSet.Count>0)
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
        else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Not engouh Health");        
    }
    
    static int SortByMinionSize(Minion p1, Minion p2)
    {
        return p1.minionSize.CompareTo(p2.minionSize);
    }

    void GenerateNewTroop()
    {
        // update data
        presentHealth -= indiviualMaxValue;
        Maxhealth -= indiviualMaxValue;

        //Destory First HP 
        Transform hpBarRemoving = hpBarsList[0].transform.parent.parent;
        Destroy(hpBarRemoving.gameObject, 0.53f);// delay for display
        hpBarsList.RemoveAt(0);
        if (barPresentId > 0) barPresentId--;

        // Generate Troop Bar
        troopPresent = Instantiate(itemMinionHPTemp, hpUI.transform);
        troopPresent.GetComponent<CanvasGroup>().alpha = 0f;
        troopPresent.gameObject.SetActive(true);
        troopPresent.GetComponent<MinionTroop>().ResetTroopHP(indiviualMaxValue, maxTroopCapacity);

        // display
        Sequence mysequence = DOTween.Sequence();
        mysequence.Join(hpBarRemoving.DOMoveY(-10f, 0.5f))
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
