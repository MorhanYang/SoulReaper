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

    [SerializeField] float hp = 10;
    [SerializeField] bool canRoam = false;
    [SerializeField] float roamInterval = 5f;
    bool isDead = false;
    float startRoamTime;
    Vector3 destination;

    // recall effect
    [SerializeField] GameObject recallingMinion;
    GameObject player;

    private void Start()
    {
        playerHP= PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        agent = GetComponent<NavMeshAgent>();
        player = PlayerManager.instance.player;

        startRoamTime = Time.time;
        destination = transform.position;
    }

    private void OnMouseEnter()
    {
        if (!isDead){
            selectEffect.SetActive(true);
            playerHP.MarkRegainTarget(transform);
        }
    }
    private void OnMouseExit()
    {
        selectEffect.SetActive(false);
        playerHP.MarkRegainTarget(null);
    }

    private void Update()
    {

        if (canRoam && !isDead && (Time.time - startRoamTime) > roamInterval)
        {
            // random a destination
            Debug.Log("Mouse random");
            Vector3 rdmDir = new Vector3(Random.Range(-1,1f), 0, Random.Range(-1, 1f));
            rdmDir.Normalize();
            destination = transform.position + rdmDir * Random.Range(0.4f, 1.5f);

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
            agent.SetDestination(transform.position);

            // play recall effect;
            GameObject effect = Instantiate(recallingMinion, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(player.transform);

            return hp;
        }
        else return 0;
    }
}
