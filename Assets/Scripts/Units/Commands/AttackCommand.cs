using UnityEngine;

public class AttackCommand : IUnitCommand
{
    private Unit _target;

    public AttackCommand(Unit target)
    {
        _target = target;
    }

    public void Execute(UnitController unit)
    {
        // Tell the unit WHAT to do (Attack this specific object)
        unit.StartAttacking(_target);
    }
}