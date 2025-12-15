
using Unity;
using UnityEngine;

public class GameStarter: MonoBehaviour
{
    public GameObject islandPrefab;
    public CameraController mainCamera;
    public UIController uiController;

    void Start()
    {
        // Spawn Player 1 Island
        SpawnIsland(0, new Vector3(0, 0, 0));
        // SpawnIsland(1, new Vector3(200, 0, 0));

        // Spawn Player 2 Island (Far away)
        // SpawnIsland(1, new Vector3(200, 0, 0));
    }

    void SpawnIsland(int playerID, Vector3 position)
    {
        GameObject islandObj = Instantiate(islandPrefab, position, Quaternion.identity);
        
        IslandController island = islandObj.GetComponent<IslandController>();
        
        // This links all the managers together
        island.Initialize(playerID); 

        // 2. Connect Camera
        if (mainCamera != null) 
            mainCamera.FocusOnTarget(island.transform.position);

        // 3. Connect UI (Clean and simple!)
        if (uiController != null)
        {
            uiController.Initialize(island);
        }
    }
    
}