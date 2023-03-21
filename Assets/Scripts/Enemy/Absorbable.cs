using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.Shapes;

public class Absorbable : MonoBehaviour
{
    [SerializeField] GameObject selectEffect;
    [SerializeField] SpriteRenderer mysprite;
    PlayerHealthBar playerHP;
    NavMeshAgent agent;
    CursorManager cursorManager;

    [SerializeField] float hp = 10;
    [SerializeField] bool canRoam = false;
    [SerializeField] float roamInterval = 5f;
    bool isDead = false;
    float startRoamTime;
    Vector3 destination;

    // flip
    bool isFacingRight = true;

    // recall effect
    [SerializeField] GameObject recallingMinion;
    GameObject player;

    private void Start()
    {
        playerHP= PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        if(canRoam) agent = GetComponent<NavMeshAgent>();
        player = PlayerManager.instance.player;
        cursorManager = GameManager.instance.GetComponent<CursorManager>();

        startRoamTime = Time.time;
        destination = transform.position;
    }

    private void OnMouseEnter()
    {
        if (!isDead){
            selectEffect.SetActive(true);
            playerHP.MarkRegainTarget(transform);
            cursorManager.ActivateRecallCursor();
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
            // random a destination
            Vector3 rdmDir = new Vector3(Random.Range(-1,1f), 0, Random.Range(-1, 1f));
            rdmDir.Normalize();
            destination = transform.position + rdmDir * Random.Range(0.4f, 1.5f);

            // flip charactor
            if(rdmDir.x > 0 && isFacingRight) 
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

    public float TakeLife()
    {
        if (!isDead){
            mysprite.color = Color.gray;
            isDead = true;

            // stop movement
            if(canRoam) agent.SetDestination(transform.position);

            // play recall effect;
            GameObject effect = Instantiate(recallingMinion, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(player.transform);

            return hp;
        }
        else return 0;
    }
}
