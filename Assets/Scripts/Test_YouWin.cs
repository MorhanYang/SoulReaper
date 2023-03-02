using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_YouWin : MonoBehaviour
{
    [SerializeField] GameObject WinUI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null){
            // you win
            WinUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
