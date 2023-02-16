using UnityEngine;

public class SoulManager : MonoBehaviour
{
    enum SoulScript
    {
        Regular,
        Bomb,
        Kite,
        Teleport,
        dash,
        parasite,
        Test,
    }
    [SerializeField] SoulScript soulScript;

    private void Start()
    {
        switch (soulScript)
        {
            case SoulScript.Regular:
                GetComponent<Soul_Regular>().soulType = 1;
                break;
            case SoulScript.Bomb:
                GetComponent<Soul_Bomb>().soulType = 2;
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().soulType = 3;
                break;
            case SoulScript.Teleport:
                GetComponent<Soul_Teleport>().soulType = 4;
                break;
            case SoulScript.dash:
                GetComponent<Soul_Dash>().soulType = 5;//testing
                break;
            case SoulScript.parasite:
                GetComponent<Soul_Parasite>().soulType = 6;//testing
                break;
            case SoulScript.Test:
                GetComponent<Soul_Parasite>().soulType = 0;//testing
                break;
        }
    }
    public void ShootSoul(Vector3 dir)
    {
        switch (soulScript)
        {
            case SoulScript.Regular:
                GetComponent<Soul_Regular>().ShootSoul(dir);
                break;
            case SoulScript.Bomb:
                GetComponent<Soul_Bomb>().ShootSoul(dir);
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().ShootSoul(dir);
                break;
            case SoulScript.Teleport:
                GetComponent<Soul_Teleport>().ShootSoul(dir);
                break;
            case SoulScript.dash:
                GetComponent<Soul_Dash>().ShootSoul(dir);
                break;
            case SoulScript.parasite:
                GetComponent<Soul_Parasite>().ShootSoul(dir);
                break;
            case SoulScript.Test:
                GetComponent<Soul_Parasite>().ShootSoul(dir);
                break;
        }
        
    }

    public void RecallFunction()
    {
        switch (soulScript)
        {
            case SoulScript.Regular:
                GetComponent<Soul_Regular>().RecallFunction();
                break;
            case SoulScript.Bomb:
                GetComponent<Soul_Bomb>().RecallFunction();
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().RecallFunction();
                break;
            case SoulScript.Teleport:
                GetComponent<Soul_Teleport>().RecallFunction();
                break;
            case SoulScript.dash:
                GetComponent<Soul_Dash>().RecallFunction();
                break;
            case SoulScript.parasite:
                GetComponent<Soul_Parasite>().RecallFunction();
                break;
            case SoulScript.Test:
                GetComponent<Soul_Parasite>().RecallFunction();
                break;
        }
    }

    public void ResetRecall()
    {
        switch (soulScript)
        {
            case SoulScript.Regular:
                GetComponent<Soul_Regular>().ResetRecall();
                break;
            case SoulScript.Bomb:
                GetComponent<Soul_Bomb>().ResetRecall();
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().ResetRecall();
                break;
            case SoulScript.Teleport:
                GetComponent<Soul_Teleport>().ResetRecall();
                break;
            case SoulScript.dash:
                GetComponent<Soul_Dash>().ResetRecall();
                break;
            case SoulScript.parasite:
                GetComponent<Soul_Parasite>().ResetRecall();
                break;
            case SoulScript.Test:
                GetComponent<Soul_Parasite>().ResetRecall();
                break;
        }
        
    }
}
