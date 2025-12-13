using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Child Components")]
    public ResourceListGenerator topPanelGenerator; 
    public ResourceListGenerator sidePanelGenerator;

    [Header("Other UI")]
    public GameObject buildingMenuPanel; 
    public Button toggleBuildButton;


    public void Initialize(IslandController localIsland)
    {
        if (topPanelGenerator != null) 
            topPanelGenerator.Initialize(localIsland.resourceManager);

        if (sidePanelGenerator != null) 
            sidePanelGenerator.Initialize(localIsland.resourceManager);
    }
}