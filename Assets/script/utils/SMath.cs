using UnityEngine;
using System.Collections;

public class SMath : MonoBehaviour {

    public static Vector2 RotatedRectangleSize(float degree, Vector2 size)
    {
        float radian = degree * Mathf.Deg2Rad;
        float rotatedX = Mathf.Abs(size.x * Mathf.Sin(degree)) + Mathf.Abs(size.y * Mathf.Cos(degree));
        float rotatedY = Mathf.Abs(size.x * Mathf.Cos(degree)) + Mathf.Abs(size.y * Mathf.Sin(degree));
        return new Vector2(rotatedX, rotatedY);
    }



    public static Vector3 GetWorldScale(Vector3 worldScale, Transform parent)
    {
        while (parent != null)
        {
            worldScale = Vector3.Scale(worldScale, parent.localScale);
            parent = parent.parent;
        }
        return worldScale;
    }



}
