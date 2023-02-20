using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameObject[] EnemyList;

    public Flowchart fungusFlowchart;

    private void Start()
    {
        EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
    }

}
