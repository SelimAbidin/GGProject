using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class GameManager : MonoBehaviour {

    public static GameManager instance;
    
    public GameObject unplaceableMarker;
    public GameObject placeableMarker;

    public GameObject gridGameObject;

    private IsometricQuadGrid grid;

    public GameObject[] Structures;

    public GameState[] gameStates;

    private GameState currentGameState;
    private Vector2 placerSize;


    private GameObject selectedStructure;
    private Sprite selectedStructureSprite;
    private Structure selectedStructureComponent;
    GameObject selectedGridStructure;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(instance);
	}
	

    void Start()
    {
        StartGame();
    }


    void StartGame()
    {
        grid = gridGameObject.GetComponent<IsometricQuadGrid>();

        List<SavableComponent> list = SaveLoadManager.Load();
        if(list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AddStructureBySaveData(list[i]);
            }
        }
      
    
        unplaceableMarker = Instantiate(unplaceableMarker);
        unplaceableMarker.SetActive(false);


        placeableMarker = Instantiate(placeableMarker);
        placeableMarker.SetActive(false);

        GridNode gridNode = grid.GetNodeByIndex(0, 0);

        Vector2 worldScale = SMath.GetWorldScale(gridNode.localSize, gridNode.parent);
        placerSize = (1 / placeableMarker.GetComponent<SpriteRenderer>().sprite.bounds.size.x) * worldScale;

        placeableMarker.transform.localScale = unplaceableMarker.transform.localScale = placerSize;

        SetGameStageByID(0);
        ArrangeLayouts();

    }


    public void AddStructureBySaveData(SavableComponent saveData)
    {
        GameObject structureGameObject = null;
        for (int i = 0; i < Structures.Length; i++)
        {
            if(Structures[i].name == saveData.name)
            {
                structureGameObject = ObjectPoolManager.Instantiate(Structures[i]);
            }
        }

        DisplayDelegate displayEventComponent = structureGameObject.GetComponent<DisplayDelegate>();
        displayEventComponent.OnDisableEvent += OnDestroyStructure;

        GridNode node = grid.GetNodeByIndex(saveData.gridIndeX, saveData.gridIndeY);

        Structure structureComponent = structureGameObject.GetComponent<Structure>();
        structureComponent.SetMainGridNode(node);
        List<GridNode> nodes = grid.GetBodeNeigbours(node, saveData.gridSizeX, saveData.gridSizeY);


        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].userData = structureGameObject;
        }


        grid.BlockNeighbourNodes(node, saveData.gridSizeX, saveData.gridSizeY);
        AdjustStructureForGrid(node, structureGameObject);

    }

    private void AdjustStructureForGrid(GridNode node,GameObject structureGameObject)
    {
        // Because the size is calculated in its own parent. We recursively multiply size with parents;
        Vector2 worldScale = SMath.GetWorldScale(node.localSize, node.parent);
        // since it is rotated grid and rectangle,  grid size will not be same when it is  rotated.  
        worldScale = SMath.RotatedRectangleSize(grid.transform.localRotation.z, worldScale);
        // sprite.bounds gives us texture size / pixel per unit; 
        // We only multiply with X bcoz it is the important one for isometric grid width sinnce sttructures are drawn isometric orientation
        Vector2 size = (1 / structureGameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x) * worldScale;

        Structure structureComponent = structureGameObject.GetComponent<Structure>();
        structureGameObject.transform.localScale = new Vector2(size.x * structureComponent.gridSizeX, size.y * structureComponent.gridSizeY);

        float xOffset = ((structureComponent.gridSizeX + 1) % 2) / 2f;
        float yOffset = ((structureComponent.gridSizeY + 1) % 2) / 2f;

        Vector2 gridLocalPosition = node.localPosition;
        gridLocalPosition += new Vector2(node.localSize.x * xOffset, node.localSize.y * yOffset);
        structureGameObject.transform.position = node.parent.TransformPoint(gridLocalPosition);
    }

    private void ArrangeLayouts()
    {
        grid.OnGridNodeClick += OnGridNodeSelection;
        if (gameStates[1].UIPanel != null)
        {
            FillStructureList();
        }
    }


    void  FillStructureList()
    {
        BuildingScreen buildingScreen = gameStates[1].UIPanel.GetComponent<BuildingScreen>();
        RectTransform rect = buildingScreen.listContent.GetComponent<RectTransform>();
        Transform listTransform = buildingScreen.listContent.transform;
        
        for (int i = 0; i < listTransform.childCount; i++)
        {
            GameObject gameObject = listTransform.GetChild(i).gameObject;
            ObjectPoolManager.Destroy(gameObject);
        }

        List<GameObject> visibleButtons = new List<GameObject>();

        for (int i = 0; i < Structures.Length; i++)
        {
            Structure structure = Structures[i].GetComponent<Structure>();
            string refName = ObjectPoolManager.GetRefName(structure.gameObject);
            GameObject buttonGameObject = ObjectPoolManager.Instantiate(buildingScreen.SampleStructureButton);
            StructureButtonCounter buttonCounter = buttonGameObject.GetComponent<StructureButtonCounter>();
            int count = structure.structureCount - ObjectPoolManager.getActiveCountsByName(refName);

            if (count > 0 || structure.structureCount== -1)
            {
                buttonCounter.SetCountText(count);
                buttonCounter.refStructureName = refName;
                buttonCounter.maxCount = structure.structureCount;
                Button buttonComponent = buttonGameObject.GetComponent<Button>();
                buttonGameObject.transform.SetParent(buildingScreen.listContent.transform);
                buttonGameObject.transform.localScale = new Vector3(1, 1, 1);
                int index = i;
                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(() => StartStructurePlacement(buttonGameObject, index));
                buttonComponent.image.overrideSprite = structure.buttonSprite;
                 visibleButtons.Add(buttonGameObject);
            }
            else
            {
                ObjectPoolManager.Destroy(buttonGameObject);
            }

        }



        float fullwidth = 55 * visibleButtons.Count;
        rect.sizeDelta = new Vector2(fullwidth, rect.sizeDelta.y);
        for (int i = 0; i < visibleButtons.Count; i++)
        {
            GameObject buttonGameObject = visibleButtons[i];
            float nextWidth = 27.5f + (55 * i);
            buttonGameObject.transform.localPosition = new Vector3(nextWidth, -27.5f, 0);
        }

    }



    void OnGridNodeSelection()
    {
        if(currentGameState.stateName == "GAME_STATE")
        {
            GridNode node = grid.GetCurrentNode();

            if(node != null)
            {
                if (node.userData != null)
                {
                    selectedGridStructure = (GameObject)node.userData;
                    selectedGridStructure.transform.localPosition += new Vector3(0,3,0);
                    SetGameStageByID(2);
                }
              
            }
   
        }
    }





    void StartStructurePlacement(GameObject buttonGameObject, int index)
    {
        string refName = ObjectPoolManager.GetRefName(Structures[index]);
        int count = ObjectPoolManager.getActiveCountsByName(refName);
        Structure structure = Structures[index].GetComponent<Structure>();
       
        if(selectedStructure != null)
        {
            UnSelectSelectedStructure();
        }


        if (structure.structureCount > count || structure.structureCount == -1)
        {
            selectedStructure = ObjectPoolManager.Instantiate(Structures[index]);
            DisplayDelegate displayEventComponent = selectedStructure.GetComponent<DisplayDelegate>();
            displayEventComponent.OnDisableEvent += OnDestroyStructure;
            displayEventComponent.UserData = buttonGameObject;
            selectedStructureSprite = selectedStructure.GetComponent<SpriteRenderer>().sprite;
            selectedStructureComponent = selectedStructure.GetComponent<Structure>();
            int currentCount = structure.structureCount - ObjectPoolManager.getActiveCountsByName(refName);
            buttonGameObject.GetComponent<StructureButtonCounter>().SetCountText(currentCount);
            grid.OnGridOverTileChanged += OnGridTileChange;
            grid.OnGridNodeClick += OnGridNodeClick;
        }
    }

    private void UnSelectSelectedStructure()
    {
        if(selectedStructure != null)
        {
            DisplayDelegate displayEventComponent = selectedStructure.GetComponent<DisplayDelegate>();
            displayEventComponent.OnDisableEvent -= OnDestroyStructure;
            grid.OnGridOverTileChanged -= OnGridTileChange;
            grid.OnGridNodeClick -= OnGridNodeClick;

            ObjectPoolManager.Destroy(selectedStructure);
            selectedStructure = null;
            selectedStructureSprite = null;
            selectedStructureComponent = null;
            FillStructureList();
        }
       
    }

    void OnDestroyStructure(DisplayDelegate displayEvent)
    {
        // TODO there is a bug when exit from application in editor. 
        if (this != null)
        {
            // delegate triggered just before being disabled. In order to get correct counting,  will skip one frame;
            StartCoroutine(StructureDisabled(displayEvent));
        }
    }

    IEnumerator StructureDisabled(DisplayDelegate displayEvent)
    {
        yield return new WaitForEndOfFrame();
        displayEvent.OnDisableEvent -= OnDestroyStructure;
        Structure structure = displayEvent.gameObject.GetComponent<Structure>();
        string refName = ObjectPoolManager.GetRefName(displayEvent.gameObject);
        int currentCount = structure.structureCount - ObjectPoolManager.getActiveCountsByName(refName);
        GameObject structureGameObject = (GameObject)displayEvent.UserData;
        GridNode mainNode = structure.GetMainGridNode();
        grid.AllowNeighbourNodes(mainNode, structure.gridSizeX, structure.gridSizeY);
        if (structureGameObject != null)
        {
            structureGameObject.GetComponent<StructureButtonCounter>().SetCountText(currentCount);
        }
        else
        {
            ResetStructureButtons();
        }
    }



    private void ResetStructureButtons()
    {
        if(gameStates[1].UIPanel != null)
        {
            BuildingScreen buildingScreen = gameStates[1].UIPanel.GetComponent<BuildingScreen>();
            int childCount = buildingScreen.listContent.transform.childCount;
            GameObject _tempGameObject;
            for (int i = 0; i < childCount; i++)
            {
                _tempGameObject = buildingScreen.listContent.transform.GetChild(i).gameObject;
                StructureButtonCounter butonCounter = _tempGameObject.GetComponent<StructureButtonCounter>();
                string refName = butonCounter.refStructureName;
                int currentCount = butonCounter.maxCount - ObjectPoolManager.getActiveCountsByName(refName);
                butonCounter.SetCountText(currentCount);
            }
        }

               


    }

    void OnGridNodeClick()
    {
        GridNode node = grid.GetCurrentNodeRespectToBorders(selectedStructureComponent.gridSizeX, selectedStructureComponent.gridSizeY);
        if(node != null && node.isValid  && selectedStructure != null)
        {
            List<GridNode> nodes = grid.GetBodeNeigbours(node, selectedStructureComponent.gridSizeX, selectedStructureComponent.gridSizeY);
            for (int i = 0; i < nodes.Count; i++)
            {
                if(!nodes[i].isValid)
                {
                    return;
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].userData = selectedStructure;
                grid.BlockNode(nodes[i]);
            }
            selectedStructureComponent.SetMainGridNode(node);

            SavableComponent saveComponent = new SavableComponent(selectedStructure); 
            SaveLoadManager.Save(saveComponent);
            selectedStructure = null;
            selectedStructureSprite = null;
            selectedStructureComponent = null;
            grid.OnGridOverTileChanged -= OnGridTileChange;
            grid.OnGridNodeClick -= OnGridNodeClick;
            placeableMarker.SetActive(false);
            unplaceableMarker.SetActive(false);
            FillStructureList();
        }
    }


    void OnGridTileChange()
    {
        GridNode gridNode = grid.GetCurrentNodeRespectToBorders(selectedStructureComponent.gridSizeX, selectedStructureComponent.gridSizeY);

        if (gridNode != null)
        {
            selectedStructure.SetActive(true);
            Vector2 gridLocalPosition = gridNode.localPosition;
            List<GridNode> nodes = grid.GetBodeNeigbours(gridNode, selectedStructureComponent.gridSizeX, selectedStructureComponent.gridSizeY);
            bool isValidPlace = true;
            for (int i = 0; i < nodes.Count; i++)
            {
                if(!nodes[i].isValid)
                {
                    isValidPlace = false;
                    break;
                }
            }

            placeableMarker.SetActive(isValidPlace);
            unplaceableMarker.SetActive(!isValidPlace);
            float xOffset = ((selectedStructureComponent.gridSizeX + 1) % 2) / 2f;
            float yOffset = ((selectedStructureComponent.gridSizeY + 1) % 2) / 2f;
            gridLocalPosition += new Vector2(gridNode.localSize.x * xOffset, gridNode.localSize.y * yOffset); ;
            selectedStructure.transform.position = gridNode.parent.TransformPoint(gridLocalPosition);
            placeableMarker.transform.position = unplaceableMarker.transform.position = selectedStructure.transform.position;

            placeableMarker.transform.localScale = unplaceableMarker.transform.localScale = new Vector2(selectedStructureComponent.gridSizeX * placerSize.x, selectedStructureComponent.gridSizeY * placerSize.y);

            AdjustStructureForGrid(gridNode, selectedStructure);
          
        }
        else
        {
            selectedStructure.transform.localScale = Vector3.zero;
            placeableMarker.SetActive(false);
            unplaceableMarker.SetActive(false);

        }

    }


    public void SetGameStageByID(int stateID)
    {

        if (stateID > -1 && stateID < gameStates.Length)
        {

            SetGameStateByObject(gameStates[stateID]);
        }
    }

    public GameState nextState;
    public void SetGameStateByObject(GameState newGameState)
    {
        if(currentGameState != null && currentGameState.stateName == "BUILD_STATE")
        {
            if(selectedStructure != null)
            {
                UnSelectSelectedStructure();
                FillStructureList();
            }
        }
        
        if (newGameState.stateName == "INFO_STATE")
        {
            if (gameStates[2].UIPanel != null)
            {
                Transform transform = gameStates[2].UIPanel.transform.FindChild("popup");

                Image image = transform.gameObject.GetComponent<Image>();

                Color popupBackground;
                if (selectedGridStructure != null)
                {
                    popupBackground = selectedGridStructure.GetComponent<Structure>().popupBackgroundColor;

                }
                else
                {
                    popupBackground = Color.white;
                }
                Color backgroundColor = popupBackground;// new Color(UnityEngine.Random.Range(0, 1000) / 1000f, UnityEngine.Random.Range(0, 1000) / 1000f, UnityEngine.Random.Range(0, 1000) / 1000f); 
                image.color = backgroundColor;
                Debug.Log(backgroundColor.a);
            }
        }



        if (currentGameState != newGameState)
        {
            if (currentGameState != null)
            {
                nextState = newGameState;
                UIScreen screen = currentGameState.UIPanel.GetComponent<UIScreen>();
                screen.onScreenLeaveComplete += OnScreenLeave;
                screen.LeaveScene();
            }
            else
            {
                currentGameState = newGameState;
                nextState = null;
                UIScreen newscreen = currentGameState.UIPanel.GetComponent<UIScreen>();
                newscreen.onScreenEnterComplete += OnScreenEnter;
                newscreen.EnterScene();
            }
        }
    }


    private void OnScreenLeave(UIScreen screen)
    {
        screen.onScreenLeaveComplete -= OnScreenLeave;
        if (nextState.UIPanel != null)
        {
            UIScreen newscreen = nextState.UIPanel.GetComponent<UIScreen>();
            newscreen.onScreenEnterComplete += OnScreenEnter;
            newscreen.EnterScene();
            currentGameState = nextState;
        }
        else
        {
            Debug.Log("UI Screen component has no panel instance");
        }

        nextState = null;
    }

    private void OnScreenEnter(UIScreen screen)
    {
        screen.onScreenEnterComplete-= OnScreenEnter;
    }





    public void ClearGrid()
    {
        for (int i = 0; i < Structures.Length; i++)
        {
            string refName = ObjectPoolManager.GetRefName(Structures[i]);
            int count = ObjectPoolManager.getActiveCountsByName(refName);
            if(count  > 0)
            {
                ObjectPoolManager.DestroyAllByRefName(refName);
            }
        }


        SaveLoadManager.Reset();
        FillStructureList();
        grid.ResetNodes();
        //PutStructureListInOrder();
    }




    [System.Serializable]
    public class GameState
    {
        public string stateName;
        public GameObject UIPanel;
    }

}
