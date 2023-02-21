using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] Canvas hpUI;
    [SerializeField] RectTransform hpBar;
    public float Maxhealth;
    public float presentHealth;

    // invincible
    float invincibleTimer = 0;

    float initialHPBarWidth;

    private void Start()
    {
        presentHealth = Maxhealth; 
    }

    private void OnEnable()
    {
        initialHPBarWidth = hpBar.sizeDelta.x;

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
        hpUI.gameObject.SetActive(false);
    }
    public void ShowHPUI(){
        hpUI.gameObject.SetActive(true);
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
        Vector2 presentHpBarSize = new Vector2((presentHealth / Maxhealth) * initialHPBarWidth, hpBar.sizeDelta.y);
        hpBar.sizeDelta = presentHpBarSize;
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
