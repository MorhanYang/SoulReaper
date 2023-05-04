using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Timeline;

public class MinionMoverMarker : MonoBehaviour
{
    float markertimer;
    [SerializeField] SpriteRenderer mysprite;

    private void Start(){
        mysprite.enabled= false;
    }

    private void Update()
    {
        if (markertimer >= 0) markertimer -= Time.deltaTime;

        if (markertimer <= 0) mysprite.enabled = false;
    }

    public void relocateMarker(Vector3 pos, Transform subject)
    {
        mysprite.enabled = true;

        // don't hit object
        if (subject == null)
        {
            transform.position = pos;
            transform.parent= null;
        }
        // hit object
        else
        {
            transform.position = subject.position;
            transform.parent = subject;
        }

        markertimer = 3f;
    }
}
