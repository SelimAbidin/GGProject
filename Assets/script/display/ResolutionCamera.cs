using UnityEngine;
using System.Collections;
using System;

public class ResolutionCamera : MonoBehaviour
{
    public Vector2 screenResolution = new Vector2(480, 330);
    Camera currentCamera;

    void Awake ()
    {
        currentCamera = this.GetComponent<Camera>();
        if(currentCamera.orthographic)
        {
            currentCamera.orthographicSize = screenResolution.y / 2;
            float screenStageOriantation = screenResolution.y / Screen.height;
            screenResolution.x = Screen.width * screenStageOriantation;
        }
        else
        {
            Debug.LogError("Component can be used in orthographic camera only");
        }

    }

}
