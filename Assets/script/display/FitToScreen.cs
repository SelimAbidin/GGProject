using UnityEngine;
using System.Collections;
using System;

public class FitToScreen : MonoBehaviour
{


    public enum FitTypes
    {
        FitToX,
        FitToY,
        FitToXY
    }

    public FitTypes fitType = FitTypes.FitToXY;

    private bool keepAspectRatio = true;

    private Vector2 previousScreenSize;
    void Start ()
    {
        OnScreenSizeChange();
    }


    private void OnScreenSizeChange()
    {
        previousScreenSize.x = Screen.width;
        previousScreenSize.y = Screen.height;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector2 realSpriteSize = spriteRenderer.sprite.bounds.size;
        float screenHeightSize = Camera.main.orthographicSize * 2;

        if (fitType == FitTypes.FitToX)
        {
            float screenWidthSize = screenHeightSize * Camera.main.aspect;
            float widthScale = screenWidthSize / realSpriteSize.x;

            if (keepAspectRatio)
                transform.localScale = new Vector3(widthScale, widthScale, transform.localScale.z);
            else
                transform.localScale = new Vector3(widthScale, transform.localScale.y, transform.localScale.z);
        }
        else if (fitType == FitTypes.FitToY)
        {
            float heightScale = screenHeightSize / realSpriteSize.y;
            if (keepAspectRatio)
                transform.localScale = new Vector3(heightScale, heightScale, transform.localScale.z);
            else
                transform.localScale = new Vector3(transform.localScale.x, heightScale, transform.localScale.z);
        }
        else
        {
            float screenWidthSize = screenHeightSize * Camera.main.aspect;
            float widthScale = screenWidthSize / realSpriteSize.x;
            float heightScale = screenHeightSize / realSpriteSize.y;

            transform.localScale = new Vector3(widthScale, heightScale, transform.localScale.z);
        }
    }
}
