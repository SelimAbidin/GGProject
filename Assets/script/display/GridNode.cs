using UnityEngine;
using System.Collections;
public class GridNode
{
    public int xNodeIndex;
    public int yNodeIndex;
    public Vector2 publicPosition;
    public Vector2 localPosition;
    public Vector2 localSize;
    public Transform parent;
    public bool isValid;
    public object userData;
}
