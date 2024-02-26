using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TroopManager : MonoBehaviour
{

    PlayerHealth playerHealth;

    public int maxTroopCapacity = 5; // it equal to the number of Minion's bar UI in Branch tree
    private int troopSlotNum = 3; // it equal to the number of Troop UI in Branch tree
    int hpUnit = 10;


    [SerializeField] BranchTreeUI branchTreeUI;
    TroopNode SelectedTroop;
    Minion SelectedMinion;
   

    public List<TroopNode> TroopDataList;

    private int presentExtraHealth;
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
        branchTreeUI.RefreshNodeUI(TroopDataList);
    }

    private void Update()
    {
        // test
        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < TroopDataList[0].minionList.Count; i++)
            {
                TroopDataList[0].minionList[i].TakeDamage(5, this.transform, Vector3.one);
            }
        }

    }

    //**************************************************************** SetUp *************************************************************

    public void SetPresentTroop( TroopNode troopNode ){

        SelectedTroop = troopNode; 
    }

    public void SetPresentMinion( Minion minion){

        SelectedMinion = minion; 
    }

    private void SetUpTroopNodeList()
    {
        TroopDataList = new List<TroopNode>();
        for (int i = 0; i < troopSlotNum; i++)
        {
            TroopDataList.Add(new TroopNode(i, TroopNode.NodeType.ExtraHp, playerHealth.ExtraHpNodeMaxHp, new List<Minion>()));
        }
    }

    //*************************************************************** Revieve Troop ***************************************************
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
                for (int j = 0; j < TroopDataList.Count; j++)
                {
                    TroopNode TargetTroop = TroopDataList[j];
                    // change it to Troop
                    if (TargetTroop.type == TroopNode.NodeType.ExtraHp){

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
                        break;
                    }
                }
            }

            branchTreeUI.RefreshNodeUI(TroopDataList);
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
    public void EatTroopToRecover()
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
            for (int i = 0; i < TroopDataList.Count; i++)
            {
                KillTroopOfMinions(TroopDataList[i]);
            }
        }

        // refresh 
        branchTreeUI.RefreshNodeUI(TroopDataList);
    }
    
    public void KillTroopOfMinions( TroopNode troop )
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
            troop.minionList[i].SetMinionDataPos(-1,-1);
            // count hp
            float hpFromMinion = hpUnit * troop.minionList[i].GetHealthPercentage();
            presentTroopHp += hpFromMinion;
        }
        troop.minionList.Clear();

        // change node type
        troop.ChangeTroopNodeType(TroopNode.NodeType.ExtraHp);
        troop.ChangeTroopHp(presentTroopHp);

    }
    
    
    public void EnemyKillOneMinion( Minion wantToKill )
    {
        wantToKill.SetInactive();
        // find where the minion is in dataList;
        int[] minionPos = wantToKill.GetMinionDataPos();
        TroopNode targetTroop = TroopDataList[minionPos[0]];
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
        branchTreeUI.RefreshNodeUI(TroopDataList);
    }
    //**************************************************************** Assign Troop *****************************************************
    public void AssignOneMinion(Vector3 aimPos)
    {

        Minion myMinion = GetClosedMinion(aimPos);

        // excecute assignment
        if (myMinion != null)
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

            // ************* Execute sprint action
            // didn't hit any thing
            if (target == null) 
            {
                // find pos at the navmesh
                NavMeshHit hit;
                Vector3 sprintPos = Vector3.zero;
                if (NavMesh.SamplePosition(aimPos, out hit, 2.5f, NavMesh.AllAreas))
                {
                    sprintPos = hit.position;
                }

                // assign minions
                myMinion.SprintToPos(sprintPos);

            }
            // Hit something
            if (target != null) 
            {
                // assign minion to attack
                myMinion.SprintToEnemy(target);
            }

            // play sound
            // mySoundManagers.PlaySoundAt(transform.position, "AssignMinion", false, false, 1.5f, 1f, 100, 100);
        }
    }

    public void AssignAllMinions(Vector3 aimPos)
    {
        List<TroopNode> myTroopList = TroopDataList;

        // use collider to detect enemies
        Transform target = null;
        Collider[] hitedEnemy = Physics.OverlapSphere(aimPos, 0.3f, LayerMask.GetMask("Enemy", "PuzzleTrigger"));

        // hit enemies. find closed one
        if (hitedEnemy.Length > 0) target = GetClosestEnemyTransform(hitedEnemy, aimPos);
        // if didn't hit a enemy
        else target = null;

        // show destination marker
        assignMarker.relocateMarker(aimPos, target);

        // ***********Execute sprint action
        // didn't hit any enemy
        if (target == null)
        {

            // find pos at the navmesh
            NavMeshHit hit;
            Vector3 sprintPos = Vector3.zero;
            if (NavMesh.SamplePosition(aimPos, out hit, 2.5f, NavMesh.AllAreas))
            {
                sprintPos = hit.position;
            }
            // assign minions

            // 3 conditions:
            //1 player select player icon - > assign all minion
            if (SelectedTroop != null) // controll specific troop
            {

            }

            for (int i = 0; i < myTroopList.Count; i++)
            {
                for (int j = 0; j < myTroopList[i].GetMinionList().Count; j++)
                {
                    myTroopList[i].GetMinionList()[j].SprintToPos(sprintPos);
                }
            }
        }

        // Hit something
        if (target != null)
        {
            for (int i = 0; i < myTroopList.Count; i++)
            {
                for (int j = 0; j < myTroopList[i].GetMinionList().Count; j++)
                {
                    myTroopList[i].GetMinionList()[j].SprintToEnemy(target);
                }
            }
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
            else if (enemyList[i].GetComponent<Enemy>())
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
        List<TroopNode> Mytroop = TroopDataList;
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
