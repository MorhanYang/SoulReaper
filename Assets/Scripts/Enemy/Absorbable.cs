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
    [SerializeField] GameObject particle;
    PlayerHealthBar playerHP;
    NavMeshAgent agent;
    CursorManager cursorManager;

    [SerializeField] float hp = 10;
    public bool addHealthMax;
    [SerializeField] bool canRoam = false;
    [SerializeField] float roamInterval = 5f;

    //IsDead
    bool isDead = false;
    [HideInInspector]
    public bool isDeadPublic { get { return isDead; } }

    float startRoamTime;
    Vector3 destination;

    // flip
    bool isFacingRight = true;

    // recall effect
    [SerializeField] GameObject recallingMinion;
    GameObject player;
    SoundManager mySoundManager;

    private void Start()
    {
        playerHP= PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        if(canRoam) agent = GetComponent<NavMeshAgent>();
        player = PlayerManager.instance.player;
        cursorManager = GameManager.instance.GetComponent<CursorManager>();
        mySoundManager = SoundManager.Instance;

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
            // play sound
            mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Release", false, false, 1, 1, 100, 100);

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

            // play recall effect;
            GameObject effect = Instantiate(recallingMinion, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(player.transform);

            if (!addHealthMax) return hp;
            // add player healthMax
            else return -1;
        }
        else return 0;
    }
}
