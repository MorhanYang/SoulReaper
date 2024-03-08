using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Test : MonoBehaviour
{
    PlayerControl playerControl;
    PlayerDialogue playerDialogue;
    PlayerHealth playerhealth;
    TroopManager troopManager;

    bool isInitial = true;

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
        if (isInitial)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
                float rm = Random.Range(-1, 2);
                if (rm < 0){
                    playerDialogue.ShowPlayerCall("Right...Mouse....Click...", 1.5f);
                }
                else if(rm <1)
                {
                    playerDialogue.ShowPlayerCall("Can't move my Legs. Unless...",1.5f);
                }
                else
                {
                    playerDialogue.ShowPlayerCall("Rats...Delicious...Absorb...", 1.5f);
                }
            }
            if (playerhealth.presentPlayerHp >= playerhealth.playerMaxHp)
            {
                playerControl.moveSpeed = 100f;
                isInitial = false;
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
        playerControl.moveSpeed = 20f;
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
