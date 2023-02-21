using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] MinionTroop[] troopList;


    [SerializeField] GameObject hpUI;
    [SerializeField] Slider[] hpBars;
    public float Maxhealth = 100;
    public float presentHealth = 100;

    // invincible
    float invincibleTimer = 0;

    // HealthBar Switch
    Slider barPresent;
    int barPresentId;
    float indiviualMaxValue;
    float HpInPresentBar;

    private void Start()
    {
        if (hpBars.Length == 0) Debug.Log("hpBars is empy. you must set something here");

        indiviualMaxValue = Maxhealth / hpBars.Length;
        barPresent = hpBars[hpBars.Length-1];

        presentHealth = Maxhealth;
        HealthHPReset();
    }

    private void Update()
    {
        //Timer to prevent players getting multiple Damage;
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(30);
        }
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
                barPresent.value = HpInPresentBar / indiviualMaxValue;
            }
            else
            {
                barPresent.value = 0;
                float passedDamage = damage - HpInPresentBar;
                barPresent = NextBar();
                TakeDamage(passedDamage);
            }
        }
    }

    public Slider NextBar()
    {
        if (barPresentId>0) barPresentId--;
        //reset bar info
        HpInPresentBar = indiviualMaxValue;
        return hpBars[barPresentId];
    }

    //***************************************************************Convert to Troop health***************************************************
    public void AddTroopMember(Minion member)
    {
        // if minions excess maxtroop capacity, it will not be recalled.
        if (troopList[0].AddTroopMember(member)){
            member.RecallMinion();
        }
    }


    //*********************************************************** Display *****************************************************
    private int FindHPBarID()
    {
        // find right HP bar
        int healthBarPresentID = (int)(presentHealth / indiviualMaxValue);
        int healthBarPresentIDTest = (int)((presentHealth - 0.1f) / indiviualMaxValue);
        // avoid heal is at the boundary
        if (healthBarPresentIDTest < healthBarPresentID && healthBarPresentIDTest >= 0)
        {
            healthBarPresentID = healthBarPresentIDTest;
        }
        // hp < 0
        if (healthBarPresentID < 0) healthBarPresentID = 0;

        return healthBarPresentID;
    }

    void HealthHPReset()
    {
        int hpBarID = FindHPBarID();

        float hpLeft = presentHealth - indiviualMaxValue * hpBarID; 


        // displace change
        for (int i = 0; i < hpBars.Length; i++)
        {
            if (i < hpBarID) hpBars[i].value = 1;
            if (i == hpBarID) hpBars[i].value = hpLeft/indiviualMaxValue;
            if (i > hpBarID) hpBars[i].value = 0;
        }

        barPresent = hpBars[hpBarID];
        HpInPresentBar = hpLeft;
        barPresentId = hpBarID;
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
