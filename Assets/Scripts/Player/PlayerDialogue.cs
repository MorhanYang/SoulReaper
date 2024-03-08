using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDialogue : MonoBehaviour
{
    public TMP_Text playerCall;
    float myLastTime;

    private void Start()
    {
        playerCall.text = null;
    }
    public void ShowPlayerCall( string content, float lastTime)
    {
        if (playerCall.text == null)
        {
            playerCall.text = content;
            myLastTime = lastTime;
            StartCoroutine(CleanPlayerCall());
        }
    }

    IEnumerator CleanPlayerCall()
    {
        yield return new WaitForSeconds(myLastTime);
        playerCall.text = null;
    }
}
