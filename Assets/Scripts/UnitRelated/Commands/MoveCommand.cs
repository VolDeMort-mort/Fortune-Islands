using UnityEngine;

public class MoveCommand : IUnitCommand
{
    private Vector2Int _target;

    public MoveCommand(Vector2Int targetPosition)
    {
        _target = targetPosition;
    }

    public void Execute(UnitController unit)
    {
        // We just tell the unit WHAT to do.
        // The unit handles the HOW (A* pathfinding, animation, etc.)
        unit.MoveTo(_target);
    }
}