using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallingMinion : MonoBehaviour
{
    Transform player;
    Transform target;
    [SerializeField] float moveSpeed = 5f;

    Coroutine recallCoroutine;
    private void Start()
    {
        player = PlayerManager.instance.player.transform;
    }


    IEnumerator SetTarget(Transform destination)
    {
        target = destination;

        while (transform.position != target.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }


    public void AimTo( Transform minion)
    {
        recallCoroutine = StartCoroutine(SetTarget(minion));
    }
}
