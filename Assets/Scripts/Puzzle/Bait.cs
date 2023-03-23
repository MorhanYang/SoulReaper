using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bait : MonoBehaviour
{
    bool canBait;
    private void OnTriggerEnter(Collider other)
    {
        canBait= true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (canBait){
            if (other.GetComponent<MinionAI>() != null)
            {
                MinionAI myAi = other.GetComponent<MinionAI>();
                if (myAi.minionState == MinionAI.MinionSate.Wait)
                {
                    myAi.SetToBait();
                    Debug.Log("Set to Bait");
                    canBait= false;
                }
            }
        }
    }
}
