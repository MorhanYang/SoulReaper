using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach this script to "Enemy" under "EnemyPrefab" to make it look towards the direction it moves at.
public class LookAtWhereItGoes : MonoBehaviour
{
    [SerializeField]
    EnemyBasicAi MyEnemyAi;
    void Start()
    {
        if (gameObject.GetComponent<EnemyBasicAi>() != null)
        {
            MyEnemyAi = gameObject.GetComponent<EnemyBasicAi>();
        }
        else
        {
            Debug.Log("No enemy ai found on object");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
