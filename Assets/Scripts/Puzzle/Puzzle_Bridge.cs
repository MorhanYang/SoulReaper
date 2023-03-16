using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Bridge : MonoBehaviour
{
    [SerializeField] Vector3 endPos;
    [SerializeField] GameObject Blocker;
    [SerializeField] Puzzle_Bridge secondBridge;
    enum ScriptState
    {
        Regular,
        Active,
        Cancel,
    }
    ScriptState state = ScriptState.Regular;

    Vector3 initalPos;
    Vector3 destination;
    public bool isWalkable = false;

    private void Start()
    {
        initalPos = transform.localPosition;
    }
    private void Update()
    {
        switch (state)
        {
            case ScriptState.Regular:
                break;

            case ScriptState.Active:
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, 0.8f * Time.deltaTime);
                if (transform.localPosition == destination)
                {
                    state = ScriptState.Regular;
                    isWalkable = true;// help other script to check if it is walkable

                    // open blocker
                    if (secondBridge == null){
                        OpenBlocker();
                    }
                    else if (secondBridge.isWalkable){
                        OpenBlocker();
                        secondBridge.OpenBlocker();
                    }

                }
                break;

            case ScriptState.Cancel:
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, 0.8f * Time.deltaTime);
                if (transform.localPosition == destination)
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
        destination = initalPos;
        state = ScriptState.Cancel;

        // Close Blocker
        CloseBlocker();
        if (secondBridge != null) secondBridge.CloseBlocker();

        isWalkable = false;// help other script to check if it is walkable
    }

    public void OpenBlocker(){
        Blocker.SetActive(false);
    }
    public void CloseBlocker(){
        Blocker.SetActive(true);
    }
}
