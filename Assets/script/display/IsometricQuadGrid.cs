using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent (typeof(BoxCollider))]
public class IsometricQuadGrid : MonoBehaviour
{

    public delegate void IsometricGridDelegate();
    public IsometricGridDelegate OnGridOver;
    public IsometricGridDelegate OnGridOut;
    public IsometricGridDelegate OnGridOverTileChanged;
    public IsometricGridDelegate OnGridNodeClick;
    public GridSetting gridSetting;
    private GridNode[][] grid;
    RaycastHit _hit3D;
    public IPickingFilter pickingFilter;
    protected GridNode currentPickedNode;
    public GameObject go;
    void Awake ()
    {
        if(string.IsNullOrEmpty(gridSetting.layerName))
        {
            gridSetting.layerName = "Isometric2.5";
        }

        pickingFilter = new DefaultPicking();
        gridSetting.layerMask = -1;
        createLayerForGrid();
	}

    void Start()
    {
        createGrid();
    }

    public IPickingFilter GetPickingFilter()
    {
        return pickingFilter;
    }

    public void SetPickingFilter(IPickingFilter filter)
    {
        pickingFilter = filter;
    }

    #region  create Layer For Grid intersection
    void createLayerForGrid()
    {
        gridSetting.layerMask = LayerMask.NameToLayer(gridSetting.layerName);

        #if UNITY_EDITOR

            if (gridSetting.layerMask == -1)
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty layers = tagManager.FindProperty("layers");
                if (layers != null && layers.isArray)
                {
                    bool isLayerExist = false;
                    if (!isLayerExist)
                    {
                        for (int i = layers.arraySize - 1; i >= 0; i++)
                        {
                            SerializedProperty property = layers.GetArrayElementAtIndex(i);
                            if (property != null && string.IsNullOrEmpty(property.stringValue))
                            {
                                property.stringValue = gridSetting.layerName;
                                isLayerExist = true;
                                break;
                            }
                        }
                        tagManager.ApplyModifiedProperties();
                    }

                    if (isLayerExist)
                    {
                        Debug.LogWarning("\"" + gridSetting.layerName + "\" named layer is created. This will not work in other platforms but in Unity editor.  Please add a layer manually and define it to component!");
                        gridSetting.layerMask = LayerMask.NameToLayer(gridSetting.layerName);
                        gameObject.layer = gridSetting.layerMask;
                    }
                    else
                    {
                        Debug.LogWarning("\"" + gridSetting.layerName + "\" Layer could not found! This will effect user controls on grid! Create layer manually or fix errors");
                    }
                }
                else
                {
                    Debug.LogWarning("\"" + gridSetting.layerName + "\" Could not reach \"Layers\" property");
                }
            }





#endif


        if (gridSetting.layerMask != -1)
        {
            gridSetting.layerMask = LayerMask.NameToLayer(gridSetting.layerName);
            gameObject.layer = gridSetting.layerMask;
        }
        else
        {
            Debug.Log("dsfsd");
        }


    } // createLayerForGrid END
    #endregion

    #region Grid preparing
    private void createGrid()
    {
        GridNode gridNode;
        gridSetting.nodeSize  = new Vector2(1f / (float)gridSetting.countX, 1f / (float)gridSetting.countY);
        Vector2 halfSize = gridSetting.nodeSize / 2f;
        

        grid = new GridNode[gridSetting.countX][];
        float x;
        float y;
        for (int i = 0; i < gridSetting.countX; i++)
        {
            grid[i] = new GridNode[gridSetting.countY];
            for (int j = 0; j < gridSetting.countY; j++)
            {
                gridNode = new GridNode();
                gridNode.xNodeIndex = i;
                gridNode.yNodeIndex = j;
                gridNode.isValid = true;
                gridNode.parent = this.transform;
                gridNode.localSize = gridSetting.nodeSize;
                x = (halfSize.x + (i * gridSetting.nodeSize.x)) - 0.5f;
                y = (halfSize.y + (j * gridSetting.nodeSize.y)) - 0.5f;
                gridNode.localPosition = new Vector2(x, y);
                gridNode.publicPosition = transform.TransformPoint(gridNode.localPosition);
                
                grid[i][j] = gridNode;
            }
        }
    }

    public void ResetNodes()
    {
        createGrid();
    }
    #endregion

    #region Node pickeng 

    #region Node Mosue picking
    public GridNode GetNodeByScreenPosition(Vector3 mousePositions)
    {
        if (pickingFilter.isAllowedToPick())
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePositions);
            Ray ray = Camera.main.ScreenPointToRay(mousePositions);
            bool isHit = Physics.Raycast(ray, out _hit3D, 1000.0f, 1 << gridSetting.layerMask);
            if (isHit)
            {

                Vector3 hitPoint = _hit3D.point;
                Vector3 gridLocalCoordinates = transform.InverseTransformPoint(hitPoint);
                Vector3 halfQuad = new Vector3(0.5f, 0.5f);
                Vector3 coordinates = gridLocalCoordinates + halfQuad;
                Vector2 gridNodeSize = gridSetting.nodeSize;
                int tileX = (int)Mathf.Floor(coordinates.x / gridNodeSize.x);
                int tileY = (int)Mathf.Floor(coordinates.y / gridNodeSize.y);

                return GetNodeByIndex(tileX, tileY);
            }
        }

        return null;
    }
    #endregion

    #region getNodeByIndex
    public GridNode GetNodeByIndex(int xIndex, int yIndex)
    {
        if (IsGridNodeExist(xIndex, yIndex))
        {
            return grid[xIndex][yIndex];
        }
        return null;
    }
    #endregion



    GridNode downGridNode;
    void OnMouseDown()
    {
        if(OnGridNodeClick != null && currentPickedNode != null)
        {
            downGridNode = currentPickedNode;
        }
    }

    void OnMouseUp()
    {
        if (OnGridNodeClick != null && downGridNode != null && currentPickedNode != null)
        {
            downGridNode = null;
            OnGridNodeClick();
        }
    }


    public bool IsGridNodeExist(int xIndex, int yIndex)
    {
        if(xIndex > - 1 && yIndex > -1)
        {
            if(xIndex  < grid.Length && yIndex < grid[0].Length)
            {
                return true;
            }
        }

        return false;
    }


    public GridNode GetCurrentNode()
    {
        return currentPickedNode;
    }


    public GridNode GetCurrentNodeRespectToBorders(int xCount, int yCount)
    {

        if (currentPickedNode == null) return null;

        if (xCount == 1 && yCount == 1) return currentPickedNode;


        int currentXIndex = currentPickedNode.xNodeIndex;
        int currentYIndex = currentPickedNode.yNodeIndex;

        int minX = (int)Mathf.Floor(xCount / 2f);
        int maxX = currentPickedNode.xNodeIndex + minX;

        if (xCount % 2 == 0)
        {
            minX -= 1;
        }

        int gLength = grid.Length - 1;
        if (currentXIndex - minX < 0)
        {
            currentXIndex  =  Mathf.Abs(currentXIndex - minX);
        }
        else if(maxX > gLength)
        {
            currentXIndex = gLength - ( maxX - gLength);
        }






        int maxY = (int)Mathf.Floor(yCount / 2f);
        int minY = currentPickedNode.yNodeIndex - maxY;

        if (yCount % 2 == 0)
        {
            minY += 1;
        }


        int gLength2 = grid[0].Length - 1;


        if (minY < 0)
        {
            currentYIndex = Mathf.Abs(currentYIndex - minY);
        }
        else if ((currentYIndex + maxY) > gLength2)
        {
            currentYIndex = gLength2 - maxY;
        }





        return GetNodeByIndex(currentXIndex, currentYIndex);
    }


    public List<GridNode> GetBodeNeigbours(GridNode node, int xCount, int yCount)
    {
        List<GridNode> nodes = new List<GridNode>();

        if(xCount == 1 && yCount == 1)
        {
            nodes.Add(node);
            return  nodes;
        }


        int minX = (int)Mathf.Floor(xCount / 2f);
        float maxX = node.xNodeIndex + minX;

        if (xCount % 2 == 0)
        {
            minX -= 1;
        }

        minX = node.xNodeIndex - minX;

        int maxY = (int)Mathf.Floor(yCount / 2f);
        int minY = node.yNodeIndex - maxY;
       
        if (yCount % 2 == 0)
        {
            minY += 1;
        }
        maxY = node.yNodeIndex + maxY;


        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                nodes.Add(GetNodeByIndex(i, j));
            }
        }

        return nodes;
    }


    #endregion

    #region Blocking or Allowing part

 
    public void BlockNeighbourNodes(GridNode node, int xCount, int yCount)
    {
        List<GridNode> nodes = GetBodeNeigbours(node, xCount, yCount);
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].isValid = false;
        }
    }

    public void AllowNeighbourNodes(GridNode node, int xCount, int yCount)
    {
        List<GridNode> nodes = GetBodeNeigbours(node, xCount, yCount);
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].isValid = true;
        }
    }

    public void BlockNodeByIndex(int x, int y)
    {
        GridNode node = GetNodeByIndex(x,y);
        if(node  != null)
        {
            node.isValid = false;
        }
    }


    public void BlockNode(GridNode node)
    {
        if (node != null)
        {
            node.isValid = false;
        }
    }
    #endregion

  
    void Update ()
    {


        if (OnGridOut != null || OnGridOver != null || OnGridOverTileChanged != null || OnGridNodeClick != null)
        {
            GridNode gn = GetNodeByScreenPosition(Input.mousePosition);
            GridNode _tempForCurrent = currentPickedNode;
            currentPickedNode = gn;
            if (OnGridOut != null)
            {
                if(_tempForCurrent != null && gn == null)
                {
                    OnGridOut();
                }
            }

            if (OnGridOver != null)
            {
                if (_tempForCurrent == null && gn != null)
                {
                    OnGridOver();
                }
            }

            if (OnGridOverTileChanged != null)
            {
                if(gn != _tempForCurrent)
                {
                    OnGridOverTileChanged();
                }
            }

        }

    }




    public interface IPickingFilter
    {
        bool isAllowedToPick();
    }

    public class DefaultPicking : IPickingFilter
    {
        public bool isAllowedToPick()
        {
            return UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }


    [System.Serializable]
    public struct GridSetting
    {
        public int countX;
        public int countY;
        public Vector2 nodeSize;
        public string layerName;
        public int layerMask;
    }

 
}
