using UnityEngine;
using System.Collections;

public class GridStyle : MonoBehaviour {

    public Vector2 gridSize = new Vector2(10,10);
	void Awake ()
    {
        GetComponent<Renderer>().material.SetTextureScale("_MainTex", gridSize);
    }


}
