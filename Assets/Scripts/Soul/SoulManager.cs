using UnityEngine;

public class SoulManager : MonoBehaviour
{
    enum SoulScript
    {
        Regular,
        Bomb,
        Boomerang,
        Kite,
    }
    [SerializeField] SoulScript soulScript;
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
            case SoulScript.Boomerang:
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().ShootSoul(dir);
                break;
            default:
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
            case SoulScript.Boomerang:
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().RecallFunction();
                break;
            default:
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
            case SoulScript.Boomerang:
                break;
            case SoulScript.Kite:
                GetComponent<Soul_Kite>().ResetRecall();
                break;
            default:
                break;
        }
        
    }
}
