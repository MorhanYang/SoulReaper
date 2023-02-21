using Fungus;
using System.Collections;
using UnityEngine;

public class Soul_Dash : MonoBehaviour
{
    Rigidbody rb;
    Vector3 moveDir;
    float moveSpeed;
    GameObject player;

    [HideInInspector]public int soulType;// reference GhostList & PlayerControl
    // Shoot
    [SerializeField] float shootSpeed;
    bool Attacked = false;

    [SerializeField] GameObject impactEffect;

    //recall
    float presentRecallSpeed;
    bool canRecall = true;
    
    [SerializeField] float resistance;
    [SerializeField] float recallSpeed = 10f;


    public float soulDamage;

    enum SoulState
    {
        shootingout,
        normal,
        recalling,
    }
    SoulState soulState = SoulState.normal;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = PlayerManager.instance.player;

        // regular soul set up
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        switch (soulState)
        {
            case SoulState.normal:
                //reset recall speed
                presentRecallSpeed = 2;
                break;
            case SoulState.shootingout:
                SlowDown();
                break;
            case SoulState.recalling:
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, presentRecallSpeed * Time.fixedDeltaTime);
                if (presentRecallSpeed < recallSpeed){
                    presentRecallSpeed += 20 * Time.fixedDeltaTime;
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.tag == "Player"){
            Destroy(gameObject);
            // send soultype to playercontrol
            collision.transform.GetComponent<PlayerControl>().AddSoulList(soulType);
        }

        // collide with walls
        if (collision.transform.tag == "Obstruction")
        {
            StopRecall();
        }

        // collide with enemy
        if (collision.transform.GetComponent<Enemy>() != null
            && !collision.transform.GetComponent<Enemy>().isDead)
        {
            if (!Attacked){
                RegularSoulHitEnemy(collision.gameObject, soulDamage);
                Attacked = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Attacked = false;
    }

    //*******************************Method**********************************

    public void ShootSoul(Vector3 shootDir)
    {
        soulState = SoulState.shootingout;
        moveSpeed = shootSpeed;
        moveDir = shootDir;
    }
    void SlowDown()
    {
        rb.velocity = moveDir * moveSpeed * Time.fixedDeltaTime;
        // slow down
        moveSpeed -= resistance * Time.fixedDeltaTime;
        if (moveSpeed <= 20)
        {
            rb.velocity = Vector3.zero;
            moveSpeed = 0;
            //become normal
            soulState = SoulState.normal;
        }
    }

    public void RecallFunction()
    {
        Debug.Log("Recall counting");
        if (canRecall){
            rb.velocity = Vector3.zero;
            //player.GetComponent<PlayerControl>().SuperDash(this.transform.position, soulDamage*5);
        }
    }
    // avoid keeping recalling when this hit obstruction
    public void ResetRecall()
    {
        canRecall = true;
    }

    void StopRecall()
    {
        soulState = SoulState.normal;
        rb.velocity = Vector3.zero;
        // stop recalling if player is pressing recall button
        if (Input.GetMouseButtonDown(1))
        {
            canRecall = false;
        }
    }
    //***************************Collide with enemy
    void RegularSoulHitEnemy(GameObject collision, float damage)
    {
        Enemy enemy = collision.transform.GetComponent<Enemy>();
        enemy.TakeDamage(damage, gameObject);

        //GameObject effct = Instantiate(impactEffect, transform.position, transform.rotation);
        //Destroy(effct, recoverLastTime);
    }
}
