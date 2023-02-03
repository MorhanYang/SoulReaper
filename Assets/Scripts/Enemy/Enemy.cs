using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead = false;

    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject haveSoulIcon;
    [SerializeField] GameObject EnemySoul;
    Health health;

    float showHealthBarTimer = 0f;


    private void OnEnable()
    {
        health= GetComponent<Health>();
        health.HideHPUI();

        // for static blocks
        if (isDead){
            ShowHaveSoulIcon();
        }
    }

    private void Update()
    {
        // health bar
        if (showHealthBarTimer >= 0){
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f) { 
            health.HideHPUI();
            }
        }
        // havesoul icon hiding
        DisableHaveSoulIcon();
    }

    // visible or invisible souls
    private void OnMouseEnter()
    {
        if (isDead && EnemySoul != null){
            Debug.Log("Hit dead body");
            ConvertSoulVisuability(true);
            // change cursor
            CursorManager.instance.ActivateRecallCursor();
        }
    }
    private void OnMouseExit()
    {
        ConvertSoulVisuability(false);

        // change cursor
        PlayerControl playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        if (playerControl.playerState == PlayerControl.PlayerState.combat)
        {
            CursorManager.instance.ActivateCombatCursor();
        }
        else CursorManager.instance.ActivateDefaultCursor();
    }


    //**************************Method***************************
    public void TakeDamage(float damage) {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        if (health.presentHealth <= 0){
            CheckIfHaveSoul();
        }
    }

    void CheckIfHaveSoul()
    {
        if (EnemySoul == null){
            Destroy(gameObject);
        }
        else
        {
            isDead = true;
            ShowHaveSoulIcon();

            GetComponent<EnemyBasicAi>().enabled = false;
            health.HideHPUI();

            // play dead animation
            enemySprite.GetComponent<Animator>().SetBool("isDead", true);
        }
            
    }

    public void ShowHaveSoulIcon()
    {
        if (haveSoulIcon != null){
            haveSoulIcon.SetActive(true);
        }
    }

    void DisableHaveSoulIcon()
    {
        if (haveSoulIcon != null && EnemySoul == null)
        {
            haveSoulIcon.SetActive(false);
        }
    }

    public void ConvertSoulVisuability(bool soulVisiablilitySate)
    {
        if (EnemySoul != null)
        {
            EnemySoul.SetActive(soulVisiablilitySate);
        }
    }
}
