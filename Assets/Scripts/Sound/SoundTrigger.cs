using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] AudioSource MySoundSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null){
            StartCoroutine(AudioFade.FadeInFunction(MySoundSource, 2f));
        }
    }

    public void StopTriggeredSound(){
        StartCoroutine(AudioFade.FadeOutFunction(MySoundSource, 2f));  
    }
}
