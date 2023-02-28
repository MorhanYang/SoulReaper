using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_MagicDoor : MonoBehaviour
{
    [SerializeField] SpriteRenderer myDoor;

    enum ScriptState
    {
        Regular,
        Active,
        Cancel,  
    }
    ScriptState state = ScriptState.Regular;

    Vector3 initalPos;
    Vector3 destination;

    private void Start()
    {
        initalPos= transform.position;
    }
    private void Update()
    {
        if (state == ScriptState.Active)
        {
            Debug.Log("active");
            transform.position = Vector3.MoveTowards(transform.position, destination, 0.3f * Time.deltaTime);
            if (transform.position == destination){
                gameObject.SetActive(false);
                state = ScriptState.Regular;
            }
        }

        if (state == ScriptState.Cancel)
        {
            Debug.Log("Cancel");
            transform.position = Vector3.MoveTowards(transform.position, destination, 0.3f * Time.deltaTime);
            if (transform.position == destination)
            {
                state = ScriptState.Regular;
            }
        }

    }

    public void ActiveScript()
    {
        if (state == ScriptState.Regular){
            destination = transform.position + new Vector3(0f, 0.1f, 0.3f);
            state = ScriptState.Active;
        }
    }

    public void CancelScript()
    {
        if (state == ScriptState.Regular){
            gameObject.SetActive(true);// setActive then script can run
            destination = initalPos;
            state = ScriptState.Cancel;
        }
    }
}
