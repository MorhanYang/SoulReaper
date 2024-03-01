using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseControlUI : MonoBehaviour
{
    public PlayerControl playerControl;
    [HideInInspector] public Canvas canvas;
    [SerializeField] GameObject leftList;
    [SerializeField] GameObject RightList;
    GameObject tempIndicator;

    // revive
    [SerializeField] GameObject RevieveRangIndicater;
    [SerializeField] LayerMask groundMask;

    RectTransform myRectTransform;
    Vector2 pos; // screen pos

    public enum Action
    {
        None,
        LeftClickNormal,
        LeftClickSpecial1,
        LeftClickSpecial2,
        RightClickNormal,
        RightClickSpecial1,
        RightClickSpecial2,
    }

    [HideInInspector] public Action myAction;


    private void OnEnable()
    {
        playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        myRectTransform = GetComponent<RectTransform>();

        myAction = Action.None;
    }

    public void ShowIcon()
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1)) // Left Click
        {
            leftList.SetActive(true);
            RightList.SetActive(false);
        }

        if (Input.GetMouseButton(1) && !Input.GetMouseButton(0)) // Right Click
        {
            leftList.SetActive(false);
            RightList.SetActive(true);
        }
    }

    public void InitializeMouseUI( Canvas myCanvas )
    {
        ShowIcon();

        canvas = myCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
        myRectTransform.localPosition = pos;
    }

    public void SwitchActionPreview( int ActionId ) // -1 - None, 0 LeftNormal, 10 RightNormal
    {
        CleanIndicator();

        switch (ActionId)
        {
            case -1:
                myAction = Action.None;
                break;

            case 0:
                myAction = Action.LeftClickNormal;
                break;

            case 1:
                myAction = Action.LeftClickSpecial1;
                break;

            case 2:
                myAction = Action.LeftClickSpecial2;
                break;

            case 10:
                myAction = Action.RightClickNormal;
                break;

            case 11:
                myAction = Action.RightClickSpecial1;
                break;

            case 12: // reall all action
                myAction = Action.RightClickSpecial2;
               
                // pinpoint a world location and generate a range marker
                Vector3 aimPos;
                var ray = Camera.main.ScreenPointToRay(transform.position);

                if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
                {
                    aimPos = hitInfo.point;
                    // generate a circle on this location
                    tempIndicator = Instantiate(RevieveRangIndicater, aimPos, transform.rotation);
                }
                break;

            default:
                break;
        }
    }
    public void CleanIndicator()
    {
        if (tempIndicator != null)
        {
            Destroy(tempIndicator);
        }
    }

    public void ScaleUpIcon( Transform icon )
    {
        icon.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    public void ScaleDownIcon(Transform icon)
    {
        icon.localScale = new Vector3(1f, 1f, 1f);
    }

    public Action GetControlUIAction()
    {
        return myAction;
    }
}
