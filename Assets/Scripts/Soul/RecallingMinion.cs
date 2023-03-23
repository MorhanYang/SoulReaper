using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallingMinion : MonoBehaviour
{
    Transform target;
    [SerializeField] float moveSpeed = 5f;

    Coroutine recallCoroutine;

    IEnumerator SetTarget(Transform destination)
    {
        target = destination;

        while (Vector3.Distance(transform.position, target.position) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        // play sound
        SoundManager mySoundManager = SoundManager.Instance;
        mySoundManager.PlaySoundAt(mySoundManager.transform.position, "Heal", false, false, 1, 1, 100, 100);


        Destroy(gameObject);
    }


    public void AimTo(Transform minion)
    {
        recallCoroutine = StartCoroutine(SetTarget(minion));
    }
}
