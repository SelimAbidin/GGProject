using UnityEngine;
using System.Collections;

public abstract class UIScreen : MonoBehaviour
{
    public delegate void ScreenDelegate(UIScreen screen);
    public ScreenDelegate onScreenEnterComplete;
    public ScreenDelegate onScreenLeaveComplete;

    public string sceneName;


    private static int screenIDCounter;
    void Awake()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = "screen_" + (screenIDCounter++);
        }

    }


    public abstract void EnterScene();
    public abstract void LeaveScene();



}
