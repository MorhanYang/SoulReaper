using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Vine : MonoBehaviour
{
    [SerializeField] VinesSingle[] singleVineList;
    [SerializeField] int objectsNeeded = 1;
    [SerializeField] GameObject Blocker;

    int objectCounter;

    // ************************************************Method*************************************************

    public int GetNeedObjectNumber()
    {
        return objectsNeeded;
    }


    public void AddObject(int size)
    {
        objectCounter += size;
        if (objectCounter >= objectsNeeded)
        {
            OpenBlocker();
        }
    }

    public void DetractObject(int size)
    {
        objectCounter -= size;// solve multiple trigger problem
        if (objectCounter < 0) objectCounter = 0;// just in case

        if (objectCounter < objectsNeeded)
        {
            gameObject.SetActive(true);// setActive then script can run

            // Close Blocker
            CloseBlocker();
        }
    }

    public void OpenBlocker()
    {
        Debug.Log("OpenBlocker");
        // open blocker
        if (Blocker != null) Blocker.SetActive(false);

        // show vines
        for (int i = 0; i < singleVineList.Length; i++)
        {
            singleVineList[i].StartSpawnVine();
        }
    }
    public void CloseBlocker()
    {
        if (Blocker != null) Blocker.SetActive(true);

        // show vines
        for (int i = 0; i < singleVineList.Length; i++)
        {
            singleVineList[i].StartKillVine();
        }
    }
}
