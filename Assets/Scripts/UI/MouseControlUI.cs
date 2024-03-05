using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseControlUI : MonoBehaviour
{
    PlayerControl playerControl;
    TroopManager troopManager;
    [HideInInspector] public Canvas canvas;
    [SerializeField] GameObject leftList;
    [SerializeField] GameObject RightList;
    GameObject tempIndicator;


    RectTransform myRectTransform;
    Vector2 pos; // screen pos

    // revive
    [SerializeField] GameObject RevieveRangIndicater;
    [SerializeField] LayerMask groundMask;

    // Eat to Recovery
    Minion presentMinion;
    List<Minion> presentMinionList;


    public enum Action
    {
        None,
        LeftClickNormal,
        LeftClickSpecial1,
        LeftClickSpecial2,
        RightClickNormal,
        RightClickSpecial1,
        RightClickSpecial2,
        RightClickSpecial3,
        RightClickSpecial4,
    }

    [HideInInspector] public Action myAction;


    private void OnEnable()
    {
        playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();
        myRectTransform = GetComponent<RectTransform>();

        myAction = Action.None;
        presentMinionList = null;
        presentMinion = null;
    }

    public void ShowControlPanel()
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
        ShowControlPanel();

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

            case 13: // eat a minion
                myAction = Action.RightClickSpecial3;
                // show single minion
                if (troopManager.GetPresentMinion() != null){
                    presentMinion = troopManager.GetPresentMinion();
                    presentMinion.ActivateEatMarker();
                }
                break;

            case 14: // eat all minion
                // show all minion
                myAction = Action.RightClickSpecial4;

                // two conditions select troop or select player
                // ****selecting player head 
                if (troopManager.GetPresentTroop() == null && troopManager.GetPresentMinion() == null)
                {
                    presentMinionList = new List<Minion>();
                    for (int i = 0; i < troopManager.troopDataList.Count; i++)
                    {
                        for (int j = 0; j < troopManager.troopDataList[i].GetMinionList().Count; j++)
                        {
                            Minion tempMinion = troopManager.troopDataList[i].GetMinionList()[j];
                            tempMinion.ActivateEatMarker();
                            presentMinionList.Add(tempMinion);
                        }
                    }
                }
                //**** normal selected troop
                else if (troopManager.GetPresentTroop().GetMinionList() != null)
                {
                    presentMinionList = troopManager.GetPresentTroop().GetMinionList();
                    for (int i = 0; i < presentMinionList.Count; i++)
                    {
                        presentMinionList[i].ActivateEatMarker();
                    }
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

        if (presentMinion != null)
        {
            presentMinion.DeactivateEatSeleted();
            presentMinion = null;
        }

        if (presentMinionList != null)
        {
            for (int i = 0; i < presentMinionList.Count; i++)
            {
                presentMinionList[i].DeactivateEatSeleted();
            }
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
