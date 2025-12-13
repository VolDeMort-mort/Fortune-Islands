using TMPro;
using UnityEngine;

public class ResourceListGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    private ResourceManager _resourceManager;

    [Header("Settings")]
    public GameObject resourceItemPrefab;   
    public Transform container;             
    public ResourceDisplayStyle listStyle; 

    public void Initialize(ResourceManager manager)
    {
        _resourceManager = manager;
        GenerateList();
    }

    void GenerateList()
    {
        Debug.Log($"Generating resource panel");
        // 1. Clear old placeholders
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. Loop through all 8 resources
        foreach (var def in _resourceManager.allResources)
        {
            Debug.Log($"Currently watching {def.type} {def.style}");
            if (def.style == listStyle){
                Debug.Log($"Currently working on {def.type} {def.style}");
                GameObject newItem = Instantiate(resourceItemPrefab, container);
                ResourceItemUI script = newItem.GetComponent<ResourceItemUI>();
                
                if (script != null)
                {
                    // Pass the style specific to THIS generator
                    script.Setup(_resourceManager, def, listStyle);
                }
            }
        }
    }
}