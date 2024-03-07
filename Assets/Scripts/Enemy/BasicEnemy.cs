using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.Shapes;

public class BasicEnemy : MonoBehaviour
{

    // set up
    PlayerHealth playerHP;
    NavMeshAgent agent;
    CursorManager cursorManager;

    [SerializeField] bool canRoam = false;
    [SerializeField] float roamInterval = 3f;

    //IsDead
    bool isDead = false;
    [HideInInspector]
    public bool GetDeadState() { return isDead; }
    public void SetDeadState( bool state ) { isDead = state; }

    float startRoamTime;
    Vector3 destination;
    Vector3 initialPos;

    // flip
    bool isFacingRight = true;

    // recall effect
    [SerializeField] GameObject recallingMinion;

    // effect 
    [SerializeField] GameObject selectEffect;
    [SerializeField] SpriteRenderer mysprite;
    [SerializeField] GameObject particle;


    private void Start()
    {
        playerHP= PlayerManager.instance.player.GetComponent<PlayerHealth>();
        if(canRoam) agent = GetComponent<NavMeshAgent>();
        cursorManager = GameManager.instance.GetComponent<CursorManager>();

        startRoamTime = Time.time;
        destination = transform.position;
    }

    private void OnMouseEnter()
    {
        if (!isDead){
            selectEffect.SetActive(true);
            playerHP.MarkRegainTarget(transform);

            if (this.GetComponent<EnemyScript>() != false){
                cursorManager.ActivateCombatCursor();
            }
            else cursorManager.ActivateRecallCursor();
        }
    }
    private void OnMouseExit()
    {
        selectEffect.SetActive(false);
        playerHP.MarkRegainTarget(null);
        cursorManager.ActivateDefaultCursor();
    }

    private void Update()
    {

        if (canRoam && !isDead && (Time.time - startRoamTime) > roamInterval)
        {
            Vector3 rdmDir;
            if (destination == initialPos)
            {
                // random a destination
                rdmDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
                rdmDir.Normalize();
                destination = transform.position + rdmDir * Random.Range(0.4f, 1.5f);
            }
            else
            {
                // return to initial pos
                destination = initialPos;
                rdmDir = (destination = transform.position).normalized;
            }

            // flip charactor
            if (rdmDir.x > 0 && isFacingRight)
            {
                mysprite.flipX = true;
                isFacingRight = !isFacingRight;
            }
            if (rdmDir.x < 0 && !isFacingRight)
            {
                mysprite.flipX = false;
                isFacingRight = !isFacingRight;
            }

            // find nearest point on the navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destination, out hit, 2.5f, NavMesh.AllAreas))
            {
                destination = hit.position;
            }
            else destination = transform.position;

            if (canRoam)
            {
                // set destination
                agent.SetDestination(destination);

                // reset timer
                startRoamTime = Time.time + Random.Range(0, 8f);
            }

        }
    }

    public void TakeLife()
    {
        if (!isDead){

            mysprite.color = Color.gray;
            particle.SetActive(false);
            selectEffect.SetActive(false);
            isDead = true;

            // stop movement
            if (canRoam){
                agent.SetDestination(transform.position);
            }
            else{
                if (GetComponent<AudioSource>() != null) GetComponent<AudioSource>().enabled = false;
                if (mysprite.GetComponent<Animator>() != null) mysprite.GetComponent<Animator>().enabled = false;
            }
        }
    }
}
