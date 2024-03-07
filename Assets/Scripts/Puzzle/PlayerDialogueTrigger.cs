using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueTrigger : MonoBehaviour
{
    PlayerDialogue myPlayerDialogue;
    PlayerControl myPlayerControl;

    [SerializeField] string content;
    [SerializeField] float lastTime;


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null)
        {
            myPlayerDialogue = other.GetComponent<PlayerDialogue>();
            myPlayerControl = other.GetComponent<PlayerControl>();
            myPlayerDialogue.ShowPlayerCall(content, lastTime);
            myPlayerControl.canMove = false;

            StartCoroutine(RegainMovement());
        }
    }

    IEnumerator RegainMovement()
    {
        yield return new WaitForSeconds(lastTime - 1f);
        myPlayerControl.canMove = true;
        Destroy(this);
    }
}
