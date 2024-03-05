using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string scene;
    public int landID;
    public int hpCellNum;
    public float healthMax;
    public float characterHealth;
    public float[] position;
    

    public PlayerData(PlayerControl player) {

        PlayerHealth playerHealthBar = player.GetComponent<PlayerHealth>();
        TroopManager troopManager = player.GetComponent<TroopManager>();

        scene = player.sceneName;
        landID = player.landID;
        characterHealth = playerHealthBar.presentPlayerHp;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        //Troop data

    }
}
