using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerHealthBar : MonoBehaviour
{

    //items
    [SerializeField] Item_PlayerHealth itemHPTemp;
    [SerializeField] MinionTroop itemMinionHPTemp;
    [SerializeField] int cellNum = 5;

    // troop
    List<MinionTroop> allTroopList = new List<MinionTroop>();
    [SerializeField] int SingleTroopMaxMember = 6;
    List<MinionTroop> activedTroopList = new List<MinionTroop>();
    MinionTroop troopPresent = null;

    // hp
    [SerializeField] GameObject hpUI;
    List<RectTransform> hpBarsList = new List<RectTransform>();
    float initialBarWidth;
    public float Maxhealth = 100;
    public float presentHealth = 100;


    // invincible
    float invincibleTimer = 0;

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
    MinionTroop MarkedTroop;

    // Effect
    [SerializeField] GameObject rebirthRangeEffect;
    float rebirthDelay = 0.6f;


    private void Awake()
    {
        InitiatePlayerHPBar();
    }

    private void Start()
    {
        indiviualMaxValue = Maxhealth / hpBarsList.Count;
        barPresent = hpBarsList[cellNum - 1];

        HealthHPReset();
    }

    private void Update()
    {
        //Timer to prevent players getting multiple Damage;
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }

        // recover through time
        if (isActiveRecover){
            if (recoverTime <= 12 ){
                HealingAfterSeconds(healingValue, healingInterval);
                recoverTime += Time.deltaTime;
            }else isActiveRecover= false;
        }
        
        if (Input.GetKeyDown(KeyCode.T)) {
            TakeDamage(30f);
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
    public void TakeDamage(float damage)
    {
        if (invincibleTimer <= 0)
        {
            if (damage < HpInPresentBar)
            {
                presentHealth -= damage;
                HpInPresentBar -= damage;
                barPresent.sizeDelta= BarWidthSize(barPresent.sizeDelta,HpInPresentBar);
            }
            else
            {
                presentHealth -= indiviualMaxValue;
                barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);
                float passedDamage = damage - indiviualMaxValue;
                // next bar
                if (barPresentId > 0) barPresentId--;
                barPresent = hpBarsList[barPresentId];
                TakeDamage(passedDamage);
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
            barPresent.sizeDelta =BarWidthSize(barPresent.sizeDelta, HpInPresentBar);
        }
        else{
            presentHealth += BarSpaceLeft;
            barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, indiviualMaxValue);
            float passedHP = healingValue - BarSpaceLeft;
            // load prevous bar
            if (barPresentId < (cellNum - activedTroopList.Count - 1)) barPresentId++;
            HpInPresentBar = 0;
            barPresent = hpBarsList[barPresentId];

            Healing(passedHP);
        }
    }

    //*************************************************************** recall Or troop die ******************************************
    public void MarkTroop(MinionTroop troop)
    {
        MarkedTroop = troop;
    }
    public void RegainHP()
    {
        if (activedTroopList.Count > 0)
        {
            MinionTroop TargetTroop = null;

            // player didn't select a troop
            if (MarkedTroop == null){
                // find troop with lowest HP
                MinionTroop lowHPTroop = null;
                for (int i = 0; i < activedTroopList.Count; i++)
                {
                    if (lowHPTroop == null)
                    {
                        lowHPTroop = activedTroopList[i];
                    }
                    else if (activedTroopList[i].GetPresentHP() < lowHPTroop.GetPresentHP())
                    {
                        lowHPTroop = activedTroopList[i];
                    }
                }
                TargetTroop = lowHPTroop;
            }
            // player select a troop
            else{
                TargetTroop = MarkedTroop;
            }

            // play recall effect
            TargetTroop.ExecuteMinionRecall();

            //remove lowest hp Troop
            RemoveTroopFromPlayerHealth(TargetTroop);

            // reset property
            MarkedTroop = null;
        }
    }

    public void RemoveTroopFromPlayerHealth(MinionTroop troop) {

        // set HP
        presentHealth += troop.GetPresentHP();
        Maxhealth += indiviualMaxValue;

        //remove 
        activedTroopList.Remove(troop);
        Destroy(troop.gameObject,0.4f);

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

    //*************************************************************** Rebirth Troop ***************************************************

    public void RebirthTroop( Vector3 pointedPos , float radius)
    {
        // have health to rebirth troop
        if (presentHealth > indiviualMaxValue)
        {
            // show range indicator
            ShowRebirthRange(pointedPos, rebirthDelay);
            Collider[] MinionInCircle = Physics.OverlapSphere(pointedPos, radius, LayerMask.GetMask("Minion"));// when rebirth minion, the layer will change

            if (MinionInCircle.Length > 0)
            {
                // if present troop is full
                if (troopPresent == null || troopPresent.GetTroopSize() >= SingleTroopMaxMember)
                {
                    GenerateNewTroop();
                }
                // fill the troop
                if ((troopPresent.GetTroopSize() + MinionInCircle.Length) <= SingleTroopMaxMember) // enough slot for minion
                {
                    // call all minions out
                    for (int i = 0; i < MinionInCircle.Length; i++)
                    {
                        if (MinionInCircle[i].GetComponent<Minion>() != null)
                        {
                            MinionInCircle[i].GetComponent<Minion>().SetActiveDelay(rebirthDelay);
                            //add to troop list
                            troopPresent.AddTroopMember(MinionInCircle[i].GetComponent<Minion>());
                        }
                    }
                }
                else if ((troopPresent.GetTroopSize() + MinionInCircle.Length) > SingleTroopMaxMember) // too much minions
                {
                    int leftSlots = SingleTroopMaxMember - troopPresent.GetTroopSize();
                    for (int i = 0; i < leftSlots; i++)
                    {
                        if (MinionInCircle[i].GetComponent<Minion>() != null)
                        {
                            MinionInCircle[i].GetComponent<Minion>().SetActiveDelay(rebirthDelay);
                            //add to troop list
                            troopPresent.AddTroopMember(MinionInCircle[i].GetComponent<Minion>());
                        }
                    }
                }
            }
        }
        else GameManager.instance.PopUpUI(new Vector3(0, 24f, 0), "Not engouh Health");        
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

        // display
        Sequence mysequence = DOTween.Sequence();
        mysequence.Join(hpBarRemoving.DOMoveY(-10f, 0.5f))
            .Join(hpBarRemoving.GetComponent<CanvasGroup>().DOFade(0, 0.5f))
            .Append(troopPresent.GetComponent<CanvasGroup>().DOFade(1f, 1f));

        // add it to count list
        activedTroopList.Add(troopPresent);

    }

    void ShowRebirthRange( Vector3 pos , float delay)
    {
        GameObject effect = Instantiate(rebirthRangeEffect, pos, transform.rotation);
        Destroy(effect, delay);
    }

    //****************************************************** Invincible ******************************************************
    public void Invincible(float delay, float duration)
    {
        if (delay > 0)
        {
            StartInvincible(delay, duration);
        }
        else
        {
            // set invincible time;
            invincibleTimer = duration;
        }
    }

    IEnumerator StartInvincible(float dly, float invcDuration)
    {
        // delay few second to continue function
        yield return new WaitForSeconds(dly);

        // set invincible time;
        invincibleTimer = invcDuration;
    }
}
