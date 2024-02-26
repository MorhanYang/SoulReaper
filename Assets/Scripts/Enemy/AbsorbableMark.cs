using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorbableMark : MonoBehaviour
{
    TroopManager troopManager;

    public enum AbsorbType
    {
        Normal,
        Enemy,
        Minion,
        Troop,
    }
    public AbsorbType myAbsorbType;
    public float recoverAmount;


    //effect
    [SerializeField] GameObject recallingPartical;
    SoundManager mySoundManager;

    private void Start()
    {
        troopManager = PlayerManager.instance.player.GetComponent<TroopManager>();

        if (myAbsorbType == AbsorbType.Enemy || myAbsorbType == AbsorbType.Minion)
        {
            this.enabled = false;
        }
        mySoundManager = SoundManager.Instance;
    }

    public float EatThisToRecover(bool isAbsorb)
    {
        float realRecoverAmount = recoverAmount; ;
        // play effect
        if (isAbsorb){
            // play recall effect;
            GameObject effect = Instantiate(recallingPartical, transform.position, transform.rotation);
            effect.GetComponent<RecallingMinion>().AimTo(PlayerManager.instance.player.transform);

            // play sound
            mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Release", false, false, 1, 1, 100, 100);
        }

        // excute target script action
        switch (myAbsorbType)
        {
            case AbsorbType.Normal:
                GetComponent<BasicEnemy>().TakeLife();

                break;
            case AbsorbType.Enemy:
                break;
            case AbsorbType.Minion:
                Minion targetMinion = GetComponent<Minion>();
                // change recover rate
                realRecoverAmount = recoverAmount * targetMinion.GetHealthPercentage();
                troopManager.EnemyKillOneMinion(targetMinion);
                break;
            case AbsorbType.Troop:
                break;
            default:
                break;
        }
        // stop use this component
        this.enabled = false;

        return realRecoverAmount;
    }

    public void ChangeAbsorbType(AbsorbType type)
    {
        myAbsorbType = type;
    }

}
