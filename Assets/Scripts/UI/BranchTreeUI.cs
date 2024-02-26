using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BranchTreeUI : MonoBehaviour
{
    private TroopManager troopManager;
    private List<TroopNode> troopDataList;

    [SerializeField] List<TroopHpUI> troopHpList;
    [SerializeField] PlayerHealthUI playerHealthUI;
    TroopHpUI SelectedTroop;
    MinionHpUI SelectedMinion;

    private void Start()
    {
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();
        troopDataList = troopManager.TroopDataList;

        //setup inital selected Troop
        SwitchPlayerAsTroop();
    }
    
    //******************************** Switch Troop Select******************************************************
    public void SwitchPresentTroopTo( TroopHpUI presentTroopUI )
    {
        if (SelectedMinion != null){
            // clean selected minion if player clicks troop icon
            SelectedMinion.SelectThisMinion( false );
            troopManager.SetPresentMinion(null);
        }
        if (SelectedTroop != null){
            SelectedTroop.ChangeTroopSelectState(false);
        }
        else{
            playerHealthUI.UnselectPlayerSlot();
        }
        SelectedTroop = presentTroopUI;
        SelectedTroop.ChangeTroopSelectState(true);

        // find selectedtroop place in troop data
        int troopId = troopHpList.IndexOf(SelectedTroop);
        TroopNode selTroopInData = troopDataList[troopId];
        troopManager.SetPresentTroop(selTroopInData);
        
    }

    public void SwitchPlayerAsTroop()
    {
        if (SelectedTroop != null){
            SelectedTroop.ChangeTroopSelectState(false);
        }
        SelectedTroop = null;
        playerHealthUI.SelectPlayerSlot();


        troopManager.SetPresentTroop(null);
        troopManager.SetPresentMinion(null);
    }

    public void SwitchSelectedMinionTo(MinionHpUI presentMinion)
    {
        if(SelectedMinion != null){
            SelectedMinion.SelectThisMinion(false);
        }
        SelectedMinion = presentMinion;
        SelectedMinion.SelectThisMinion( true );

        // find selectedMinion place in troop data
        int troopId = troopHpList.IndexOf(SelectedTroop);
        int minionId = SelectedTroop.minionHpList.IndexOf(presentMinion);
        TroopNode selTroop = troopDataList[troopId];
        Minion selMinion = selTroop.GetMinionList()[minionId];

        troopManager.SetPresentMinion(selMinion);
    }

    // ******************************************************************** Fresh UI ************************************************
    public void RefreshNodeUI( List<TroopNode> TroopDataList )
    {
        // reset troop & Minion 
        for (int i = 0; i < troopHpList.Count; i++)
        {
            TroopHpUI troopUI = troopHpList[i];

            troopUI.ChangeNodeType(TroopDataList[i].type);
            // Check Node Type
            switch (troopHpList[i].myNodeType)
            {
                case TroopNode.NodeType.Troop:
                    for (int j = 0; j < troopHpList[i].minionHpList.Count; j++)
                    {
                        // change node HpBar display
                        troopUI.TroopHpBarDisplay(troopDataList[i].troopHp, troopDataList[i].maxTroopHp);

                        MinionHpUI targetMinionUI = troopUI.minionHpList[j];
                        // show minion
                        targetMinionUI.ShowMinionUIBg(true);
                        if (j < TroopDataList[i].minionList.Count)
                        {
                            targetMinionUI.SwitchThisMinionSlot(true);
                            targetMinionUI.FreshMinionHpBar(TroopDataList[i].GetMinionList()[j].GetHealthPercentage());

                        }
                        else
                        {
                            targetMinionUI.SwitchThisMinionSlot(false);
                        }
                    }
                    break;
                
                case TroopNode.NodeType.ExtraHp:
                    // change node HpBar display
                    troopUI.TroopHpBarDisplay(TroopDataList[i].troopHp, TroopDataList[i].maxTroopHp);

                    // hide minion
                    
                    for (int j = 0; j < troopUI.minionHpList.Count; j++)
                    {
                        troopUI.minionHpList[j].ShowMinionUIBg(false);
                        troopUI.minionHpList[j].SwitchThisMinionSlot( false );
                    }
                    break;

                case TroopNode.NodeType.Empty:
                    troopUI.TroopHpBarDisplay(0, TroopDataList[i].maxTroopHp);// prevent node health bar from lefting health.
                    
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

    public void FreshOneMinion(int[] minonPos)
    {
        Debug.Log(minonPos);
        int x = minonPos[0];
        int y = minonPos[1];
        // get target UI
        TroopHpUI targetTroopUI = troopHpList[x];
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

}
