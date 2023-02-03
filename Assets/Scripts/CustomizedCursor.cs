using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizedCursor : MonoBehaviour
{
    public static CustomizedCursor instance;

    public Texture2D defaultCursor;

    private void Awake()
    {
        instance= this;
    }

    public void ActivateDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, new Vector2(defaultCursor.width / 2, defaultCursor.height / 2), CursorMode.Auto);
    }
}
