using UnityEngine;


using FortuneIslands.Economy;

namespace FortuneIslands.Building.Effects
{
    public class DiceProvider : MonoBehaviour, IBuildingFeature, IDiceSource
    {
        [Header("Dice Configuration")]
        // Drag your ScriptableObject here (e.g. "House Die")
        public Dice diceToProvide;

        private DiceManager _diceManager;

        public Dice Dice=>diceToProvide;

        public bool IsActive {get; private set;}

        public void Initialize(BuildingContext ctx)
        {
            _diceManager = ctx.Dice;
            _diceManager.Register(this);
            IsActive = true;
        }
        private void OnDestroy()
        {
            if (_diceManager != null) _diceManager.Unregister(this);
        }

    }
}