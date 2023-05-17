using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Make camera follow player and mouse;
    [SerializeField] GameObject target;
    [SerializeField] float camSpeed;

    [SerializeField] Vector3 camOffset;
    [SerializeField] Vector3 MouseOffset;

    void Update()
    {
        MouseOffset.x = Mathf.Clamp((Mathf.Abs(Input.mousePosition.x - Screen.width / 2) - Screen.width / 4) / Screen.width * 8, 0 , (Screen.width * 0.1f) / (Screen.height * 0.2f)) * Mathf.Sign(Input.mousePosition.x - Screen.width / 2);
        MouseOffset.z = Mathf.Clamp((Mathf.Abs(Input.mousePosition.y - Screen.height / 2) - Screen.height / 4) / Screen.height * 8, 0 , (Screen.height * 0.1f) / (Screen.width * 0.1f)) * Mathf.Sign(Input.mousePosition.y - Screen.height / 2);

        Debug.Log("Width " + Screen.width);
        Debug.Log("Height " + Screen.height);
        Debug.Log(Screen.width / Screen.height);

    }

    private void FixedUpdate()
    {
        Vector3 nextPos = target.transform.position + camOffset + MouseOffset;
        transform.position = Vector3.Lerp(transform.position, nextPos, camSpeed * Time.fixedDeltaTime);
    }
    public Vector3 GetCamOffset()
    {
        return camOffset;
    }
}
