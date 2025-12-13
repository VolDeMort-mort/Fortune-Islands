using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Dependencies")]
    public ResourceManager resourceManager;

    [Header("Generation Settings")]
    public GameObject resourceItemPrefab; // Your new Prefab
    public Transform resourceListContainer; // The Panel with Layout Group
     

    [Header("Other Panels")]
    public GameObject buildingMenuPanel; 
    public Button toggleBtn;

    void Start()
    {
        
        if (toggleBtn != null) 
             toggleBtn.onClick.AddListener(ToggleBuildMenu);
    } 

    public void ToggleBuildMenu()
    {
        if (buildingMenuPanel != null)
        {
            buildingMenuPanel.SetActive(!buildingMenuPanel.activeSelf);
        }
    }
}