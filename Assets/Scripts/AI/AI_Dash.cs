using MoonSharp.VsCodeDebugger.SDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Dash : MonoBehaviour
{
    // dash
    [SerializeField] GameObject dashIndicator_Axis;
    [SerializeField] GameObject DashIndicator_Bar;
    [SerializeField] SpriteRenderer enemySprite;
    [SerializeField] float dashSpeed = 8f;
    [SerializeField] float prepareTimeDashing = 1.6f;
    float dashDelayTime = 0;
    Vector3 dashDir;
    float presentDashSpeed;

    List<Collider> DamagedTarget = new List<Collider>();
    NavMeshAgent agent;

    SoundManager mySoundManager;
    bool dashed = false;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        presentDashSpeed = dashSpeed;

        mySoundManager = SoundManager.Instance;
    }


    //************************************************************************ Dash *************************************************************
    public void PrepareDash(Transform target)
    {
        dashDelayTime = Time.time;// delay timer

        agent.SetDestination(transform.position);// stop moving

        dashDir = target.position - transform.position;
        dashDir.Normalize();

        //display
        enemySprite.color = Color.red;
        dashIndicator_Axis.SetActive(true);
        float angle = Mathf.Atan2(dashDir.z, dashDir.x) * Mathf.Rad2Deg;
        Quaternion DashRoatation = Quaternion.Euler(new Vector3(0, -angle, 0));
        dashIndicator_Axis.transform.rotation = DashRoatation;

        mySoundManager.PlaySoundAt(mySoundManager.transform.position, "DashPrepare", false, false, 1.5f, 1, 100, 100);
    }

    public bool EnemyDashing(float damage)
    {
        //display: the bar shows time count down
        Vector3 barScale = DashIndicator_Bar.transform.localScale;
        barScale.x = (Time.time - dashDelayTime) / prepareTimeDashing;
        DashIndicator_Bar.transform.localScale = barScale;

        // dash
        if ((Time.time - dashDelayTime) >= prepareTimeDashing)
        {

            // display
            dashIndicator_Axis.SetActive(false);

            // dash movement
            float dashResistance = 1.2f * dashSpeed;
            agent.Move(dashDir * presentDashSpeed * Time.deltaTime);
            presentDashSpeed -= dashResistance * Time.deltaTime;

            // deal damage
            Collider[] hitedObjecct = Physics.OverlapSphere((transform.position + dashDir * 0.2f), 0.14f, LayerMask.GetMask("Player", "MovingMinion"));
            // don't use Minion layermask because the moving minion is not in Minion 
            for (int i = 0; i < hitedObjecct.Length; i++)
            {
                if (hitedObjecct[i].GetComponent<PlayerControl>() != null)
                {
                    hitedObjecct[i].GetComponent<PlayerControl>().PlayerTakeDamage(damage, transform);
                    //recuce damge after hit an object
                    damage = (int)(damage * 0.5f) + 1;
                }
                else if (!DamagedTarget.Contains(hitedObjecct[i]))
                {
                    Minion hitedMinion = hitedObjecct[i].GetComponent<Minion>();
                    hitedMinion.TakeDamage(damage, transform, transform.position);
                    hitedMinion.GetComponent<MinionAI>().SetToFaint();
                    DamagedTarget.Add(hitedObjecct[i]);
                    Debug.Log("Deal Damage: " + damage);
                    // recuce damge after hit an object
                    damage = (int)(damage * 0.5f) + 1;
                }
            }
            // play sound
            if (hitedObjecct.Length > 0 && !dashed)
            {
                mySoundManager.PlaySoundAt(mySoundManager.transform.position, "DashHit", false, false, 1.5f, 1, 100, 100);
                dashed = true;
            }


            // End dashing
            if (presentDashSpeed <= 0.5f)
            {
                enemySprite.color = Color.white;

                // reset the property
                presentDashSpeed = dashSpeed;
                DamagedTarget.Clear();
                dashed= false;

                return false; // isn't dashing
            }
        }
        return true;
    }

     public bool MinionDashing(float damage)
    {
        //display: the bar shows time count down
        Vector3 barScale = DashIndicator_Bar.transform.localScale;
        barScale.x = (Time.time - dashDelayTime) / prepareTimeDashing;
        DashIndicator_Bar.transform.localScale = barScale;

        // dash
        if ((Time.time - dashDelayTime) >= prepareTimeDashing)
        {
            // display
            dashIndicator_Axis.SetActive(false);

            // dash movement
            float dashResistance = 1.2f * dashSpeed;
            agent.Move(dashDir * presentDashSpeed * Time.deltaTime);
            presentDashSpeed -= dashResistance * Time.deltaTime;

            // deal damage
            Collider[] hitedObjecct = Physics.OverlapSphere((transform.position + dashDir * 0.2f), 0.14f, LayerMask.GetMask("Enemy"));
            for (int i = 0; i < hitedObjecct.Length; i++)
            {
                if (!DamagedTarget.Contains(hitedObjecct[i])){
                    hitedObjecct[i].GetComponent<EnemyScript>().TakeDamage(damage, transform, transform.position);
                    DamagedTarget.Add(hitedObjecct[i]);
                }
            }

            // play sound
            if (hitedObjecct.Length > 0 && !dashed)
            {
                mySoundManager.PlaySoundAt(mySoundManager.transform.position, "DashHit", false, false, 1.5f, 1, 100, 100);
                dashed = true;
            }

            // End dashing
            if (presentDashSpeed <= 0.5f)
            {
                enemySprite.color = Color.white;

                Debug.Log("End Dash");
                // reset the property
                presentDashSpeed = dashSpeed;
                DamagedTarget.Clear();

                return false; // isn't dashing
            }
        }

        return true;
    }

    public void CancelDashing()
    {
        enemySprite.color = Color.white;

        Debug.Log("End Dash");
        // reset the property
        presentDashSpeed = dashSpeed;
        DamagedTarget.Clear();
        dashIndicator_Axis.SetActive(false);
    }
}
