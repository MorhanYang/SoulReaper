using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallingMinion : MonoBehaviour
{
    Transform target;
    [SerializeField] float moveSpeed = 5f;
    Transform player;
    SoundManager mySoundManager;

    Coroutine recallCoroutine;

    private void Start()
    {
        mySoundManager = SoundManager.Instance;
        player = PlayerManager.instance.player.transform;
    }
    IEnumerator SetTarget(Transform destination)
    {
        target = destination;

        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        // play sound
        if (destination.tag == "Player"){
            mySoundManager.PlaySoundAt(mySoundManager.transform.position, "Heal", false, false, 1, 1, 100, 100);
        }
        else{
            //mySoundManager.PlaySoundAt(mySoundManager.transform.position, "Revive", false, false, 1, 1, 100, 100);
        }

        Destroy(gameObject);
    }


    public void AimTo(Transform minion)
    {
        recallCoroutine = StartCoroutine(SetTarget(minion));
    }
}
