using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopNode
{
    public enum NodeType
    {
        Troop,
        ExtraHp,
        Empty,
    }

    public int troopId;
    public List<Minion> minionList;
    public float maxTroopHp;
    public float troopHp;
    public NodeType type;

    public TroopNode(int myId, TroopNode.NodeType myType, float troopMaxHp, List<Minion> myMinionList)
    {
        troopId = myId;
        type = myType;
        minionList = myMinionList;
        maxTroopHp = troopMaxHp;
        troopHp = maxTroopHp;
    }

    public void ChangeTroopHp( float myHp)
    {
        troopHp = myHp;
    }

    public void AddMinion(Minion myMinion)
    {
        if (type == NodeType.Troop)
        {
            minionList.Add(myMinion);
        }
        
    }

    public List<Minion> GetMinionList()
    {
        return minionList;
    }

    public void ChangeTroopNodeType( NodeType myType )
    {
        type = myType;
        if (type == NodeType.ExtraHp)
        {
            minionList.Clear();
        }
    }

    public void SetNodeHp( float hp , float maxHp )
    {
        troopHp = hp;
        maxTroopHp = maxHp;

        if (troopHp == 0){
            ChangeTroopNodeType(NodeType.Empty);
        }

        if (type == NodeType.Empty && hp > 0)
        {
            ChangeTroopNodeType(NodeType.ExtraHp);
        }
    }
}
