// IBuildingFeature.cs
using FortuneIslands.Game;

namespace FortuneIslands.Building.Effects
{

    public interface IBuildingFeature
    {
        // Called when the building is finished/placed
        void Initialize(IslandController island);
    }
}