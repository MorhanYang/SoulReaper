using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    Rigidbody rb;
    Vector3 moveDir;
    float moveSpeed;
    GameObject player;
    //recall
    float presentRecallSpeed;
    bool canRecall = true;

    [SerializeField] float resistance;
    [SerializeField] float recallSpeed = 10f;

    public int soulType;// 0-Normal,1-special,
    public int soulDamage;
    [SerializeField] GameObject impactEffect;

    enum SoulState
    {
        shootingout,
        normal,
        recalling,
    }
    SoulState soulState= SoulState.normal;


    private void Start()
    {
        rb= GetComponent<Rigidbody>();
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
                if (presentRecallSpeed < recallSpeed)
                {
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
        if (collision.transform.tag == "Obstruction"){
            StopRecall();
        }

        // collide with enemy
        if (collision.transform.GetComponent<Enemy>() != null
            && !collision.transform.GetComponent<Enemy>().isDead)
        {
            float effectLast = 2f;
            // shoot -> bounce back and destory itself
            if (moveSpeed > 100f)
            {
                Enemy enemy = collision.transform.GetComponent<Enemy>();
                enemy.TakeDamage(soulDamage);
                // Bounce Back
                moveDir *= -1;
                moveSpeed = 100f;

                gameObject.SetActive(false);
                GameObject effct = Instantiate(impactEffect, transform.position, transform.rotation);
                Destroy(effct, effectLast);
                Invoke("RecoverSoul", effectLast);
            }

            // recall-> stop recall produce low damage. Then destory itself
            if (soulState == SoulState.recalling)
            {
                StopRecall();

                Enemy enemy = collision.transform.GetComponent<Enemy>();
                enemy.TakeDamage(soulDamage/2);

                gameObject.SetActive(false);
                GameObject effct = Instantiate(impactEffect, transform.position, transform.rotation);
                Destroy(effct, effectLast);
                Invoke("RecoverSoul", effectLast);
            }

        }
    }

    //*******************************Method**********************************
    // reoverSoul After 3s delay
    void RecoverSoul()
    {
        gameObject.SetActive(true);
    }

    public void ShootSoul(Vector3 shootDir ,float shootSpeed){
        soulState= SoulState.shootingout;
        moveSpeed= shootSpeed;
        moveDir= shootDir;
    }
    void SlowDown() {
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


    public void RecallFunction(){
        if (canRecall){
            soulState = SoulState.recalling;
        }
    }
    // avoid keeping recalling when this hit obstruction
    public void ResetRecall(){
        canRecall = true;
    }

    void StopRecall()
    {
        soulState = SoulState.normal;
        rb.velocity = Vector3.zero;
        // stop recalling if player is pressing E
        if (Input.GetKey(KeyCode.E))
        {
            canRecall = false;
        }
    }


}
