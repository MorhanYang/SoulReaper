using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public int hpCellNum;
    public float healthMax;
    public float health;
    public float[] position;
    public int normalMinionNum;
    public int specialMinionNum;

    public PlayerData(PlayerControl player) {

        PlayerHealthBar playerHealthBar = player.GetComponent<PlayerHealthBar>();

        level = player.levelNum;
        hpCellNum = playerHealthBar.cellNum;
        healthMax = hpCellNum * 20;// Set 20 as a unit in unity
        health = healthMax;


        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        // Troop data
        List<MinionTroop> troopList = playerHealthBar.GetActivedTroop();
        normalMinionNum = 0;
        specialMinionNum= 0;
        for (int i = 0; i < troopList.Count; i++)
        {
            // check special minion
            if (troopList[i].GetTroopEmptySpace() == 0 && troopList[i].GetMinionList().Count <= 1){
                specialMinionNum++;
            }
            // normal minions
            else{
                normalMinionNum += troopList[i].GetMinionList().Count;
            }
        }
    }
}