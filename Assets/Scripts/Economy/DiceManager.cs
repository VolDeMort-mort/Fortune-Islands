using UnityEngine;
using System;
using System.Collections.Generic;

using FortuneIslands.Core;
using FortuneIslands.Economy.Stockpile;

namespace FortuneIslands.Economy
{
    public class DiceManager : MonoBehaviour
    {
        // Event: Sends the list of faces we rolled (for UI to display icons)
        public event Action<List<DiceFace>> OnDiceRolled;
        private readonly List<IDiceSource> _sources = new List<IDiceSource>();

        private ResourceManager _resources;

        public void Initialize(ResourceManager resources)
        {
            _resources = resources;
        }

        public void Register(IDiceSource source)
        {
            if (!_sources.Contains(source)) _sources.Add(source);
        }

        public void Unregister(IDiceSource source)
        {
            if (!_sources.Contains(source)) _sources.Add(source);
        }

        public void PerformRoll()
        {
            List<DiceFace> results = new List<DiceFace>(_sources.Count);
            Log.Info($"Dice sources: {_sources.Count}");


            foreach (var source in _sources)
            {
                if (!source.IsActive || source.Dice == null) continue;

                // The Dice Definition handles the random logic
                DiceFace result = source.Dice.Roll();
                results.Add(result);

                // 3. Apply Resource immediately (or wait for animation)
                if (result.amount > 0)
                {
                    Log.Info($"Add resources: {result.type} {result.amount}");
                    _resources.AddResource(result.type, result.amount);
                }
            }

            Log.Info($"Rolled {results.Count} dice.");

            // 4. Update UI
            OnDiceRolled?.Invoke(results);
        }
    }
}