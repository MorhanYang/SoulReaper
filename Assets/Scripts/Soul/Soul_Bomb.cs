using UnityEngine;

public class Soul_Bomb : MonoBehaviour
{
    Rigidbody rb;
    Vector3 moveDir;
    float moveSpeed;
    GameObject player;

    [HideInInspector]public int soulType;// reference GhostList & PlayerControl

    // Shoot
    [SerializeField] float shootSpeed;

    [SerializeField] GameObject impactEffect;

    //recall
    float presentRecallSpeed;
    bool canRecall = true;
    
    [SerializeField] float resistance;
    [SerializeField] float recallSpeed = 10f;
    [SerializeField] float recoverLastTime = 2f;


    public float soulDamage;
    [SerializeField] float explosionRadus = 2f;
    bool exploded;
    [SerializeField] LayerMask layerMask;

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

    private void OnCollisionEnter(Collision collision)
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

        // collide with enemy
        if (collision.transform.GetComponent<Enemy>() != null
            && !collision.transform.GetComponent<Enemy>().isDead)
        {
            if (!exploded){
                Explode(soulDamage);
                exploded = true;
            }
        }
    }

    // recall process
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Destroy(gameObject);
            // send soultype to playercontrol
            other.GetComponent<PlayerControl>().AddSoulList(soulType);
        }

        if (other.GetComponent<Enemy>() != null
            && !other.GetComponent<Enemy>().isDead) {
            if (soulState == SoulState.recalling) {
                RegularSoulHitEnemy(other.gameObject, soulDamage / 2);
            }
            
        }

    }


    //*******************************Method**********************************
    // reoverSoul After 3s delay
    void RecoverSoul()
    {
        exploded = false;
        gameObject.SetActive(true);
        // prevent execute recall when soul is disabled.
        StopRecall();
        GetComponent<Collider>().isTrigger = true;
        rb.useGravity = false;
    }

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
            if (!exploded){
                Explode(soulDamage);
            }
        }
    }


    public void RecallFunction()
    {
        if (canRecall){
            soulState = SoulState.recalling;
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
        // stop recalling if player is pressing E
        if (Input.GetMouseButtonDown(1))
        {
            canRecall = false;
        }
    }
    //***************************Collide with enemy
    void Explode(float damage)
    {
        moveSpeed = 0;
        soulState = SoulState.normal;

        // get all objects inside the radus
        Collider[] Enemies = Physics.OverlapSphere(transform.position, explosionRadus, layerMask);

        // play animation
        GameObject effct = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effct, recoverLastTime);

        // deal damage
        foreach (Collider nearbyEnemy in Enemies)
        {
            TakeExplosionDamage(nearbyEnemy.gameObject, damage);
        }

        gameObject.SetActive(false);
        Invoke("RecoverSoul", recoverLastTime);
    }

    void TakeExplosionDamage(GameObject collision , float damage)
    {
        Enemy enemy = collision.transform.GetComponent<Enemy>();
        if (enemy != null){
            enemy.TakeDamage(soulDamage, transform, transform.position);
        }
    }

    void RegularSoulHitEnemy(GameObject collision, float damage)
    {
        Enemy enemy = collision.transform.GetComponent<Enemy>();
        enemy.TakeDamage(damage, transform, transform.position);
    }
}
