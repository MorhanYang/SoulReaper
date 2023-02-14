using Fungus;
using System.Collections;
using UnityEngine;

public class Soul_Kite : MonoBehaviour
{
    Rigidbody rb;
    Vector3 moveDir;
    float moveSpeed;
    GameObject player;

    public int soulType;// reference GhostList & PlayerControl

    // Shoot
    [SerializeField] float shootSpeed;
    bool attacked = false;

    [SerializeField] GameObject impactEffect;

    //recall
    float presentRecallSpeed;
    [SerializeField] float initalrecallSpeed = 0.3f;
    [SerializeField] float recallResistance = 1.5f;
    bool canRecall = true;
    
    [SerializeField] float resistance;
    [SerializeField] float recallSpeed = 10f;

    //damage
    public float soulDamage;
    [SerializeField] float maxExistTime = 4f;
    float existTimer = 0;
    float damageTimer = 0;


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

        // soul set up
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
        damageTimer = 0;
    }

    private void FixedUpdate()
    {
        switch (soulState)
        {
            case SoulState.normal:
                //reset recall speed
                presentRecallSpeed = initalrecallSpeed;
                break;
            case SoulState.shootingout:
                SlowDown();
                break;
            case SoulState.recalling:
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, presentRecallSpeed * Time.fixedDeltaTime);
                if (presentRecallSpeed < recallSpeed){
                    presentRecallSpeed += recallResistance * Time.fixedDeltaTime;
                }
                break;
        }

        // exist timer
        if (existTimer <= maxExistTime){
            existTimer += Time.fixedDeltaTime;
        }
        else attacked = true;
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.tag == "Player")
        {
            Destroy(gameObject);
            // send soultype to playercontrol
            collision.transform.GetComponent<PlayerControl>().AddSoulList(soulType);
        }

        // collide with walls
        if (collision.transform.tag == "Obstruction")
        {
            StopRecall();
        }
    }

    private void OnTriggerStay(Collider other)
    {

        // collide with enemy
        if (other.GetComponent<Enemy>() != null
            && !other.GetComponent<Enemy>().isDead)
        {
            if (!attacked){
                KiteLikeSoulHitEnemy(other.gameObject, soulDamage);
            }
        }
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
        if (moveSpeed <= 20){
            rb.velocity = Vector3.zero;
            moveSpeed = 0;
            //become normal
            soulState = SoulState.normal;
        }
    }


    public void RecallFunction()
    {
        if (canRecall){
            soulState = SoulState.recalling;
            // reset attack
            attacked = false;
            existTimer = 0;
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
    void KiteLikeSoulHitEnemy(GameObject collision, float damage)
    {
        Enemy enemy = collision.transform.GetComponent<Enemy>();

        float interval = 0.2f;
        if (damageTimer >= interval) {
            enemy.TakeDamage(damage);
            damageTimer = 0;
        }
        
        damageTimer += Time.deltaTime;
    }
}
