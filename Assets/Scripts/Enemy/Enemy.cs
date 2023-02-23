
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead = false;
    bool haveSoul;

    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemySoul;
    [SerializeField] float myDamage = 10;
    [SerializeField] float attackInterval = 3f;

    Health health;
    EnemyBasicAi ai;
    GameObject player;

    float showHealthBarTimer = 0f;
    // combat
    float damageTimer;
    GameObject target;

    // death trigger
    [SerializeField] GameObject otherScrips;


    private void Start()
    {

        health= GetComponent<Health>();
        health.HideHPUI();
        
        ai = GetComponent<EnemyBasicAi>();
        player = PlayerManager.instance.player;


        // Initiate the havesoul 
        if (enemySoul != null){
            haveSoul = true;
        } else haveSoul= false;
    }

    private void Update()
    {
        // target is missing set new target
        if (target == null){
            target = player;
            ai.SetTarget(target);
        }
        else if (target.GetComponent<Minion>() != null && !target.GetComponent<Minion>().isActive)
        {
            target = player;
            ai.SetTarget(target);
        }

        // health bar
        if (showHealthBarTimer >= 0){
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f) { 
            health.HideHPUI();
            }
        }
    }

    // Cursor Controll
    private void OnMouseEnter()
    {
        if (isDead && haveSoul){
            // change cursor
            CursorManager.instance.ActivateRecallCursor();
        }
    }
    private void OnMouseExit()
    {
        // change cursor
        PlayerControl playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        if (playerControl.playerState == PlayerControl.PlayerState.combat)
        {
            CursorManager.instance.ActivateCombatCursor();
        }
        else CursorManager.instance.ActivateDefaultCursor();
    }


    private void OnTriggerStay(Collider other)
    {
        // colide with target
        if (other.gameObject == target)
        {
            if (!isDead && damageTimer >= attackInterval)
            {
                if (other.transform.GetComponent<Minion>() != null && !other.IsDestroyed()) other.transform.GetComponent<Minion>().TakeDamage(myDamage);
                if (other.transform.GetComponent<PlayerControl>() != null) other.transform.GetComponent<PlayerControl>().PlayerTakeDamage(myDamage);
                damageTimer = 0f;
            }
            else damageTimer += Time.deltaTime;
        }
    }


    //************************************************************************** Combat **************************************************************
    public void TakeDamage(float damage , GameObject subject) {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        // slow down enemy
        if (ai != null){
            ai.SlowDownEnemy(0.2f);
        }

        // change target
        if (target == player){
            target = subject;
            ai.SetTarget(target);
        }

        //died
        if (health.presentHealth <= 0)
        {
            // generate minion
            CheckIfHaveSoul();

            // tirgger event
            if (otherScrips != null) TriggerAdditionalScript();
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

    // **************************************Addtional Scrips trigger*********************************
    void TriggerAdditionalScript()
    {
    }
}
