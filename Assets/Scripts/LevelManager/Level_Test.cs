using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Test : MonoBehaviour
{
    PlayerControl playerControl;
    PlayerDialogue playerDialogue;
    PlayerHealth playerhealth;
    TroopManager troopManager;

    private void Start()
    {
        playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        playerDialogue = PlayerManager.instance.player.GetComponent<PlayerDialogue>();
        playerhealth = PlayerManager.instance.player.GetComponent<PlayerHealth>();
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();
        DisableAllAbility();
    }

    private void Update()
    {
        // hint player how to unlock the movement
        if (!playerControl.canMove)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
                float rm = Random.Range(-1, 1);
                if (rm < 0){
                    playerDialogue.ShowPlayerCall("Er...I need energy.");
                }
                else{
                    playerDialogue.ShowPlayerCall("I cant move my Leg, Use right click to grab life.");
                }
            }
            if (playerhealth.presentPlayerHp >= playerhealth.playerMaxHp)
            {
                playerControl.canMove = true;
            }
        }

        // unlock right hold action if player get a troopnode
        if (!playerControl.canRightSpecialAction && troopManager.troopDataList[0].type != TroopNode.NodeType.Locked)
        {
            playerControl.canRightSpecialAction = true;
        }
        // unlock Group Assign if player have many nodes
        if (!playerControl.canLeftSpecialAction && troopManager.troopDataList[1].type != TroopNode.NodeType.Locked)
        {
            playerControl.canLeftSpecialAction = true;
        }
    }

    void DisableAllAbility()
    {
        playerControl.canMove = false;
        playerControl.canMeleeAttack = false;
        playerControl.canRightSpecialAction = false;
        playerControl.canLeftSpecialAction = false;
    }
    void EnableAbility(int Id)
    {
        switch (Id)
        {
            case 0:
                playerControl.canMove = true;
                break;
            case 1:
                playerControl.canMeleeAttack = true;
                break;
            case 2:
                playerControl.canRightSpecialAction = true;
                break;
            case 3:
                playerControl.canLeftSpecialAction = true;
                break;
            default:
                break;
        }
    }

}
