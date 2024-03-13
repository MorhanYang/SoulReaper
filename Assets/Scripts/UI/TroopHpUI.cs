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
        switch (myNodeType)
        {
            case TroopNode.NodeType.Locked:
                this.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            case TroopNode.NodeType.Troop:
                // Shrink Troop Sprite
                this.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;

            case TroopNode.NodeType.TroopWithSpecialMinion:
                // Shrink Troop Sprite
                this.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            case TroopNode.NodeType.ExtraHp:
                // Recover Troop Sprite
                this.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            case TroopNode.NodeType.Empty:
                this.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            default:
                break;
        }
    }

    //**************************************** HP Fucntion **************************************************

    public void TroopHpBarDisplay( float presentHp, float MaxHp )
    {
        if (MaxHp != 0) healthBar.fillAmount = presentHp / MaxHp;
        else Debug.Log("Troop's MaxHp shouldn't be 0");
        
    }
}
