using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    //items
    [SerializeField] Item_PlayerHealth itemTemp;
    [SerializeField] int cellNum = 5;

    // troop
    List<MinionTroop> allTroopList = new List<MinionTroop>();
    [SerializeField] int SingleTroopMaxMember = 6;
    List<MinionTroop> activedTroopList = new List<MinionTroop>();
    MinionTroop troopPresent = null;

    // hp
    [SerializeField] GameObject hpUI;
    List<RectTransform> hpBars = new List<RectTransform>();
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



    private void Awake()
    {
        InitiatePlayerHPBar();
    }

    private void Start()
    {
        indiviualMaxValue = Maxhealth / hpBars.Count;
        barPresent = hpBars[cellNum - 1];

        //presentHealth = Maxhealth;
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
        HealingAfterSeconds(healingValue, healingInterval);

    }
    //****************************************************Initialize & property*************************************************************
    void InitiatePlayerHPBar()
    {
        itemTemp.gameObject.SetActive(true);
        initialBarWidth = itemTemp.troopHPUI.sizeDelta.x;
        for (int i = 0; i < cellNum; i++)
        {
            Item_PlayerHealth hpCell = Instantiate(itemTemp, hpUI.transform);
            allTroopList.Add(hpCell.troop);
            hpBars.Add(hpCell.troopHPUI);
        }

        itemTemp.gameObject.SetActive(false);
    }

    public List<MinionTroop> GetActivedTroop(){
        return activedTroopList;
    }

    Vector3 BarWidthSize(Vector3 previousSize, float Hpinbar){
        return new Vector3((Hpinbar / indiviualMaxValue * initialBarWidth), previousSize.y, previousSize.z);
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
                barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);
                float passedDamage = damage - HpInPresentBar;
                //reset previous bar info
                HpInPresentBar = indiviualMaxValue;
                // next bar
                if (barPresentId > 0) barPresentId--;
                barPresent = hpBars[barPresentId];
                TakeDamage(passedDamage);
            }
        }
    }

    //*************************************************Recover*****************************************

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
            barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, indiviualMaxValue);
            float passedHP = healingValue - BarSpaceLeft;
            // load prevous bar
            if (barPresentId < (cellNum - activedTroopList.Count - 1)) barPresentId++;
            HpInPresentBar = 0;
            barPresent = hpBars[barPresentId];

            Healing(passedHP);
        }
    }

    //*************************************************************** recall Or troop die ******************************************
    public void RegainHP()
    {
        if (activedTroopList.Count > 0)
        {
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
                    if (activedTroopList[i].GetPresentHP() < lowHPTroop.GetPresentHP())
                        lowHPTroop = activedTroopList[i];
                }
            }
            // play recall effect
            lowHPTroop.PlayMinionRecall();

            //remove lowest hp Troop
            RemoveTroopFromPlayerHealth(lowHPTroop);
        }
    }

    public void RemoveTroopFromPlayerHealth(MinionTroop troop) {

        // set HP
        presentHealth += troop.GetPresentHP();
        Maxhealth += indiviualMaxValue;

        //remove 
        Item_PlayerHealth previousItem = troop.transform.parent.GetComponent<Item_PlayerHealth>();
        activedTroopList.Remove(troop);
        allTroopList.Remove(troop);
        hpBars.Remove(previousItem.troopHPUI);
        Destroy(previousItem.gameObject);

        // genrate new item 
        itemTemp.gameObject.SetActive(true);
        Item_PlayerHealth myItem = Instantiate(itemTemp, hpUI.transform);
        itemTemp.gameObject.SetActive(false);
        // change new item hierarchy to 0
        myItem.transform.SetSiblingIndex(0);

        // add it to two list
        allTroopList.Insert(0, myItem.troop);
        hpBars.Insert(0, myItem.troopHPUI);

        //reset parameter
        HealthHPReset();
    }

    //*************************************************************** Troop ***************************************************

    public void RebirthTroop( Vector3 pointedPos , float radius)
    {
        Collider[] MinionInCircle = Physics.OverlapSphere(pointedPos, radius, LayerMask.GetMask("MinionLayer"));

        if (MinionInCircle.Length > 0)
        {
            // if present troop is full
            if (troopPresent == null || troopPresent.GetTroopSize()>= SingleTroopMaxMember)
            {
                GenerateNewTroop();
            }

            // fill the troop
            if ((troopPresent.GetTroopSize() + MinionInCircle.Length) <= SingleTroopMaxMember) // enough slot for minion
            {
                // call all minions out
                for (int i = 0; i < MinionInCircle.Length; i++)
                {
                    if (MinionInCircle[i].GetComponent<Minion>() != null){
                        MinionInCircle[i].GetComponent<Minion>().SetActiveDelay(0.6f);
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
                        MinionInCircle[i].GetComponent<Minion>().SetActiveDelay(0.6f);
                        //add to troop list
                        troopPresent.AddTroopMember(MinionInCircle[i].GetComponent<Minion>());
                    }
                }
            }
        }
    }

    void GenerateNewTroop()
    {
        // refresh health bar
        presentHealth -= indiviualMaxValue;
        Maxhealth -= indiviualMaxValue;
        barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);// clean previous bar
        // next bar
        if (barPresentId > 0) barPresentId--;
        barPresent = hpBars[barPresentId];
        barPresent.sizeDelta = BarWidthSize(barPresent.sizeDelta, HpInPresentBar);


        // show Troop Bar
        int troopId = allTroopList.Count - 1 - activedTroopList.Count;
        if (troopId < 0) troopId++;
        troopPresent = allTroopList[troopId];

        troopPresent.gameObject.SetActive(true);
        troopPresent.ResetTroopHP(indiviualMaxValue, SingleTroopMaxMember);

        // add it to count list
        activedTroopList.Add(troopPresent);

    }

    //*********************************************************** Display *****************************************************
    void HealthHPReset()
    {
        // clean all bars
        for (int i = 0; i < hpBars.Count; i++){
            hpBars[i].sizeDelta = BarWidthSize(barPresent.sizeDelta, 0);
        }

        // count full bars
        int fullBarNum = 0;
        fullBarNum = (int)((presentHealth - 0.1f) / indiviualMaxValue);
        if (fullBarNum < 0) fullBarNum= 0;

        // set full bars
        for (int i = 0; i < fullBarNum; i++)
        {
            hpBars[i].sizeDelta = BarWidthSize(barPresent.sizeDelta, indiviualMaxValue);
        }

        // set the half bar
        HpInPresentBar = presentHealth - (indiviualMaxValue * fullBarNum);
        if (HpInPresentBar > 0){
            hpBars[fullBarNum].sizeDelta = BarWidthSize(barPresent.sizeDelta, HpInPresentBar);
        }

        // property
        barPresent = hpBars[fullBarNum];
        barPresentId = fullBarNum;
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
