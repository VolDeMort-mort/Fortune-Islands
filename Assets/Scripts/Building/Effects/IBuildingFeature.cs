

using FortuneIslands.Economy;
using FortuneIslands.Economy.Stockpile;


namespace FortuneIslands.Building.Effects
{

    public readonly struct BuildingContext
    {
        public readonly ResourceManager Resources;
        public readonly DiceManager Dice;

        public BuildingContext(ResourceManager resources, DiceManager dice)
        {
            Resources = resources;
            Dice = dice;
        }
    }

    public interface IBuildingFeature
    {
        void Initialize(BuildingContext ctx);
    }
}