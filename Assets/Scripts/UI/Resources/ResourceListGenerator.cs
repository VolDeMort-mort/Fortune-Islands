using UnityEngine;

public class ResourceListGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    public ResourceManager resourceManager; // Drag GameManager here

    [Header("Settings")]
    public GameObject resourceItemPrefab;   // Drag the Prefab here
    public Transform container;             // Drag the Panel here

    // Change this in the Inspector for each panel!
    public ResourceDisplayStyle listStyle; 

    void Start()
    {
        GenerateList();
    }

    void GenerateList()
    {
        // 1. Clear old placeholders
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. Loop through all 8 resources
        foreach (var def in resourceManager.allResources)
        {
            Debug.Log($"Currently watching {def.type} {def.style}");
            if (def.style == listStyle){
                Debug.Log($"Currently working on {def.type} {def.style}");
                GameObject newItem = Instantiate(resourceItemPrefab, container);
                ResourceItemUI script = newItem.GetComponent<ResourceItemUI>();
                
                if (script != null)
                {
                    // Pass the style specific to THIS generator
                    script.Setup(resourceManager, def, listStyle);
                }
            }
        }
    }
}