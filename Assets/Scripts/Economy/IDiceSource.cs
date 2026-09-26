

namespace FortuneIslands.Economy
{

    public interface IDiceSource
    {
        Dice Dice { get; }
        bool IsActive { get; }
    }
}
