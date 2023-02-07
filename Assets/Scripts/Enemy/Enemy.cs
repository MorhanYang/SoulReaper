using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead = false;

    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject haveSoulIcon;
    [SerializeField] GameObject EnemySoul;
    [SerializeField] float damage = 10;
    Health health;
    EnemyBasicAi ai;

    float showHealthBarTimer = 0f;


    private void OnEnable()
    {
        health= GetComponent<Health>();
        health.HideHPUI();
        
        ai = GetComponent<EnemyBasicAi>();

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

    private void OnCollisionEnter(Collision collision)
    {
        // colide with Player
        if (collision.transform.GetComponent<PlayerControl>()!= null){
            if (!isDead){
                PlayerControl player = collision.transform.GetComponent<PlayerControl>();
                player.PlayerTakeDamage(damage);
            }
        }
    }


    //**************************Method***************************
    public void TakeDamage(float damage) {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        // slow down enemy
        if (ai != null){
            ai.SlowDownEnemy(0.3f);
        }

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
            if (ai != null){
                ai.enabled = false;
            }

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
