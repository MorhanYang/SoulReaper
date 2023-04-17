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

    int objectsNeeded = 1;
    int objectCount = 0 ;

    Puzzle_Bridge myBridge;
    //Puzzle_MagicDoor myDoor;

    float UITimer;

    private void Start()
    {
        if (targetObject.GetComponent<Puzzle_Bridge>() != null) myBridge = targetObject.GetComponent<Puzzle_Bridge>();
        objectsNeeded = myBridge.GetNeedObjectNumber();
    }
    private void Update()
    {
        // hide UI after a few seconds 
        if (UITimer > 0f) UITimer -= Time.deltaTime;
        if (myCanvas.gameObject.activeSelf && UITimer <= 0){
            myCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int addNum = 1;
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            if (other.GetComponent<Minion>() != null && other.GetComponent<Minion>().minionSize > 1){
                addNum = other.GetComponent<Minion>().minionSize;
                objectCount += addNum;
            }
            else objectCount+= addNum;

            // Show UI
            myCanvas.gameObject.SetActive(true);
            text.text = objectCount + " / " + objectsNeeded;
            UITimer = 5f;

            // Move platform downward
            if (objectCount < 5){
                platform.position = new Vector3(platform.position.x, platform.position.y - 0.01f, platform.position.z);
            }

            if (myBridge != null) myBridge.AddObject(addNum);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int detractNum = 1;
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            if (other.GetComponent<Minion>() != null && other.GetComponent<Minion>().minionSize > 1){
                detractNum = other.GetComponent<Minion>().minionSize;
                objectCount -= detractNum;
            }
            else objectCount -= detractNum;

            // Show UI
            myCanvas.gameObject.SetActive(true);
            text.text = objectCount + " / " + objectsNeeded;
            UITimer = 5f;

            // Move platform upward
            if (objectCount >= 0 ){
                platform.position = new Vector3(platform.position.x, platform.position.y + 0.01f, platform.position.z);
            }


            if (myBridge != null) myBridge.DetractObject(detractNum);
        }
    }
}
