using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using FortuneIslands.Building;
using FortuneIslands.Economy.Stockpile;
using FortuneIslands.WorldEntities;

namespace FortuneIslands.UI
{
    public class UIController : MonoBehaviour
    {
        [System.Serializable]
        public struct BuildingTypeBtn
        {
            public BuildingTypes buildingType;
            public Button btn;
        }

        // [Header("Main Menu")]
        // public GameObject mainMenuCanvas;
        // public GameObject inGameCanvas;


        [Header("Child Components")]
        public ResourceListGenerator topPanelGenerator;
        public ResourceListGenerator sidePanelGenerator;

        [Header("Other UI")]
        public GameObject buildingMenuPanel;
        public Button toggleBuildButton;
        public Button placeBuilding;
        public List<BuildingTypeBtn> placingBtns;


        public void Initialize(ResourceManager resources, BuildingManager buildings)
        {

            if (topPanelGenerator != null)
                topPanelGenerator.Initialize(resources);

            if (sidePanelGenerator != null)
                sidePanelGenerator.Initialize(resources);

            // mainMenuCanvas.SetActive(true);
            // inGameCanvas.SetActive(false);

            foreach (var pair in placingBtns)
            {
                var type = pair.buildingType;
                pair.btn.onClick.RemoveAllListeners();
                pair.btn.onClick.AddListener(() => { buildings.StartPlacingBuilding(type); });
            }
        }
    }
}