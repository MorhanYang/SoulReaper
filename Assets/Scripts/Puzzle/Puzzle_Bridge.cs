using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Bridge : MonoBehaviour
{
    [SerializeField] Vector3 endPos;
    [SerializeField] GameObject Blocker;
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
        initalPos = transform.position;
    }
    private void Update()
    {
        switch (state)
        {
            case ScriptState.Regular:
                break;

            case ScriptState.Active:
                transform.position = Vector3.MoveTowards(transform.position, destination, 0.8f * Time.deltaTime);
                if (transform.position == destination)
                {
                    Blocker.SetActive(false);
                    state = ScriptState.Regular;
                }
                break;

            case ScriptState.Cancel:
                transform.position = Vector3.MoveTowards(transform.position, destination, 0.8f * Time.deltaTime);
                if (transform.position == destination)
                {
                    state = ScriptState.Regular;
                }
                break;
        }

    }

    public void ActiveScript()
    {
        destination = endPos;
        state = ScriptState.Active;
    }

    public void CancelScript()
    {
        gameObject.SetActive(true);// setActive then script can run
        Blocker.SetActive(true);
        destination = initalPos;
        state = ScriptState.Cancel;
    }
}
