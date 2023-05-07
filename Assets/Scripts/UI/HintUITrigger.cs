using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintUITrigger : MonoBehaviour
{
    [SerializeField] HintUI myHint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null)
        {
            myHint.ShowHint();

            Destroy(gameObject);
        }

    }
}
