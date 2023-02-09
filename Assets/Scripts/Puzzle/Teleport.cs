using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] GameObject teleportObject;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            teleportObject.transform.position = new Vector3(2f,0,0);
        } 
    }
}
