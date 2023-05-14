using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackTest : MonoBehaviour
{
    DamageManager myDamageManager;

    private void Start()
    {
        myDamageManager = DamageManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(myDamageManager.gameObject.name);
            myDamageManager.DealAOEDamage(null,transform.position,2f,10f);
        }
    }
}
