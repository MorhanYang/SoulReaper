using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeadIconManager 
{

    private static Dictionary<string, Sprite> headIconDic;

    // construct
    static HeadIconManager()
    {
        headIconDic = new Dictionary<string, Sprite>();
        Initialize();
    }
    private static void Initialize()
    {
        headIconDic.Add("Select", Resources.Load<Sprite>("HeadIcon/HeadIcon_Sel"));
        headIconDic.Add("Absorb", Resources.Load<Sprite>("HeadIcon/HeadIcon_Absorb"));
        headIconDic.Add("Alert", Resources.Load<Sprite>("HeadIcon/HeadIcon_Alert"));
        headIconDic.Add("Revive", Resources.Load<Sprite>("HeadIcon/HeadIcon_Revive"));
        headIconDic.Add("ReviveSel", Resources.Load<Sprite>("HeadIcon/HeadIcon_Revive_Sel"));
    }

    public static Sprite GetSprite(string spriteName)
    {
        return headIconDic[spriteName];
    }

}
