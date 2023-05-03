using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    public Texture2D combatCursor;  
    public Texture2D InvestigateCursor;
    public Texture2D recallCursor;

    private void Awake()
    {
        instance = this;
    }

    public void ActivateDefaultCursor()
    {
        Cursor.SetCursor(null, new Vector2(0, 1), CursorMode.Auto);
    }

    public void ActivateCombatCursor()
    {
        Cursor.SetCursor(combatCursor, new Vector2(0, 1), CursorMode.Auto);
    }

    public void ActivateInvestigateCursor()
    {
        Cursor.SetCursor(InvestigateCursor, new Vector2(0, 1), CursorMode.Auto);
    }
    public void ActivateRecallCursor()
    {
        Cursor.SetCursor(recallCursor, new Vector2(0, 1), CursorMode.Auto);
    }
}
