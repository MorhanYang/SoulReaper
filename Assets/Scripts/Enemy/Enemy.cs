using UnityEngine;

public class Enemy : MonoBehaviour
{
    CursorManager cursorManager;
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
    [SerializeField] GameObject attackEffect;

    // death trigger
    [SerializeField] SpikeGate blocker;
    [SerializeField] SoundTrigger previousSoundTrigger;

    // sound 
    SoundManager mySoundManager;
    private void Start()
    {

        health= GetComponent<Health>();
        health.HideHPUI();
        
        ai = GetComponent<EnemyBasicAi>();
        player = PlayerManager.instance.player;
        shaker= GetComponent<Shaker>();
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        mySoundManager = SoundManager.Instance;


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
        cursorManager.ActivateCombatCursor();
    }
    private void OnMouseExit()
    {
        cursorManager.ActivateDefaultCursor();
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
            if (blocker != null) blocker.openDoor();
            // stop triggered sound
            if(previousSoundTrigger != null) previousSoundTrigger.StopTriggeredSound();
            // change cursor
            GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
        }

        // play sound 
        mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 0.5f, 100, 100);

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

    // *********************************** Stop Combate music ***********************************


}
