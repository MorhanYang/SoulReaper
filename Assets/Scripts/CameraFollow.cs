using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    GameObject target;
    [SerializeField] float camSpeed;

    [SerializeField] Vector3 camOffset;

    private void Start()
    {
        target = PlayerManager.instance.player;
        transform.position = target.transform.position + camOffset;
    }
    private void FixedUpdate()
    {
        Vector3 nextPos = target.transform.position + camOffset;
        transform.position = Vector3.Lerp(transform.position, nextPos, camSpeed * Time.fixedDeltaTime);
    }
    public Vector3 GetCamOffset()
    {
        return camOffset;
    }
}
