using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TroopManager : MonoBehaviour
{

    PlayerHealth playerHealth;

    public int maxTroopCapacity = 5; // it equal to the number of Minion's bar UI in Branch tree
    private int troopSlotNum = 5; // it equal to the number of Troop UI in Branch tree
    public int ActiveSlotNum = 0;
    public int hpUnit = 10;


    [SerializeField] BranchTreeUI branchTreeUI;
    TroopNode SelectedTroop;
    public TroopNode tempAbsorbedTroop;
    Minion SelectedMinion;

    public List<TroopNode> troopDataList;

    // revieve
    Transform MarkedRevievSubject;
    float rebirthDelay = 0.6f;

    float assignMinionTimer;
    [SerializeField] MinionMoverMarker assignMarker;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        SetUpTroopNodeList();
    }

    private void Start()
    {
        branchTreeUI.RefreshNodeUI(troopDataList);
    }

    //**************************************************************** SetUp *************************************************************

    public void SetPresentTroop( TroopNode troopNode ){

        SelectedTroop = troopNode; 
    }

    public void setTempAbsorbedTroop( TroopNode troopNode)
    {
        tempAbsorbedTroop = troopNode;
    }

    public TroopNode GetPresentTroop()
    {
        return SelectedTroop;
    }

    public void SetPresentMinion( Minion minion){

        SelectedMinion = minion; 
    }
    public Minion GetPresentMinion()
    {
        return SelectedMinion;
    }

    private void SetUpTroopNodeList()
    {
        troopDataList = new List<TroopNode>();
        for (int i = 0; i < troopSlotNum; i++)
        {
            if (i < ActiveSlotNum){
                troopDataList.Add(new TroopNode(i, TroopNode.NodeType.ExtraHp, playerHealth.ExtraHpNodeMaxHp, new List<Minion>()));
            }
            else
            {
                troopDataList.Add(new TroopNode(i, TroopNode.NodeType.Locked, playerHealth.ExtraHpNodeMaxHp, new List<Minion>()));
            }

        }
    }

    //*************************************************************** Lock & Unlcok a node **************************************************
    public void UnlockTroopNode( int number )
    {
        ActiveSlotNum += number;
        int count = number;
        // find first locked node
        for (int i = 0; i < troopDataList.Count; i++)
        {
            if (troopDataList[i].type == TroopNode.NodeType.Locked)
            {
                troopDataList[i].ChangeTroopNodeType(TroopNode.NodeType.ExtraHp);
                troopDataList[i].ChangeTroopHp(troopDataList[i].maxTroopHp);
                count--;
                if (count<=0){
                    break;
                }
            }
        }
        // update UI
        branchTreeUI.RefreshNodeUI(troopDataList);
    }
    public void lockTroopNode(int number)
    {
        ActiveSlotNum -= number;
        int count = number;
        // find last unlocked node
        for (int i = troopDataList.Count - 1; i >= 0; i--)
        {
            if (troopDataList[i].type != TroopNode.NodeType.Locked)
            {
                Debug.Log("LockOne");
                troopDataList[i].ChangeTroopNodeType(TroopNode.NodeType.Locked);
                count--;
                if (count <= 0){
                    break;
                }
            }
        }
        // update UI
        branchTreeUI.RefreshNodeUI(troopDataList);
    }


    //*************************************************************** Revieve Troop ***************************************************
    public void MarkedReviveMinion( Transform target )
    {
        MarkedRevievSubject = target;
    }
    public Transform GetMarkedReviveMinion()
    {
        return MarkedRevievSubject;
    }
    
    public void ReviveSingleMinion()
    {
        if (MarkedRevievSubject != null)
        {
            Debug.Log("revive target");
            List<Minion> myMinionList = new List<Minion>();
            myMinionList.Add(MarkedRevievSubject.GetComponent<Minion>());

            // revive
            ReviveMinionsFunction(myMinionList);
        }

        MarkedReviveMinion(null);
    }

    public void ReviveTroopNormal(Vector3 pointedPos, float radius)
    {
        Collider[] MinionInCircle = Physics.OverlapSphere(pointedPos, radius, LayerMask.GetMask("Minion"));// when rebirth minion, the layer will change

        List<Minion> minionSet = new List<Minion>();
        for (int i = 0; i < MinionInCircle.Length; i++)
        {
            minionSet.Add(MinionInCircle[i].GetComponent<Minion>());
        }
        
        // revive minion
        ReviveMinionsFunction(minionSet);

        MarkedReviveMinion(null); // player may use mous menu to revive so reset this would be better
    }

    void ReviveMinionsFunction(List<Minion> minionSet)
    {
        // reorder minions from highest to lowest
        //minionSet.Sort(SortByMinionSize);
        //minionSet.Reverse();

        if (minionSet.Count > 0)
        {
            // find a space for new minions
            foreach (Minion item in minionSet)
            {
                Minion thisMinion = item;
                for (int j = 0; j < troopDataList.Count; j++)
                {
                    if (troopDataList[j].type != TroopNode.NodeType.Locked)
                    {
                        TroopNode TargetTroop = troopDataList[j];
                        // change it to Troop
                        if (TargetTroop.type == TroopNode.NodeType.ExtraHp)
                        {

                            TargetTroop.ChangeTroopNodeType(TroopNode.NodeType.Troop);
                        }

                        //Check if troop have a space
                        float space = TargetTroop.troopHp - (TargetTroop.minionList.Count * hpUnit);
                        // enough space -> Add a health Minion 
                        if (space >= hpUnit)
                        {
                            // get data position in datalist
                            int[] dataPos = { j, TargetTroop.minionList.Count };

                            item.SetActiveDelay(rebirthDelay, dataPos);
                            item.SetHealthPercentage(1);// set minion a full hp
                            TargetTroop.AddMinion(item);
                            thisMinion = null; // success Add minion
                            break;
                        }
                        // not enough space -> Add a injured Minion
                        else if (space < hpUnit && space > 0)
                        {
                            // get data position in datalist
                            int[] dataPos = { j, TargetTroop.minionList.Count };

                            item.SetActiveDelay(rebirthDelay, dataPos);
                            item.SetHealthPercentage(space / hpUnit);// set minion injured hp
                            TargetTroop.AddMinion(item);
                            thisMinion = null; // success Add minion
                            break;
                        }
                    }
                }
                if (thisMinion != null) // didn't success add minion
                {
                    GetComponent<PlayerDialogue>().ShowPlayerCall("I need more health to revive", 2f);
                }
            }

            branchTreeUI.RefreshNodeUI(troopDataList);
        }
    }

    public void ReviveTroopLoading(List<Minion> reviveMinions)
    {
        ReviveMinionsFunction(reviveMinions);
    }
    // *********************************************************** Update Minion info ***************************************************
    public void RefreshOneMinionInfo( Minion myMinion )
    {
        int[] minionPos = myMinion.GetMinionDataPos();
        // update Data?
            // no need because data is lock to Minion

        // update UI
        branchTreeUI.RefreshOneMinion(minionPos);
    }

    // **************************************************************** kill Minions *******************************************************
    public void AbsorbTroopToRecover()
    {
        // ****** hovering branch tree to absorb
        if (tempAbsorbedTroop != null) 
        {
            if (tempAbsorbedTroop.type == TroopNode.NodeType.Troop)
            {
                KillTroopOfMinions(tempAbsorbedTroop);
                tempAbsorbedTroop = null;
            }
        }
        // ****** in world right click
        else
        {
            TroopNode targetTroop = SelectedTroop;
            // Player select a troop
            if (targetTroop != null && targetTroop.type == TroopNode.NodeType.Troop)
            {
                KillTroopOfMinions(targetTroop);
            }
            // player select player icon or unuseful troop
            else
            {
                for (int i = 0; i < troopDataList.Count; i++)
                {
                    KillTroopOfMinions(troopDataList[i]);
                }
            }
        }
        
        // reset all markedAbsorbTarget
        playerHealth.MarkRegainTarget(null);

        // refresh 
        branchTreeUI.RefreshNodeUI(troopDataList);
    }
    
    public void KillTroopOfMinions( TroopNode troop )
    {
        if (troop.type != TroopNode.NodeType.Locked)
        {
            // get hp left in the troop node
            float presentTroopHp;
            if (troop.troopHp > troop.minionList.Count * hpUnit) // Left hp in troop
            {
                presentTroopHp = troop.troopHp - troop.minionList.Count * hpUnit;
            }
            else // no hp left (troop.troopHp <= troop.minionList.Count * hpUnit) 
            {
                presentTroopHp = 0;
            }

            // remove minion
            for (int i = 0; i < troop.minionList.Count; i++)
            {
                troop.minionList[i].SetInactive();
                // clean minion dataPos info
                troop.minionList[i].SetMinionDataPos(-1, -1);
                // count hp
                float hpFromMinion = hpUnit * troop.minionList[i].GetHealthPercentage();
                presentTroopHp += hpFromMinion;
            }
            troop.minionList.Clear();

            // change node type
            troop.ChangeTroopNodeType(TroopNode.NodeType.ExtraHp);
            troop.ChangeTroopHp(presentTroopHp);
        }
    }
    
    
    public void EnemyKillOneMinion( Minion wantToKill )
    {
        wantToKill.SetInactive();
        // find where the minion is in dataList;
        int[] minionPos = wantToKill.GetMinionDataPos();
        TroopNode targetTroop = troopDataList[minionPos[0]];
        // remove minion from troop
        targetTroop.minionList.Remove(wantToKill);
        // tell minion the new position
        for (int i = 0; i < targetTroop.minionList.Count; i++){
            int[] myPos = targetTroop.minionList[i].GetMinionDataPos();
            targetTroop.minionList[i].SetMinionDataPos( myPos[0], i );
        }

        if (targetTroop.minionList.Count <= 0){
            //targetTroop.minionList.Clear();
            targetTroop.ChangeTroopNodeType(TroopNode.NodeType.ExtraHp);
        }

        // destruct Troop Hp
        float presentTroopHp = targetTroop.troopHp - hpUnit;
        // dont have hp left
        if (presentTroopHp <= 0) presentTroopHp = 0; // it will automatically change it to type.empty 

        targetTroop.SetNodeHp(presentTroopHp, targetTroop.maxTroopHp);

        // update Ui
        branchTreeUI.RefreshNodeUI(troopDataList);
    }
    //**************************************************************** Assign Troop *****************************************************
    public void AssignOneMinion(Vector3 aimPos)
    {
        // use collider to detect enemies
        Transform target = null;
        Collider[] hitedEnemy = Physics.OverlapSphere(aimPos, 0.3f, LayerMask.GetMask("Enemy", "PuzzleTrigger"));

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, aimPos);
        // if didn't hit a enemy
        else target = null;

        // show destination marker
        assignMarker.relocateMarker(aimPos, target);


        // ********************** excecute assignment
        // find pos at the navmesh
        NavMeshHit hit;
        Vector3 sprintPos = Vector3.zero;
        if (NavMesh.SamplePosition(aimPos, out hit, 2.5f, NavMesh.AllAreas))
        {
            sprintPos = hit.position;
        }

        // excute different action depending on type
        BranchTreeUI.SelectType myType = branchTreeUI.GetSelectType();
        Minion myMinion = GetClosedMinion(aimPos);

        switch (myType)
        {
            case BranchTreeUI.SelectType.SelectMinion:
                // assign single minion
                myMinion = SelectedMinion;
                if (target == null)// didn't hit anything
                {
                    myMinion.SprintToPos(sprintPos);
                }
                else // Hit something
                {
                    myMinion.SprintToEnemy(target);
                }

                break;
            case BranchTreeUI.SelectType.SelectTroop:
                //assign whole troop
                TroopNode mytroop = SelectedTroop;
                for (int i = 0; i < mytroop.GetMinionList().Count; i++)
                {
                    if (target == null) // don't have enemy target
                    {
                        mytroop.GetMinionList()[i].SprintToPos(sprintPos);
                    }
                    else // have enemy target
                    {
                        mytroop.GetMinionList()[i].SprintToEnemy(target);
                    }
                }
                break;
            case BranchTreeUI.SelectType.SelectPlayer:
                // assign random minion
                if (myMinion != null)
                {
                    if (target == null)// didn't hit anything
                    {
                        myMinion.SprintToPos(sprintPos);
                    }
                    else // Hit something
                    {
                        myMinion.SprintToEnemy(target);
                    }
                }
                break;
                

            default:
                break;
        }

        // play sound
        // mySoundManagers.PlaySoundAt(transform.position, "AssignMinion", false, false, 1.5f, 1f, 100, 100);
    }

    public void AssignAllMinions(Vector3 aimPos)
    {
        // use collider to detect enemies
        Transform target = null;
        Collider[] hitedEnemy = Physics.OverlapSphere(aimPos, 0.3f, LayerMask.GetMask("Enemy", "PuzzleTrigger"));

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, aimPos);
        // if didn't hit a enemy
        else target = null;

        // show destination marker
        assignMarker.relocateMarker(aimPos, target);

        // ************************* Execute sprint action
        // find pos at the navmesh
        NavMeshHit hit;
        Vector3 sprintPos = Vector3.zero;
        if (NavMesh.SamplePosition(aimPos, out hit, 2.5f, NavMesh.AllAreas))
        {
            sprintPos = hit.position;
        }

        // assign minions there are 3 conditions: 
        BranchTreeUI.SelectType myType = branchTreeUI.GetSelectType();
        switch (myType)
        {
            case BranchTreeUI.SelectType.SelectMinion:
                //assign whole troop
                TroopNode mytroop = SelectedTroop;
                for (int i = 0; i < mytroop.GetMinionList().Count; i++)
                {
                    if (target == null) // don't have enemy target
                    {
                        mytroop.GetMinionList()[i].SprintToPos(sprintPos);
                    }
                    else // have enemy target
                    {
                        mytroop.GetMinionList()[i].SprintToEnemy(target);
                    }    
                }
                break;


            case BranchTreeUI.SelectType.SelectTroop:
                // Assign All troop
                for (int i = 0; i < troopDataList.Count; i++)
                {
                    for (int j = 0; j < troopDataList[i].GetMinionList().Count; j++)
                    {
                        if (target == null)// don't have enemy target
                        {
                            troopDataList[i].GetMinionList()[j].SprintToPos(sprintPos);
                        }
                        else // have enemy target
                        {
                            troopDataList[i].GetMinionList()[j].SprintToEnemy(target);
                        }
                    }
                }
                break;


            case BranchTreeUI.SelectType.SelectPlayer:
                // Assign All troop
                for (int i = 0; i < troopDataList.Count; i++)
                {
                    for (int j = 0; j < troopDataList[i].GetMinionList().Count; j++)
                    {
                        if (target == null) // don't have enemy target
                        {
                            troopDataList[i].GetMinionList()[j].SprintToPos(sprintPos);
                        }
                        else // have enemy target
                        {
                            troopDataList[i].GetMinionList()[j].SprintToEnemy(target);
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    private Transform GetClosestEnemyTransform(Collider[] enemyList, Vector3 referencePoint)
    {
        Transform closedEnemy = null;

        for (int i = 0; i < enemyList.Length; i++)
        {
            // get trigger
            if (enemyList[i].GetComponent<PuzzleTrigger>() != null)
            {
                closedEnemy = enemyList[i].transform;
                return closedEnemy;
            }
            // get enemy
            else if (enemyList[i].GetComponent<EnemyScript>())
            {
                Collider testEnemy = enemyList[i];
                if (closedEnemy == null)
                {
                    closedEnemy = testEnemy.transform;
                }
                // test which is closer
                else if (Vector3.Distance(testEnemy.transform.position, referencePoint) > Vector3.Distance(closedEnemy.transform.position, referencePoint))
                {
                    closedEnemy = testEnemy.transform;
                }
            }
        }
        return closedEnemy;
    }

    private Minion GetClosedMinion(Vector3 pos)
    {
        // Find closest Minin to the Target
        Minion closestMinion = null;
        // assign single minion
        List<TroopNode> Mytroop = troopDataList;
        for (int i = 0; i < Mytroop.Count; i++)
        {
            List<Minion> minionList = Mytroop[i].GetMinionList();
            for (int j = 0; j < minionList.Count; j++)
            {
                if (closestMinion == null)
                {
                    if (minionList[j].CanAssign()) closestMinion = minionList[j];

                }
                else if (Vector3.Distance(minionList[j].transform.position, pos) < Vector3.Distance(closestMinion.transform.position, pos) && minionList[j].CanAssign())
                {
                    closestMinion = minionList[j];
                }
            }
        }
        return closestMinion;
    }

    
}
