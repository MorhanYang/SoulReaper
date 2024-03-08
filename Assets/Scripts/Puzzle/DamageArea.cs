using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    DamageManager myDamageManager;
    [SerializeField] float damage;

    private void Start()
    {
        myDamageManager = DamageManager.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>()!=null)
        {
            myDamageManager.DealSingleDamage(transform, transform.position, other.transform, damage);
            StartCoroutine(RestartDamage());
            this.enabled = false;
        }
    }

    IEnumerator RestartDamage()
    {
        yield return new WaitForSeconds(3f);
        this.enabled = true;
    }
}
