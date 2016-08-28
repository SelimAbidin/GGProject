using UnityEngine;
using System.Collections;

[System.Serializable]
public class SavableComponent
{

    public string name = "";
    public float[] eulerAngles = new float[3];
    public int gridIndeX;
    public int gridIndeY;
    public int gridSizeX = 1;
    public int gridSizeY = 1;



    public SavableComponent (GameObject gameObject)
    {
        eulerAngles[0] = gameObject.transform.localEulerAngles.x;
        eulerAngles[1] = gameObject.transform.localEulerAngles.y;
        eulerAngles[2] = gameObject.transform.localEulerAngles.z;

        Structure structure = gameObject.GetComponent<Structure>();
        GridNode gridNode = structure.GetMainGridNode();
        gridIndeX = gridNode.xNodeIndex;
        gridIndeY = gridNode.yNodeIndex;
        gridSizeX = structure.gridSizeX;
        gridSizeY = structure.gridSizeY;
        name = gameObject.name;

    }
}
