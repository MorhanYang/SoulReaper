using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bait : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MinionAI>() != null){
            MinionAI myAi = other.GetComponent<MinionAI>();
            if (myAi.minionState == MinionAI.MinionSate.Sprint){
                myAi.minionState = MinionAI.MinionSate.Bait;
                Debug.Log("Set to Bait");
            }
            

        }
    }
}
