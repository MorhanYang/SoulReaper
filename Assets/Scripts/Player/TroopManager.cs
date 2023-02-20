using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopManager : MonoBehaviour
{

    public MinionTroop[] troopList;



    public MinionTroop GetTroopItem( int troopID )
    {
        return troopList[troopID];
    }
}
