using TMPro;
using UnityEngine;

using FortuneIslands.Core;
using FortuneIslands.Economy.Stockpile;

namespace FortuneIslands.UI
{

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
            Log.Info($"Generating resource panel");
            // 1. Clear old placeholders
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // 2. Loop through all 8 resources
            foreach (var def in _resourceManager.allResources)
            {
                Log.Info($"Currently watching {def.type} {def.style}");
                if (def.style == listStyle)
                {
                    Log.Info($"Currently working on {def.type} {def.style}");
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
}