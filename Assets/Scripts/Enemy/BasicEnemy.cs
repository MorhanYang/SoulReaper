using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour
{
    public enum EnemyType
    {
        StaticEnemy,
        MovingEnemy,
    }
    public EnemyType myEnemyType;
    [SerializeField] int fightingRounds; // how many rounds will it take to really die(1 = aborbe once, 2 = twice, -1 = die)

    // set up
    PlayerHealth playerHP;
    NavMeshAgent agent;
    CursorManager cursorManager;
    EnemyScript myEnemyScript;
    AbsorbableMark myAbsorbableMark;

    //health
    Health health;
    float showHealthBarTimer;

    //roam
    float startRoamTime;
    Vector3 destination;
    Vector3 initialPos;

    //damage
    float invincibleTime = 0;

    // flip
    public bool isFacingRight = true;

    // recall effect
    [SerializeField] GameObject recallingMinion;

    // effect 
    // effect & Sound
    Shaker shaker;
    SoundManager mySoundManager;
    [SerializeField] SpriteRenderer headIcon = null;
    [SerializeField] SpriteRenderer mysprite;
    [SerializeField] GameObject particle;


    private void Start()
    {
        playerHP= PlayerManager.instance.player.GetComponent<PlayerHealth>();
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        myAbsorbableMark = GetComponent<AbsorbableMark>();

        startRoamTime = Time.time;
        destination = transform.position;

        switch (myEnemyType)
        {
            case EnemyType.StaticEnemy:
                headIcon.sprite = HeadIconManager.GetSprite("Absorb");
                break;
            case EnemyType.MovingEnemy:
                agent = GetComponent<NavMeshAgent>();
                health = GetComponent<Health>();
                shaker = GetComponent<Shaker>();
                mySoundManager = SoundManager.Instance;
                myEnemyScript = GetComponent<EnemyScript>();
                health.HideHPUI();
                break;

            default:
                break;
        }
    }

    private void OnMouseEnter()
    {
        switch (myEnemyType)
        {
            case EnemyType.StaticEnemy:
                if (fightingRounds >=0) // didn't die
                {
                    headIcon.sprite = HeadIconManager.GetSprite("Select");
                    playerHP.MarkRegainTarget(transform);
                    cursorManager.ActivateRecallCursor();
                }
                break;

            case EnemyType.MovingEnemy:
                if (myEnemyScript.action == EnemyScript.EnemyAction.Recovering)
                {
                    headIcon.sprite = HeadIconManager.GetSprite("Select");
                    playerHP.MarkRegainTarget(transform);
                    cursorManager.ActivateRecallCursor();
                }
                else cursorManager.ActivateCombatCursor();
                break;

            default:
                break;
        }
    }
    private void OnMouseExit()
    {
        switch (myEnemyType)
        {
            case EnemyType.StaticEnemy:
                headIcon.sprite = null;
                playerHP.MarkRegainTarget(null);
                cursorManager.ActivateDefaultCursor();
                if (fightingRounds >=0)
                {
                    headIcon.sprite = HeadIconManager.GetSprite("Absorb");
                }
                else headIcon.sprite = null;

                break;

            case EnemyType.MovingEnemy:
                if (myEnemyScript.action == EnemyScript.EnemyAction.Recovering)
                {
                    headIcon.sprite = HeadIconManager.GetSprite("Absorb");
                }
                else { headIcon.sprite = null; }
                playerHP.MarkRegainTarget(null);
                cursorManager.ActivateDefaultCursor();
                break;
            default:
                break;
        }

    }

    private void Update()
    {
        if (myEnemyType == EnemyType.MovingEnemy)
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
            // flip sprite when move
            Flip();
        }
        
    }


    // ********************************************* take Damage *****************************************
    public void TakeDamage(float damage, Transform subject, Vector3 attackPos)
    {
        if (Time.time - invincibleTime > 2f) // 1.5 seconds invincible
        {
            float hideHealthBarDelay = 5f;

            // normal state takes damage
            if (myEnemyScript.action != EnemyScript.EnemyAction.Recovering)
            {
                health.TakeDamage(damage);

                health.ShowHPUI();
                showHealthBarTimer = hideHealthBarDelay;

                // try to change target
                myEnemyScript.ChangeTargetToAttacker(subject);
                //die
                if (health.presentHealth <= 0)
                {
                    if (fightingRounds <= 0) // should die right now
                    {
                        myEnemyScript.BecomeMinion();
                    }
                    else if (fightingRounds > 0) // wait for absorb and keep fight
                    {
                        myAbsorbableMark.enabled = true;
                        myEnemyScript.SetEnemyAction(EnemyScript.EnemyAction.Recovering);
                        health.HideHPUI();
                        headIcon.sprite = HeadIconManager.GetSprite("Absorb");
                        invincibleTime = Time.time; // set invicible timer

                        // automatic execute recovering enemy after 6s 
                        Invoke("TakeLifeButRecover", 6);
                    }
                }
            }
            // recovering state
            else if (myEnemyScript.action == EnemyScript.EnemyAction.Recovering)
            {
                myAbsorbableMark.enabled = false;
                TakeLifeButRecover();
            }

            // play sound 
            mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 0.5f, 100, 100);
        }
    }

    // ********************************************* Eat Enemy *****************************************
    public void TakeLife()
    {
        switch (myEnemyType)
        {
            case EnemyType.StaticEnemy:
                if (fightingRounds >= 0)
                {
                    mysprite.color = Color.gray;
                    particle.SetActive(false);
                    headIcon.sprite = null;
                    fightingRounds = -1;
                }
                break;

            case EnemyType.MovingEnemy:
                break;
            default:
                break;
        }
    }

    // ************************** Fight Next Round 
    public void TakeLifeButRecover()
    {
        if (myEnemyScript.action == EnemyScript.EnemyAction.Recovering) // for delay function, if this enemy is not at recovering state just ignore the dealy function.
        {
            // fightingRounds > 1 -> keep fighting
            if (fightingRounds > 1)
            {
                health.SetHealThPercentage(1);
                myEnemyScript.SetEnemyAction(EnemyScript.EnemyAction.idle);
            }

            // fightRounds == 1 -> absorb and die
            if (fightingRounds <= 1)
            {
                myEnemyScript.BecomeMinion();
            }

            // reset cursor and health marker
            headIcon.sprite = null;
            playerHP.MarkRegainTarget(null);
            cursorManager.ActivateDefaultCursor();

            // next round
            fightingRounds--;
        }
        
    }
    //************************************************ Flip *******************************************
    void Flip()
    {
        //Enemies face right when moving right
        if (agent.velocity.x < 0 && isFacingRight)
        {
            mysprite.flipX = true;
            isFacingRight = !isFacingRight;

        }
        //face left when facing left
        else if (agent.velocity.x > 0 && !isFacingRight)
        {
            mysprite.flipX = false;
            isFacingRight = !isFacingRight;
        }
        //or remain its direction when static
    }

    // ****************************************** Roam *************************************************
    void Roam()
    {
        //if (canRoam && !isDead && (Time.time - startRoamTime) > roamInterval)
        //{
        //    Vector3 rdmDir;
        //    if (destination == initialPos)
        //    {
        //        // random a destination
        //        rdmDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
        //        rdmDir.Normalize();
        //        destination = transform.position + rdmDir * Random.Range(0.4f, 1.5f);
        //    }
        //    else
        //    {
        //        // return to initial pos
        //        destination = initialPos;
        //        rdmDir = (destination = transform.position).normalized;
        //    }

        //    // find nearest point on the navmesh
        //    NavMeshHit hit;
        //    if (NavMesh.SamplePosition(destination, out hit, 2.5f, NavMesh.AllAreas))
        //    {
        //        destination = hit.position;
        //    }
        //    else destination = transform.position;

        //    if (canRoam)
        //    {
        //        // set destination
        //        agent.SetDestination(destination);

        //        // reset timer
        //        startRoamTime = Time.time + Random.Range(0, 8f);
        //    }

        //}
    }
}
