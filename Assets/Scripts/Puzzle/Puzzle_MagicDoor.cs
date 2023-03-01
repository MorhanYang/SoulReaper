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
        switch (state)
        {
            case ScriptState.Regular:
                break;

            case ScriptState.Active:
                transform.position = Vector3.MoveTowards(transform.position, destination, 0.4f * Time.deltaTime);
                if (transform.position == destination){
                    gameObject.SetActive(false);
                    state = ScriptState.Regular;
                }
                break;

            case ScriptState.Cancel:
                transform.position = Vector3.MoveTowards(transform.position, destination, 0.4f * Time.deltaTime);
                if (transform.position == destination){
                    state = ScriptState.Regular;
                }
                break;
        }

    }

    public void ActiveScript()
    {
        destination = transform.position + new Vector3(0f, 0.1f, 0.3f);
        state = ScriptState.Active;
    }

    public void CancelScript()
    {
        gameObject.SetActive(true);// setActive then script can run
        destination = initalPos;
        state = ScriptState.Cancel;
    }
}
