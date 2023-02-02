using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] bool haveSoul = true;
    [SerializeField] GameObject enemySprite;
    Health health;

    float showHealthBarTimer = 0f;

    private void OnEnable()
    {
        health= GetComponent<Health>();
        health.HideHPUI();
    }

    private void Update()
    {
        if (showHealthBarTimer >= 0){
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f) { 
            health.HideHPUI();
            }
        }
    }

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
        if (!haveSoul){
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("You get dead body");
            GetComponent<EnemySoulLogic>().DeathRitual();
            GetComponent<EnemyBasicAi>().enabled = false;
            health.HideHPUI();

            // play dead animation
            enemySprite.GetComponent<Animator>().SetBool("isDead", true);
        }
            
    }
}
