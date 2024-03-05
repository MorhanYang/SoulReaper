using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BranchTreeUI : MonoBehaviour
{
    private TroopManager troopManager;
    private List<TroopNode> troopDataList;

    [SerializeField] List<TroopHpUI> troopHpUIList;
    [SerializeField] PlayerHealthUI playerHealthUI;
    TroopHpUI SelectedTroopUI;
    MinionHpUI SelectedMinionUI;

    int[] tempMinionPos;
    public enum SelectType
    {
        SelectMinion,
        SelectTroop,
        SelectPlayer,
    }
    public SelectType mySelectType = SelectType.SelectPlayer;

    private void Start()
    {
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();
        troopDataList = troopManager.troopDataList;

        //setup inital selected Troop
        SwitchPlayerAsTroop();
    }

    //*********************************************************************** Select Troop ******************************************************
    void CleanInfoDependingOnType()
    {
        // clean previous info depending on type
        switch (mySelectType)
        {
            case SelectType.SelectMinion:
                // clean selected minion
                SelectedTroopUI.ChangeTroopSelectState(false);
                troopManager.SetPresentTroop(null);
                SelectedTroopUI = null;

                SelectedMinionUI.SelectThisMinion(false);
                troopManager.SetPresentMinion(null);
                SelectedMinionUI = null;
                break;
            case SelectType.SelectTroop:
                SelectedTroopUI.ChangeTroopSelectState(false);
                troopManager.SetPresentTroop(null);
                SelectedTroopUI = null;
                break;
            case SelectType.SelectPlayer:
                playerHealthUI.UnselectPlayerSlot();
                break;
            default:
                break;
        }
    }

    public void SwitchPresentTroopTo( TroopHpUI presentTroopUI )
    {
        // clean info first
        CleanInfoDependingOnType();

        // type is troop
        mySelectType = SelectType.SelectTroop;

        // Change UI
        SelectedTroopUI = presentTroopUI;
        SelectedTroopUI.ChangeTroopSelectState(true);

        // find the selectedtroop place and cast to troop data
        int troopId = troopHpUIList.IndexOf(SelectedTroopUI);
        if (troopId < troopDataList.Count){

            TroopNode selTroopInData = troopDataList[troopId];
            troopManager.SetPresentTroop(selTroopInData);
        }
        else { Debug.LogWarning("Can't find a TroopdataList ID same as UI you selected"); }
    }

    public void SwitchPlayerAsTroop()
    {
        // clean info first
        CleanInfoDependingOnType();

        // type is player
        mySelectType = SelectType.SelectPlayer;

        // Change UI
        playerHealthUI.SelectPlayerSlot();
    }

    public void SwitchSelectedMinionTo(MinionHpUI presentMinionUI)
    {
        // clean info first
        CleanInfoDependingOnType();

        // type is Minion
        mySelectType = SelectType.SelectMinion;

        // find selectedMinion place and cast to troop data
        int troopUiId = 0;
        for (int i = 0; i < troopHpUIList.Count; i++){
            if (troopHpUIList[i].minionHpList.Contains(presentMinionUI)){
                troopUiId = i;
                break;
            }  
        }
        int minionUiId = troopHpUIList[troopUiId].minionHpList.IndexOf(presentMinionUI);
        TroopNode selTroop = troopDataList[troopUiId];
        Minion selMinion = selTroop.GetMinionList()[minionUiId];

        // set Data
        SelectedMinionUI = presentMinionUI;
        SelectedTroopUI = troopHpUIList[troopUiId];
        troopManager.SetPresentTroop(selTroop);
        troopManager.SetPresentMinion(selMinion);

        // Change UI
        SelectedTroopUI.ChangeTroopSelectState(true);
        SelectedMinionUI.SelectThisMinion(true);
    }

    public SelectType GetSelectType()
    {
        return mySelectType;
    }

    // ******************************************************************** Fresh UI ************************************************
    public void RefreshNodeUI( List<TroopNode> DataList )
    {
        // reset troop & Minion 
        for (int i = 0; i < troopHpUIList.Count; i++)
        {
            TroopHpUI troopUI = troopHpUIList[i];

            troopUI.ChangeNodeType(DataList[i].type);
            // Check Node Type
            switch (troopHpUIList[i].myNodeType)
            {
                case TroopNode.NodeType.Locked:
                    // change node HpBar display
                    troopUI.TroopHpBarDisplay(0, DataList[i].maxTroopHp);
                    troopUI.ChangeLockIconState(true);
                    // hide minion
                    for (int j = 0; j < troopUI.minionHpList.Count; j++)
                    {
                        troopUI.minionHpList[j].ShowMinionUIBg(false);
                        troopUI.minionHpList[j].SwitchThisMinionSlot(false);
                    }
                    break;

                case TroopNode.NodeType.Troop:
                    // change node HpBar display
                    troopUI.TroopHpBarDisplay(troopDataList[i].troopHp, troopDataList[i].maxTroopHp);
                    troopUI.ChangeLockIconState(false);

                    // show minion
                    for (int j = 0; j < troopHpUIList[i].minionHpList.Count; j++)
                    {
                        MinionHpUI targetMinionUI = troopUI.minionHpList[j];
                        targetMinionUI.ShowMinionUIBg(true);
                        if (j < DataList[i].minionList.Count)
                        {
                            targetMinionUI.SwitchThisMinionSlot(true);
                            targetMinionUI.FreshMinionHpBar(DataList[i].GetMinionList()[j].GetHealthPercentage());

                        }
                        else
                        {
                            targetMinionUI.SwitchThisMinionSlot(false);
                        }
                    }
                    break;
                
                case TroopNode.NodeType.ExtraHp:
                    // change node HpBar display
                    troopUI.TroopHpBarDisplay(DataList[i].troopHp, DataList[i].maxTroopHp);
                    troopUI.ChangeLockIconState(false);

                    // hide minion
                    for (int j = 0; j < troopUI.minionHpList.Count; j++)
                    {
                        troopUI.minionHpList[j].ShowMinionUIBg(false);
                        troopUI.minionHpList[j].SwitchThisMinionSlot( false );
                    }
                    break;

                case TroopNode.NodeType.Empty:
                    troopUI.TroopHpBarDisplay(0, DataList[i].maxTroopHp);// prevent node health bar from lefting health.
                    troopUI.ChangeLockIconState(false);

                    // hide minion
                    for (int j = 0; j < troopUI.minionHpList.Count; j++)
                    {
                        troopUI.minionHpList[j].ShowMinionUIBg(false);
                        troopUI.minionHpList[j].SwitchThisMinionSlot(false);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void RefreshOneMinion(int[] minonPos)
    {
        Debug.Log(minonPos);
        int x = minonPos[0];
        int y = minonPos[1];
        // get target UI
        TroopHpUI targetTroopUI = troopHpUIList[x];
        MinionHpUI targetMinionUI = targetTroopUI.minionHpList[y];

        // get target Data
        TroopNode targetTroop = troopDataList[x];
        Minion targetMinion = targetTroop.minionList[y];
        float percentage = targetMinion.GetHealthPercentage();

        targetMinionUI.FreshMinionHpBar(percentage);
    }

    //********************************************************************** HP **********************************************************************
    public void ChangePlayerHpDisplay( float presentHp, float maxHp )
    {
        if (maxHp != 0) playerHealthUI.FreshHpInfo(presentHp, maxHp);
        else Debug.Log("Player's MaxHp shouldn't be 0");
    }

    //********************************************************************* Hovering select **********************************************************

    public void SelectAllMember( TroopHpUI troopUI )
    {
        // find troop and minions
        int troopId = troopHpUIList.IndexOf(troopUI);
        if (troopId != -1) // fond this troop
        {
            // availible ID
            if (troopId < troopDataList.Count)
            {
                for (int i = 0; i < troopDataList[troopId].GetMinionList().Count; i++)
                {
                    troopDataList[troopId].GetMinionList()[i].ActivateEatMarker();
                }
            }
        }
        

    }
    public void UnselectAllMember(TroopHpUI troopUI)
    {
        // find troop and minions
        int troopId = troopHpUIList.IndexOf(troopUI);
        if (troopId != -1)
        {
            // availible ID
            if (troopId < troopDataList.Count)
            {
                for (int i = 0; i < troopDataList[troopId].GetMinionList().Count; i++)
                {
                    troopDataList[troopId].GetMinionList()[i].DeactivateEatSeleted();
                }
            }
        }
        
    }

    public void SelectOneMember( MinionHpUI minionUI )
    {
        // find minion
        int troopId = -1;
        int minionId = -1;
        for (int i = 0; i < troopHpUIList.Count; i++)
        {
            if (troopHpUIList[i].minionHpList.Contains(minionUI))
            {
                troopId = i;
                minionId = troopHpUIList[i].minionHpList.IndexOf(minionUI);
                break;
            }  
        }
        tempMinionPos = new int[]{troopId, minionId};

        //activate if available 
        if (troopId < troopDataList.Count && minionId < troopDataList[troopId].GetMinionList().Count)
        {
            troopDataList[troopId].GetMinionList()[minionId].ActivateEatMarker();
        }
        
    }

    public void UnselectOneMember( MinionHpUI minionUI )
    {
        // check if TempMinionPos is the right minionUI`
        int x = tempMinionPos[0];
        int y = tempMinionPos[1];
        if (x != -1 && y != -1) // prevent it from {-1,-1}
        {
            if (troopHpUIList[x].minionHpList[y] == minionUI)
            {
                // available?
                if (x < troopDataList.Count && y < troopDataList[x].GetMinionList().Count)
                {
                    troopDataList[x].GetMinionList()[y].DeactivateEatSeleted();
                }
            }
            // not the right temp
            else
            {
                Debug.LogWarning("wrong tempMinionPos");
            }
        }
        
    }

}
