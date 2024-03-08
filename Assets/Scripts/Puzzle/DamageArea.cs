using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageArea : MonoBehaviour
{
    DamageManager myDamageManager;
    [SerializeField] float damage;
    [SerializeField] Puzzle_Bridge myBridge;
    [SerializeField] TMP_Text damageProgress;

    int damageCount = 1;
    

    private void Start()
    {
        myDamageManager = DamageManager.instance;
        damageProgress.text = damageCount + "/4";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>()!= null)
        {
            GetComponent<Collider>().enabled = false;

            myDamageManager.DealSingleDamage(transform, transform.position, other.transform, damage);
            myBridge.AddObject(1);
            damageCount++;
            damageProgress.text = damageCount + "/4";
            StartCoroutine(RestartDamage());
            
        }
    }

    IEnumerator RestartDamage()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<Collider>().enabled = true;
    }
}
