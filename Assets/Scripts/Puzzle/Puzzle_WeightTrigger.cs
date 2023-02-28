using UnityEngine;
using TMPro;

public class Puzzle_WeightTrigger : MonoBehaviour
{
    [SerializeField] Puzzle_MagicDoor myDoor;
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
            if (objectCount < objectsNeeded)
            {
                objectCount++;
                // move trigger platform downward
                transform.position = new Vector3(transform.position.x, transform.position.y - (0.05f/ objectsNeeded), transform.position.z);
                // open door
                if (objectCount >= objectsNeeded) myDoor.ActiveScript();

                // Show UI
                myCanvas.gameObject.SetActive(true);
                text.text = objectCount + " / " + objectsNeeded;
                UITimer = 4f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Minion>() != null || other.GetComponent<PlayerControl>() != null)
        {
            if (objectCount>0)
            {
                objectCount--;
                // move trigger platform upward
                transform.position = new Vector3(transform.position.x, transform.position.y + (0.05f / objectsNeeded), transform.position.z);
                // open door
                if (objectCount < objectsNeeded) myDoor.CancelScript();

                // Show UI
                myCanvas.gameObject.SetActive(true);
                text.text = objectCount + " / " + objectsNeeded;
                UITimer = 4f;
            }
        }
    }
}
