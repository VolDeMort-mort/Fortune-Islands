using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [System.Serializable]
    public struct BuildingTypeBtn
    {
        public BuildingTypes buildingType;
        public Button btn;
    }

    [Header("Child Components")]
    public ResourceListGenerator topPanelGenerator; 
    public ResourceListGenerator sidePanelGenerator;

    [Header("Other UI")]
    public GameObject buildingMenuPanel; 
    public Button toggleBuildButton;
    public Button placeBuilding;
    public List<BuildingTypeBtn> placingBtns;


    public void Initialize(IslandController localIsland)
    {
        if (topPanelGenerator != null) 
            topPanelGenerator.Initialize(localIsland.resourceManager);

        if (sidePanelGenerator != null) 
            sidePanelGenerator.Initialize(localIsland.resourceManager);


        foreach(var pair in placingBtns){
            pair.btn.onClick.AddListener(()=>{localIsland.buildManager.StartPlacingBuilding(pair.buildingType);});
            Debug.Log($"{pair.buildingType}");
        }
    }
}