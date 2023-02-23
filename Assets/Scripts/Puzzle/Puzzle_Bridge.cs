using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Bridge : MonoBehaviour
{
    [SerializeField] Collider airWall;
    [SerializeField] float moveSpeed = 1f;

    int isActived = 0;
    private void Update()
    {
        if (isActived == 1) MoveBridge();
    }

    public void ActiveScript()
    {
        Debug.Log("active script");
        isActived ++;
    }

    void MoveBridge()
    {
        Vector3 destination = new Vector3(transform.position.x, -0.5f, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
        // reach destination
        if (Vector3.Distance(transform.position, destination) < 0.02f){
            airWall.enabled = false;
            isActived++;
        }
    }
}
