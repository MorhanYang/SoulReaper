using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    [SerializeField] GameObject recallingMinion;

    public int MinionType = 0; // refer to MinionTroop;
    int listBelonging;



    private void Update()
    {
        // health bar

    }

    //*********************************************************Method*******************************************************
    public void SetListId( int listId )
    {
        listBelonging = listId;
    }

    public void RecallMinion()
    {
        Debug.Log("Recall");
        Instantiate(recallingMinion,transform.position,transform.rotation);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {



    }

}
