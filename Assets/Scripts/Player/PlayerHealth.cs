using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]BranchTreeUI branchTree;
    GameManager gameManager;
    TroopManager troopManager;

    // Hp management
    [SerializeField] float playerMaxHp = 20;
    float presentPlayerHp;
    float extraPlayerHp;
    List<TroopNode> ExtraHealthNodeList;
    List<TroopNode> troopDataList;
    public float ExtraHpNodeMaxHp = 50;

    // recover health
    Transform MarkedSubject;

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

        troopDataList = troopManager.TroopDataList;

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
            TakeDamage(10, this.transform, Vector3.one);
        }
    }

    //************************************************************** Initialize *****************************************************

    private void InitiallizeHp()
    {
        // *****************************Data setup
        // Player HP
        presentPlayerHp = playerMaxHp;

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
        MarkedSubject = target;
    }
    public void AbsorbOthers()
    {
        float recoverAmount;
        if (MarkedSubject != null && MarkedSubject.GetComponent<AbsorbableMark>() != null)
        {
            AbsorbableMark myTaget = MarkedSubject.GetComponent<AbsorbableMark>();
            // is enabled Mark?
            if (myTaget.isActiveAndEnabled)
            {
                // different types excute different function
                AbsorbableMark.AbsorbType myTargetType = myTaget.myAbsorbType;
                switch (myTargetType)
                {
                    case AbsorbableMark.AbsorbType.Normal:
                        myTaget.EatThis(true);
                        recoverAmount = myTaget.GetRecoverAmount();
                        RecoverHpInOrder(recoverAmount);
                        break;
                    case AbsorbableMark.AbsorbType.Enemy:
                        break;
                    case AbsorbableMark.AbsorbType.Minion:
                        myTaget.EatThis(true);
                        recoverAmount = myTaget.GetRecoverAmount();
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

    void RecoverHpInOrder( float recvoerAmount )
    {
        float recoverUnit = 10; // smallest health unity 
        int recoverTimes = (int)(recvoerAmount / recoverUnit);
        for (int i = 0; i < recoverTimes; i++)
        {
            // recover Player HP first
            if (presentPlayerHp < playerMaxHp)
            {
                presentPlayerHp += recoverUnit;
                if (presentPlayerHp > playerMaxHp) presentPlayerHp = playerMaxHp;
            }
            // recover troopNode HP
            else if (troopDataList.Count > 0)
            {
                for (int j = 0; j < troopDataList.Count; j++)
                {
                    if (troopDataList[j].troopHp < troopDataList[j].maxTroopHp)
                    {
                        // add hp
                        float hp = troopDataList[j].troopHp + recoverUnit;
                        if (hp > troopDataList[j].maxTroopHp) hp = troopDataList[j].maxTroopHp;

                        troopDataList[j].SetNodeHp(hp, troopDataList[j].maxTroopHp);
                        break;
                    } 
                }
            }
        }

        // Update UI
        branchTree.ChangePlayerHpDisplay(presentPlayerHp, playerMaxHp);
        branchTree.RefreshNodeUI(troopDataList);
    }

    void KillMinions(List<Minion> killList)
    {
        for (int i = 0; i < killList.Count; i++)
        {
            //killList[i].SetInactive(true);
        }
        // change cursor
        GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
    }
}
