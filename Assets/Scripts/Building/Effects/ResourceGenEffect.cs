using UnityEngine;
using System.Collections;

public class ResourceGenEffect : MonoBehaviour, IBuildingFeature
{
    [Header("Settings")]
    public ResourceType resourceType;
    public int amountPerTick = 10;
    public float timeBetweenTicks = 5f;

    private IslandController _island;
    private bool _isActive = false;

    public void Initialize(IslandController island)
    {
        _island = island;
        _isActive = true;
        // StartCoroutine(GenerateRoutine());
    }

    // private IEnumerator GenerateRoutine()
    // {
    //     while (_isActive)
    //     {
    //         yield return new WaitForSeconds(timeBetweenTicks);
            
    //         // "Effect they give later"
    //         _island.resourceManager.AddResource(resourceType, amountPerTick);
            
    //         // Optional: Pop up UI text here "+10 Gold"
    //         Debug.Log($"Generated {amountPerTick} {resourceType}");
    //     }
    // }
}