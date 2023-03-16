using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintUITrigger : MonoBehaviour
{
    [SerializeField] HintUI myHint;
    
    private void OnTriggerEnter(Collider other)
    {
        Time.timeScale = 0;
        myHint.ShowHint();

        Destroy(gameObject);
    }
}
