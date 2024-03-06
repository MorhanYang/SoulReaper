using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDialogue : MonoBehaviour
{
    public TMP_Text playerCall;

    private void Start()
    {
        playerCall.text = null;
    }
    public void ShowPlayerCall( string content )
    {
        if (playerCall.text == null)
        {
            playerCall.text = content;
            StartCoroutine(CleanPlayerCall());
        }
    }

    IEnumerator CleanPlayerCall()
    {
        yield return new WaitForSeconds(1.5f);
        playerCall.text = null;
    }
}
