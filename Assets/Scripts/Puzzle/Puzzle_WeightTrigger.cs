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

    [SerializeField] int objectsNeeded = 2;
    int objectCount = 0 ;

    Puzzle_Bridge myBridge;
    Puzzle_MagicDoor myDoor;

    float UITimer;

    private void Start()
    {
        if (targetObject.GetComponent<Puzzle_Bridge>() != null) myBridge = targetObject.GetComponent<Puzzle_Bridge>();
        if (targetObject.GetComponent<Puzzle_MagicDoor>() != null) myDoor = targetObject.GetComponent<Puzzle_MagicDoor>();
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
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            objectCount++;
            // Show UI
            myCanvas.gameObject.SetActive(true);
            text.text = objectCount + " / " + objectsNeeded;
            UITimer = 5f;

            // Move platform downward
            if (objectCount < 5){
                platform.position = new Vector3(platform.position.x, platform.position.y - 0.01f, platform.position.z);
            }

            // Open door
            if (objectCount >= objectsNeeded)
            {
                if(myDoor != null) myDoor.ActiveScript();
                if(myBridge != null) myBridge.ActiveScript();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            objectCount--;
            // Show UI
            myCanvas.gameObject.SetActive(true);
            text.text = objectCount + " / " + objectsNeeded;
            UITimer = 5f;

            // Move platform upward
            if (objectCount >= 0 ){
                platform.position = new Vector3(platform.position.x, platform.position.y + 0.01f, platform.position.z);
            }

            // Close door
            if (objectCount < objectsNeeded)
            {
                if (myDoor != null) myDoor.CancelScript();
                if (myBridge != null) myBridge.CancelScript();
            }
        }
    }
}
