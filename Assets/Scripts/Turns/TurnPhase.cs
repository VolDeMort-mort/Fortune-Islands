namespace FortuneIslands.Turns
{
    public enum TurnPhase
    {
        // Players spend resources on buildings. Every round starts here.
        Build,

        // War rounds only: players mark where their warriors should sail.
        WarPlanning,

        // War rounds only: the marked attacks play out. Ends when the battle is resolved.
        WarBattle,

        // Every building rolls its dice and players collect resources.
        Roll
    }
}
