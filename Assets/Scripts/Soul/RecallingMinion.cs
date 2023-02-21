using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallingMinion : MonoBehaviour
{
    Transform player;
    [SerializeField] float moveSpeed = 0.05f;
    private void Start()
    {
        player = PlayerManager.instance.player.transform;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player"){
            Destroy(gameObject);
        }
    }
}
