using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]BranchTreeUI branchTree;
    GameManager gameManager;
    TroopManager troopManager;

    // Hp management
    public float playerMaxHp = 20;
    public float presentPlayerHp;
    List<TroopNode> ExtraHealthNodeList;
    List<TroopNode> troopDataList;
    public float ExtraHpNodeMaxHp = 50;

    // recover health
    [HideInInspector] public Transform MarkedEatSubject;

    // Damge
    Shaker shacker;//knock back
    // invincible
    float invincibleTimer = 0;
    bool isInvicible = false; // reset the collision state


    // Effect & sound
    float rebirthDelay = 0.6f;
    SoundManager mySoundManager;

    private void Start()
    {
        gameManager = GameManager.instance;
        troopManager = GetComponent<TroopManager>();
        shacker = GetComponent<Shaker>();
        mySoundManager = SoundManager.Instance;

        troopDataList = troopManager.troopDataList;

        InitiallizeHp();
    }

    private void Update()
    {
        //Timer to prevent players getting multiple Damage;
        if (isInvicible){
            if (invincibleTimer > 0){
                invincibleTimer -= Time.deltaTime;
            }
            else{
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
                isInvicible = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            TakeDamage(15, this.transform, Vector3.one);
        }
    }

    //************************************************************** Initialize *****************************************************

    private void InitiallizeHp()
    {
        // *****************************Data setup

        // Extra Health
        ExtraHealthNodeList = new List<TroopNode>();
        foreach (TroopNode item in troopDataList)
        {
            if (item.type == TroopNode.NodeType.ExtraHp){
                ExtraHealthNodeList.Add(item);

                item.SetNodeHp(ExtraHpNodeMaxHp, ExtraHpNodeMaxHp);
            }
        }

        //*****************************UI Setup
        branchTree.ChangePlayerHpDisplay(presentPlayerHp, playerMaxHp);
        branchTree.RefreshNodeUI( troopDataList );

    }

    private void UpdateExtraHpInfo()
    {
        ExtraHealthNodeList.Clear();

        foreach (TroopNode item in troopDataList)
        {
            if (item.type == TroopNode.NodeType.ExtraHp)
            {
                ExtraHealthNodeList.Add(item);
            }
        }
    }

    //

    //********************************************************** Damage Fucntion********************************************
    public void TakeDamage(float damage, Transform damageDealer, Vector3 attackPos)
    {
        if (invincibleTimer <= 0)
        {
            //Check extra health first
            UpdateExtraHpInfo();
            // have extra health
            if (ExtraHealthNodeList.Count > 0)
            {
                // find traget extra health node
                int targetId = ExtraHealthNodeList.Count - 1;
                TroopNode targetNode = ExtraHealthNodeList[targetId];
                float presentNodeHp = targetNode.troopHp - damage;
                if (presentNodeHp < 0) presentNodeHp = 0;

                //set node health
                targetNode.SetNodeHp(presentNodeHp, ExtraHpNodeMaxHp);

                //update UI display
                branchTree.RefreshNodeUI(troopDataList);
            }
            // dont have extra health deal player health
            else
            {
                presentPlayerHp -= damage;
                if (presentPlayerHp < 0) presentPlayerHp = 0;

                branchTree.ChangePlayerHpDisplay(presentPlayerHp, playerMaxHp);
            }

            // knock back
            if (damageDealer != null) shacker.AddImpact((transform.position - attackPos), damage, false);

            // become invincible
            Invincible(0.2f);

            // game over detect
            if (presentPlayerHp <=0)
            {
                Debug.Log("Game Over");
            }
        }
    }

    //****************************************************** Invincible ******************************************************
    public void Invincible(float duration)
    {
        // set invincible time;
        invincibleTimer = duration;
        isInvicible = true;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"));
    }

    //******************************************************* Recover health **************************************************
    public void MarkRegainTarget(Transform target)
    {
        MarkedEatSubject = target;
    }
    public void AbsorbOthers()
    {
        float recoverAmount;
        if (MarkedEatSubject != null && MarkedEatSubject.GetComponent<AbsorbableMark>() != null)
        {
            AbsorbableMark myTaget = MarkedEatSubject.GetComponent<AbsorbableMark>();
            // is enabled Mark?
            if (myTaget.isActiveAndEnabled)
            {
                // different types excute different function
                AbsorbableMark.AbsorbType myTargetType = myTaget.myAbsorbType;
                switch (myTargetType)
                {
                    case AbsorbableMark.AbsorbType.Normal:
                        recoverAmount = myTaget.EatThisToRecover(true); 
                        RecoverHpInOrder(recoverAmount);
                        break;
                    case AbsorbableMark.AbsorbType.Enemy:
                        break;
                    case AbsorbableMark.AbsorbType.Minion:
                        recoverAmount = myTaget.EatThisToRecover(true);
                        RecoverHpInOrder(recoverAmount);
                        break;
                    case AbsorbableMark.AbsorbType.Troop:
                        break;
                    default:
                        break;
                }
            } 
        }
    }

    /// <summary>
    /// recover health is consistent, but take damage is by unit;
    /// </summary>
    void RecoverHpInOrder( float recoverAmount )
    {
        if (recoverAmount > 0)
        {
            float neededHp;
            // 1. recover Player HP first
            if (presentPlayerHp < playerMaxHp)
            {
                neededHp = playerMaxHp - presentPlayerHp;
                // recoverAmount doesn't cover need
                if (neededHp >= recoverAmount)
                {
                    presentPlayerHp += recoverAmount;
                    RecoverHpInOrder(0);// refresh HP bar
                    return;
                }
                // more recoverAmount, reload this function again
                else if (neededHp < recoverAmount)
                {
                    float newRecoverAmt = recoverAmount - neededHp;
                    presentPlayerHp += neededHp;
                    RecoverHpInOrder(newRecoverAmt);
                    return;
                }
            }
            else if (troopDataList.Count > 0)
            {
                // 2. recover troopNode HP
                for (int j = 0; j < troopDataList.Count; j++)
                {
                    // need recover
                    if (troopDataList[j].troopHp < troopDataList[j].maxTroopHp)
                    {
                        neededHp = troopDataList[j].maxTroopHp - troopDataList[j].troopHp;
                        if (neededHp >= recoverAmount)
                        {
                            // add hp
                            float hp = troopDataList[j].troopHp + recoverAmount;
                            troopDataList[j].SetNodeHp(hp, troopDataList[j].maxTroopHp);
                            RecoverHpInOrder(0);// refresh HP bar
                            return;
                        }
                        else if (neededHp < recoverAmount)
                        {
                            float newRecoverAmt = recoverAmount - neededHp;
                            // add hp
                            float hp = troopDataList[j].troopHp + neededHp;
                            troopDataList[j].SetNodeHp(hp, troopDataList[j].maxTroopHp);
                            RecoverHpInOrder(newRecoverAmt);
                            return;
                        }
                    }
                }

                // 3. recover minion HP recoverAmount is still availible -> recover minion HP
                for (int i = 0; i < troopDataList.Count; i++)
                {
                    for (int j = 0; j < troopDataList[i].GetMinionList().Count; j++)
                    {
                        Minion targetMinion = troopDataList[i].GetMinionList()[j];
                        if (targetMinion.presentHp < targetMinion.MaxHp)
                        {
                            neededHp = targetMinion.MaxHp - targetMinion.presentHp;
                            // minion will revcover more health than player
                            float multiplier = targetMinion.MaxHp / troopManager.hpUnit;
                            // doesn't cover the hp shortage
                            if (recoverAmount * multiplier < neededHp)
                            {
                                targetMinion.SetHealthPercentage((recoverAmount * multiplier + targetMinion.presentHp) / targetMinion.MaxHp);
                                RecoverHpInOrder(0);
                                return;
                            }
                            // recoverAmount left
                            else if (recoverAmount * multiplier >= neededHp)
                            {
                                targetMinion.SetHealthPercentage(1);
                                float newRecoverAmt = recoverAmount - (neededHp / multiplier);
                                RecoverHpInOrder(newRecoverAmt);
                                return;
                            }
                        }
                    }
                }

            }
        }
        

        // Update UI
        branchTree.ChangePlayerHpDisplay(presentPlayerHp, playerMaxHp);
        branchTree.RefreshNodeUI(troopDataList);
    }
}
