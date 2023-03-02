using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bait : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<MinionAI>() != null){
            MinionAI myAi = other.GetComponent<MinionAI>();
            myAi.SetToWait();
        }
    }
}
