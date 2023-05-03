using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] Canvas hpUI;
    [SerializeField] Image hpBar;
    public float Maxhealth;
    public float presentHealth;

    // invincible
    float invincibleTimer = 0;


    private void Awake()
    {
        presentHealth = Maxhealth; 
    }

    private void OnEnable()
    {
        HealthUpdate();
    }
    private void Update()
    {
        //Timer to prevent players getting multiple Damage;
        if (invincibleTimer >0){
            invincibleTimer -= Time.deltaTime;
        }
    }

    //*****************Method*******************
    public void HideHPUI(){
        if (hpUI != null) hpUI.gameObject.SetActive(false);
    }
    public void ShowHPUI(){
        if (hpUI != null) hpUI.gameObject.SetActive(true);
    }

    public void TakeDamage(float damage)
    {
        if (invincibleTimer <= 0)
        {
            presentHealth -= damage;
            HealthUpdate();
        }
    }

    public void HealthUpdate()
    {
        hpBar.fillAmount = presentHealth / Maxhealth;
    }

    public void Invincible(float delay, float duration) {
        if (delay > 0)
        {
            StartInvincible(delay, duration);
        }
        else {
            // set invincible time;
            invincibleTimer = duration;
        }
    }

    IEnumerator StartInvincible(float dly, float invcDuration) { 
        // delay few second to continue function
        yield return new WaitForSeconds(dly);

        // set invincible time;
        invincibleTimer = invcDuration;
    }
}
