using UnityEngine;
using System.Collections;

using FortuneIslands.Economy.Stockpile;

namespace FortuneIslands.Building.Effects
{

    public class ResourceGenEffect : MonoBehaviour, IBuildingFeature
    {
        [Header("Settings")]
        public ResourceType resourceType;
        public int amountPerTick = 10;
        public float timeBetweenTicks = 5f;

        private ResourceManager _resources;
        private bool _isActive = false;

        public void Initialize(BuildingContext ctx)
        {
            _resources = ctx.Resources;
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
        //         Log.Info($"Generated {amountPerTick} {resourceType}");
        //     }
        // }
    }
}