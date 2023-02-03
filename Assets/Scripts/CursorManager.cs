using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    public Texture2D combatCursor;  
    public Texture2D talkCursor;
    public Texture2D recallCursor;

    private void Awake()
    {
        instance = this;
    }

    public void ActivateDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ActivateCombatCursor()
    {
        Cursor.SetCursor(combatCursor, new Vector2(combatCursor.width / 2, combatCursor.height / 2), CursorMode.Auto);
    }

    public void ActivateTalkCursor()
    {
        Cursor.SetCursor(talkCursor, new Vector2(talkCursor.width / 2, talkCursor.height / 2), CursorMode.Auto);
    }
    public void ActivateRecallCursor()
    {
        Cursor.SetCursor(recallCursor, new Vector2(recallCursor.width / 2, recallCursor.height / 2), CursorMode.Auto);
    }
}
