using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class Puzzle_WeightTrigger : MonoBehaviour
{
    [SerializeField] Transform targetObject;
    [SerializeField] Transform platform;
    [SerializeField] Canvas myCanvas;
    [SerializeField] TMP_Text text;
    [SerializeField] int maxNum;
    int presentNum;

    int objectsNeeded = 1;
    int objectCount = 0 ;

    Puzzle_Bridge myBridge;
    //Puzzle_MagicDoor myDoor;

    private void Start()
    {
        if (targetObject.GetComponent<Puzzle_Bridge>() != null) myBridge = targetObject.GetComponent<Puzzle_Bridge>();
        objectsNeeded = myBridge.GetNeedObjectNumber();

        presentNum = 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        int addNum = 1;
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            if (presentNum < maxNum)
            {
                if (other.GetComponent<Minion>() != null && other.GetComponent<Minion>().minionSize > 1)
                {
                    int size = other.GetComponent<Minion>().minionSize;

                    if (size > maxNum - presentNum){
                        addNum = maxNum - presentNum;
                    }
                    else addNum = size;

                    presentNum += addNum;
                }
                else{
                    presentNum += addNum;
                }


                if (myBridge != null) myBridge.AddObject(addNum);
            }

            // UI display
            if (other.GetComponent<PlayerControl>() != null) objectCount += 1;
            if (other.GetComponent<Minion>() != null) objectCount += other.GetComponent<Minion>().minionSize;// ui don't care about the max
            text.text = objectCount + " / " + objectsNeeded;

            // Move platform downward
            if (objectCount < 5){
                platform.position = new Vector3(platform.position.x, platform.position.y - 0.01f, platform.position.z);
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        int detractNum = 1;

        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            // UI display
            if (other.GetComponent<PlayerControl>() != null) objectCount -= 1;
            if (other.GetComponent<Minion>() != null) objectCount -= other.GetComponent<Minion>().minionSize;// ui don't care about the max
            text.text = objectCount + " / " + objectsNeeded;

            if (objectCount <= presentNum && presentNum > 0) // only when trigger don't have enough minion functrion start to detract
            {
                Debug.Log("Detract");
                if (other.GetComponent<Minion>() != null && other.GetComponent<Minion>().minionSize > 1)
                {
                    int size = other.GetComponent<Minion>().minionSize;

                    if (size > presentNum)
                    {
                        detractNum = presentNum;
                    }
                    else detractNum = size;

                    presentNum -= detractNum;
                }
                else
                {
                    presentNum -= detractNum;
                }


                if (myBridge != null) myBridge.DetractObject(detractNum);
            }

            // Move platform upward
            if (objectCount >= 0 ){
                platform.position = new Vector3(platform.position.x, platform.position.y + 0.01f, platform.position.z);
            }

        }
    }
}
