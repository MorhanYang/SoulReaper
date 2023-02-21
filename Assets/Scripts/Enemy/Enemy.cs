
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isDead = false;
    bool haveSoul;

    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject haveSoulIcon;
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



    private void Start()
    {

        health= GetComponent<Health>();
        health.HideHPUI();
        
        ai = GetComponent<EnemyBasicAi>();
        player = PlayerManager.instance.player;

        // for static blocks
        if (isDead){
            ShowHaveSoulIcon();
        }

        // Initiate the havesoul 
        if (enemySoul != null){
            haveSoul = true;
        } else haveSoul= false;
    }

    private void Update()
    {
        // target is missing set new target
        if (target == null)
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
        // havesoul icon hiding
        DisableHaveSoulIcon();

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


    //**************************************************************************Method**************************************************************
    public void TakeDamage(float damage , GameObject subject) {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        // slow down enemy
        if (ai != null){
            ai.SlowDownEnemy(0.2f);
        }

        if (health.presentHealth <= 0){
            CheckIfHaveSoul();
        }

        // change target
        if (target == player){
            target = subject;
            ai.SetTarget(target);
        }
    }

    void CheckIfHaveSoul()
    {
        if (!haveSoul){
            Destroy(gameObject);
        }
        else
        {
            isDead = true;
            ShowHaveSoulIcon();
            if (ai != null){
                ai.enabled = false;
            }
            GetComponent<Collider>().isTrigger = true;

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
        if (haveSoulIcon != null && !haveSoul)
        {
            haveSoulIcon.SetActive(false);
        }
    }

    public void Rebirth(){
        if (haveSoul){
            GameObject minion = Instantiate(enemySoul, transform.position, transform.rotation);
            minion.GetComponent<Minion>().SetRebirthDelay(1f);
            haveSoul = false;
            GetComponent<Collider>().enabled = false;
        }
    }
}
