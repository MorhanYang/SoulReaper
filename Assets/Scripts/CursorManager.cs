using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] LayerMask layermasks;
    void Update()
    {
        ShootDetectiveRay();
    }

    void ShootDetectiveRay()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        GameObject hit;
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, layermasks))
        {
            if(hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("FollowingSoul"))
            {

            }
            hit = hitInfo.collider.gameObject;
        }
        else hit = null; // hit nothing
    }
}
