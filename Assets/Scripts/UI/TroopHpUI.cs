using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TroopHpUI : MonoBehaviour
{

    public Image troopHpBar;
    [SerializeField] Image specialMinionHead;

    [SerializeField] Image selectedImage;
    [SerializeField] BranchTreeUI branchTree;
    [SerializeField] Image healthBar;
    [SerializeField] Image lockIcon;

    public List<MinionHpUI> minionHpList;
    public TroopNode.NodeType myNodeType;

    private void Start()
    {
        myNodeType = TroopNode.NodeType.ExtraHp;
    }

    //**************************************** Troop Fucntion **************************************************
    public void ChangeTroopSelectState(bool State)
    {
        selectedImage.gameObject.SetActive(State); 
    }

    public void SendSelectedMinionToBranchTree(MinionHpUI presentMinion) {

        // set select minion
        branchTree.SwitchSelectedMinionTo(presentMinion);
    }

    public void ChangeLockIconState(bool state)
    {
        lockIcon.gameObject.SetActive(state);
    }

    public void ChangeSpecialMinionHead( Minion.MinionStyle myStyle)
    {
        if (myStyle == Minion.MinionStyle.defualt)
        {
            specialMinionHead.enabled = false;
        }
        else
        {
            specialMinionHead.enabled = true;
            Sprite mysprite = MinionProfileManager.GetSpriteByMinionStyle(myStyle);
            specialMinionHead.sprite = mysprite;
        }

    }

    public void ChangeNodeType(TroopNode.NodeType type)
    {
        myNodeType = type;
        switch (type)
        {
            case TroopNode.NodeType.Troop:
                healthBar.color = Color.white;
                break;
            case TroopNode.NodeType.TroopWithSpecialMinion:
                healthBar.color = Color.white;
                break;
            case TroopNode.NodeType.ExtraHp:
                healthBar.color = new Color32(111,248,132,255);
                break;
            case TroopNode.NodeType.Empty:
                break;
            case TroopNode.NodeType.Locked:
                break;
            default:
                break;
        }
    }

    //**************************************** HP Fucntion **************************************************

    public void TroopHpBarDisplay( float presentHp, float MaxHp )
    {
        if (MaxHp != 0)
        {
            // from 0.08 - 0.92
            healthBar.fillAmount = (presentHp / MaxHp) * 0.84f + 0.08f;
        }
        else Debug.Log("Troop's MaxHp shouldn't be 0");
        
    }
}
