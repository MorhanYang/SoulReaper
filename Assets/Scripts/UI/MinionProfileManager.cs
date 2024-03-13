using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MinionProfileManager 
{
    private static Dictionary<string, Sprite> minionProfileDic;

    static MinionProfileManager()
    {
        minionProfileDic = new Dictionary<string, Sprite>();
        Initialize();
    }

    private static void Initialize()
    {
        minionProfileDic.Add("Rat", Resources.Load<Sprite>("MinionProfile/MinionProfile_Rat"));
        minionProfileDic.Add("Normal", Resources.Load<Sprite>("MinionProfile/MinionProfile_Normal"));
        minionProfileDic.Add("Range", Resources.Load<Sprite>("MinionProfile/MinionProfile_Wizard"));
        minionProfileDic.Add("Dash", Resources.Load<Sprite>("MinionProfile/MinionProfile_Knight"));
        minionProfileDic.Add("Vine", Resources.Load<Sprite>("MinionProfile/MinionProfile_Vine"));
    }

    public static Sprite GetSprite( string spriteName )
    {
        return minionProfileDic[spriteName];
    }
    public static Sprite GetSpriteByMinionStyle( Minion.MinionStyle myStyle)
    {
        switch (myStyle)
        {
            case Minion.MinionStyle.defualt:
                return null;

            case Minion.MinionStyle.Rats:
                return minionProfileDic["Rat"];

            case Minion.MinionStyle.Normal:
                return minionProfileDic["Normal"];

            case Minion.MinionStyle.Range:
                return minionProfileDic["Range"];

            case Minion.MinionStyle.Dash:
                return minionProfileDic["Dash"];

            case Minion.MinionStyle.Vine:
                return minionProfileDic["Vine"];

            default:
                return null;

        }
    }
}
