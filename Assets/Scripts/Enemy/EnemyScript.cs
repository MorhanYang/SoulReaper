using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemySoul;
    GameObject player;


    Health health;
    BasicEnemy basicEnemy;
    bool haveSoul;

    // effect & Sound
    Shaker shaker;
    [SerializeField] GameObject attackEffect;
    SoundManager mySoundManager;

    // death trigger
    [SerializeField] SpikeGate blocker;
    [SerializeField] SoundTrigger previousSoundTrigger;

    float showHealthBarTimer = 0f;
    bool isDying = false;
    

    private void Start()
    {
        health = GetComponent<Health>();
        basicEnemy = GetComponent<BasicEnemy>();
        health.HideHPUI();

        player = PlayerManager.instance.player;
        shaker = GetComponent<Shaker>();
        mySoundManager = SoundManager.Instance;


        // Initiate the havesoul 
        if (enemySoul != null)
        {
            haveSoul = true;
        }
        else haveSoul = false;
    }

    private void Update()
    {
        // health bar
        if (showHealthBarTimer >= 0)
        {
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f)
            {
                health.HideHPUI();
            }
        }
    }

    // kill
    public void TakeDamage(float damage, Transform subject, Vector3 attackPos)
    {
        float hideHealthBarDelay = 5f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        //// slow down enemy
        //if (ai != null)
        //{
        //    ai.SlowDownEnemy(0.6f);
        //}

        //knock back
        shaker.AddImpact((transform.position - attackPos), damage, false);

        //// change target
        //if (ai.target == player.transform)
        //{
        //    ai.target = subject;
        //}
        //dying
        if (health.presentHealth <= 0 && !isDying)
        {
            StartCoroutine(DyingCoroutine());
        }
        else if (isDying){
            Die();
        }
        /*
        //died
        if (health.presentHealth <= 0)
        {
            // generate minion
            CheckIfHaveSoul();

            // decativate blockers
            if (blocker != null) blocker.openDoor();
            // stop triggered sound
            if (previousSoundTrigger != null) previousSoundTrigger.StopTriggeredSound();
            // change cursor
            GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
        }
        */
        // play sound 
        mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 0.5f, 100, 100);
    }

    IEnumerator DyingCoroutine()
    {
        //******************************
        //dying animation/sound effect to be implemented
        //******************************
        isDying = true;
        yield return new WaitForSeconds(5); // Wait for 5 seconds
        isDying = false;
        Die();
    }
    void Die()
    {
        // Your death logic here: Destroy the enemy, play death animation, etc.
        if (haveSoul)
        {
            basicEnemy.SetDeadState(true);
            if (enemySprite.GetComponent<Animator>() != null)
            {
                enemySprite.GetComponent<Animator>().SetBool("isDead", true);
            }
            BecomeMinion();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CheckIfHaveSoul()
    {
        if (!haveSoul)
        {
            Destroy(gameObject);
        }
        else
        {
            basicEnemy.SetDeadState(true);
            //if (ai != null) ai.enabled = false;

            health.HideHPUI();

            // play dead animation
            if (enemySprite.GetComponent<Animator>() != null)
            {
                enemySprite.GetComponent<Animator>().SetBool("isDead", true);
            }
            BecomeMinion();
        }
    }

    public void BecomeMinion()
    {
        if (haveSoul)
        {
            GameObject minion = Instantiate(enemySoul, transform.position, transform.rotation);
            haveSoul = false;
            Destroy(gameObject);
        }
    }
}
