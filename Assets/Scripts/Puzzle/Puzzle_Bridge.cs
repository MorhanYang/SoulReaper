using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Bridge : MonoBehaviour
{
    [SerializeField] Vector3 endPos;
    [SerializeField] GameObject Blocker;
    [SerializeField] Puzzle_Bridge secondBridge;

    [SerializeField] int objectsNeeded = 1;
    [SerializeField] bool canTransparent = false;
    int objectCounter;
    Color myColor;
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
        if (canTransparent) myColor = GetComponent<MeshRenderer>().material.color;
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
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, 4f * Time.deltaTime);
                if (transform.localPosition == destination)
                {
                    state = ScriptState.Regular;
                }
                break;
        }

    }
    //*********************************************Method****************************************************
    public int GetNeedObjectNumber()
    {
        return objectsNeeded;
    }


    public void AddObject(int size)
    {
        objectCounter+= size;
        if (objectCounter >= objectsNeeded){
            destination = endPos;
            state = ScriptState.Active;
        }
    }

    public void DetractObject(int size)
    {
        objectCounter-= size;// solve multiple trigger problem
        if (objectCounter < 0) objectCounter = 0;// just in case

        if (objectCounter < objectsNeeded) {
            gameObject.SetActive(true);// setActive then script can run
            destination = initalPos;
            state = ScriptState.Cancel;

            // Close Blocker
            CloseBlocker();
            if (secondBridge != null) secondBridge.CloseBlocker();



            isWalkable = false;// help other script to check if it is walkable
        } 
    }

    public void OpenBlocker(){
        if(Blocker !=null) Blocker.SetActive(false);
        Debug.Log("open Blocker " + gameObject.name);

        // transparent control
        if (canTransparent){
            Color newColor = new Color(myColor.r, myColor.g, myColor.b, 0.6f);
            GetComponent<MeshRenderer>().material.color = newColor;
        }
    }
    public void CloseBlocker(){
        Blocker.SetActive(true);

        // transparent control
        if (canTransparent){
            Color newColor = new Color(myColor.r, myColor.g, myColor.b, 1f);
            GetComponent<MeshRenderer>().material.color = newColor;
        }
    }

    public bool GetBridgeState()
    {
        return Blocker.activeSelf;
    }
}
