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
    [SerializeField] Bait bait;
    [SerializeField] GameObject glowingCircuit;

    int presentNum;

    int objectsNeeded = 1;
    int objectCount = 0 ;

    Puzzle_Bridge myBridge;
    SoundManager mySoundManager;
    //Puzzle_MagicDoor myDoor;

    private void Start()
    {
        if (targetObject.GetComponent<Puzzle_Bridge>() != null) myBridge = targetObject.GetComponent<Puzzle_Bridge>();
        objectsNeeded = myBridge.GetNeedObjectNumber();

        mySoundManager = SoundManager.Instance;

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

                //show light
                if(glowingCircuit!= null) glowingCircuit.SetActive(true);
                // play sound
                mySoundManager.PlaySoundAt(transform.position, "MagicTrigger", false, false, 1.5f, 1f, 100, 100);
            }
            else{
                bait.gameObject.SetActive(false);// hide bait and stop trap minion if reach max
            }

            // UI display
            if (other.GetComponent<PlayerControl>() != null) objectCount += 1;
            if (other.GetComponent<Minion>() != null) objectCount += other.GetComponent<Minion>().minionSize;// ui don't care about the max
            text.text = objectCount + " / " + objectsNeeded;

            //// Move platform downward
            //if (objectCount < 5){
            //    platform.position = new Vector3(platform.position.x, platform.position.y - 0.01f, platform.position.z);
            //}
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

            if (objectCount < presentNum && presentNum > 0) // only when trigger don't have enough minion functrion start to detract
            {
                Debug.Log("Detract");
                if (other.GetComponent<Minion>() != null && other.GetComponent<Minion>().minionSize > 1){
                    int size = other.GetComponent<Minion>().minionSize;

                    if (size > presentNum){
                        detractNum = presentNum;
                    }
                    else detractNum = size;

                    presentNum -= detractNum;
                }
                else{
                    presentNum -= detractNum;
                }

                if (myBridge != null) myBridge.DetractObject(detractNum);
                bait.gameObject.SetActive(true);

                //hide light
                if (glowingCircuit != null) glowingCircuit.SetActive(false);
            }

            //// Move platform upward
            //if (objectCount >= 0 ){
            //    platform.position = new Vector3(platform.position.x, platform.position.y + 0.01f, platform.position.z);
            //}

        }
    }
}
