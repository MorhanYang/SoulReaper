using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] float camSpeed;

    [SerializeField] Vector3 camOffset = new Vector3(0, 3f, -2.8f);
    private void FixedUpdate()
    {
        Vector3 nextPos = target.transform.position + camOffset;
        transform.position = Vector3.Lerp(transform.position, nextPos, camSpeed * Time.fixedDeltaTime);
    }
}
