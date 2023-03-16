
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead = false;
    bool haveSoul;

    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemySoul;

    Health health;
    EnemyBasicAi ai;
    GameObject player;

    float showHealthBarTimer = 0f;
    // combat
    Shaker shaker;

    // death trigger
    [SerializeField] BossBlocker blocker;


    private void Start()
    {

        health= GetComponent<Health>();
        health.HideHPUI();
        
        ai = GetComponent<EnemyBasicAi>();
        player = PlayerManager.instance.player;
        shaker= GetComponent<Shaker>();


        // Initiate the havesoul 
        if (enemySoul != null){
            haveSoul = true;
        } else haveSoul= false;
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
    }

    // Cursor Icon
    private void OnMouseEnter()
    {
        GameManager.instance.GetComponent<CursorManager>().ActivateCombatCursor();
    }
    private void OnMouseExit()
    {
        GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
    }

    //************************************************************************** Combat **************************************************************
    public void TakeDamage(float damage , Transform subject) {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        // slow down enemy
        if (ai != null){
            ai.SlowDownEnemy(0.6f);
        }
        //knock back
        shaker.AddImpact(transform.position - subject.position, damage, false);

        // change target
        if (ai.target == player.transform){
            ai.target = subject;
        }

        //died
        if (health.presentHealth <= 0)
        {
            // generate minion
            CheckIfHaveSoul();

            // decativate blockers
            if (blocker != null) blocker.ChangeBlockersState(false);

            // change cursor
            GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
        }

    }

    // ********************************************* rebirth *******************************************
    void CheckIfHaveSoul()
    {
        if (!haveSoul){
            Destroy(gameObject);
        }
        else
        {
            isDead = true;
            if (ai != null) ai.enabled = false;

            health.HideHPUI();

            // play dead animation
            if (enemySprite.GetComponent<Animator>() != null)
            {
                enemySprite.GetComponent<Animator>().SetBool("isDead", true);
            }
            BecomeMinion();
        }
            
    }

    public void BecomeMinion(){
        if (haveSoul){
            GameObject minion = Instantiate(enemySoul, transform.position, transform.rotation);
            haveSoul = false;
            Destroy(gameObject);
        }
    }

}
