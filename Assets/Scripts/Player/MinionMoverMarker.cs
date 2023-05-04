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

        if (markertimer <= 0) {
            transform.parent = null;
            mysprite.enabled = false; }
    }

    public void relocateMarker(Vector3 pos, Transform subject)
    {
        float lastTime = 1.5f;

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

        markertimer = lastTime;
    }
}
