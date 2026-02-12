using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Assets")]
    public GameObject islandPrefab;
    public CameraController mainCamera;
    public UIController uiController;


    [Header("State Machine")]
    private GameState _currentState;
    public BuildState buildState;
    public RollState rollState;
    public WarState warState;

    
    public Button changeStateBtn;

    public List<IslandController> AllIslands { get; private set; } = new List<IslandController>();

    void Awake()
    {
        Instance = this;
        
        changeStateBtn.onClick.AddListener(()=>{ChangeState(warState);});
        buildState = new BuildState(this);
        rollState = new RollState(this);
        warState = new WarState(this);
    }

    public void StartSinglePlayerGame()
    {
        Debug.Log("Starting Single Player...");

        ClearOldGame();

        SpawnIsland(0, Vector3.zero);

        SpawnIsland(1, new Vector3(50, 0, 0));

        ChangeState(buildState);
    }


    private void SpawnIsland(int id, Vector3 pos)
    {
        GameObject obj = Instantiate(islandPrefab, pos, Quaternion.identity);
        IslandController island = obj.GetComponent<IslandController>();
        
        island.Initialize(id);
        AllIslands.Add(island);

        if (id == 0)
        {
            if (mainCamera != null) mainCamera.FocusOnTarget(obj.transform.position);
            if (uiController != null) uiController.Initialize(island);
        }
    }

    private void ClearOldGame()
    {
        foreach (var island in AllIslands)
        {
            if (island != null) Destroy(island.gameObject);
        }
        AllIslands.Clear();
    }

    public void ChangeState(GameState newState)
    {
        if (_currentState != null) _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}