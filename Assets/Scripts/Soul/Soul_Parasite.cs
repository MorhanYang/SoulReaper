using Fungus;
using UnityEngine;

public class Soul_Parasite : MonoBehaviour
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
    bool isInBody = false;
    
    [SerializeField] float resistance;
    [SerializeField] float recallSpeed = 10f;

    // deal damage
    public float soulDamage;
    [SerializeField] float damageInterval = 0.2f;
    float parasiteTimer;
    GameObject Hoster;
    bool parasited = false;// prevent parasite unpredictable items

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

        parasiteTimer = damageInterval;

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

        if (isInBody){
            ParasitingEnemy(Hoster);
        }
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

        // collide with enemy
        if (collision.transform.GetComponent<Enemy>() != null
            && !collision.transform.GetComponent<Enemy>().isDead)
        {
            // recall-> stop recall produce low damage. Then destory itself
            if (soulState == SoulState.recalling){

                if (!parasited){
                    RegularSoulHitEnemy(collision.gameObject, soulDamage);
                }
            }
            else { 
                if (!Attacked){
                    StickToEnemy(collision.gameObject);
                    Attacked = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider collision){
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
        if (canRecall){
            soulState = SoulState.recalling;
        }
    }
    // avoid keeping recalling when this hit obstruction
    public void ResetRecall()
    {
        if (!isInBody) canRecall = true;
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
        enemy.TakeDamage(damage);

    }

    void StickToEnemy(GameObject enemy)
    {
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled= false;
        rb.isKinematic= true;
        transform.position = new Vector3(transform.position.x, transform.position.y, enemy.transform.position.z - 0.2f);
        transform.parent = enemy.transform;
        isInBody= true;

        canRecall= false;

        Hoster = enemy;
    }

    void ParasitingEnemy(GameObject enemy) {
        if (enemy.GetComponent<Enemy>() != null){
            if (!enemy.GetComponent<Enemy>().isDead){
                if (parasiteTimer >= damageInterval)
                {
                    enemy.GetComponent<Enemy>().TakeDamage(soulDamage);
                    parasiteTimer = 0;

                    if (enemy.GetComponent<Enemy>().isDead){
                        StopParasiting();
                    }
                }
                else parasiteTimer += Time.deltaTime;
            }
            if (enemy.GetComponent<Enemy>().isDead) StopParasiting();

        }
    }

    public void StopParasiting()
    {
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        transform.parent = null;
        isInBody = false;
        Attacked = false;

        ResetRecall();
        parasited = true;
    }
}
