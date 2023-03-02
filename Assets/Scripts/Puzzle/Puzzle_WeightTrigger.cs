using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class Puzzle_WeightTrigger : MonoBehaviour
{
    [SerializeField] Puzzle_MagicDoor myDoor;
    [SerializeField] Transform platform;
    [SerializeField] Canvas myCanvas;
    [SerializeField] TMP_Text text;

    [SerializeField] int objectsNeeded = 2;
    int objectCount = 0 ;

    float UITimer;
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
            if (objectCount >= objectsNeeded) myDoor.ActiveScript();
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
            if (objectCount < objectsNeeded) myDoor.CancelScript();
        }
    }

    int CheckObjectInside()
    {
        Collider[] allObjects = Physics.OverlapBox(
            transform.position + new Vector3(0, 0.7f, 0), 
            new Vector3(0.4f, 0.3f, 0.4f), 
            transform.rotation, 
            LayerMask.GetMask("Player","Minion","MovingMinion"));
        return allObjects.Length;
    }
}
