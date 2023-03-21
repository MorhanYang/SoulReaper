using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBlocker : MonoBehaviour
{
    [SerializeField] GameObject[] blockers;

    private void Start()
    {
        ChangeBlockersState(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ChangeBlockersState(true);
            Destroy(GetComponent<Collider>());
        }
    }

    // ******************************************** Method *********************************************
    public void ChangeBlockersState(bool state)
    {
        for (int i = 0; i < blockers.Length; i++)
        {
            blockers[i].SetActive(state);
        }
    }
}
