using UnityEngine;
using System.Collections;
using System;

public class Structure : MonoBehaviour,  ObjectPoolManager.IResetable
{
    public int structureCount = 1;
    public int gridSizeX = 1;
    public int gridSizeY = 1;
    public Sprite buttonSprite;
    protected bool isAdded = false;
    public Color popupBackgroundColor;


    private GridNode _mainGrid;
    

    public GridNode GetMainGridNode()
    {
        return _mainGrid;
    }

    public void SetMainGridNode(GridNode node)
    {
        _mainGrid = node;
    }

    public void OnInstantiate()
    {
        this.transform.localScale = Vector2.zero;
    }

    public void OnDestroy()
    {
        
    }
}
