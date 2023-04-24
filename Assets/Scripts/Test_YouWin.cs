using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test_YouWin : MonoBehaviour
{
    [SerializeField] GameObject WinUI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null){
            // you win
            SceneManager.LoadScene("CreditScene");
        }
    }
}
