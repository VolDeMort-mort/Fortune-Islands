using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour, ISelectable
{
    // --- Data Pattern ---
    public UnitStats stats; 

    // --- State ---
    private MapManager _mapRef;
    private Vector2Int _currentGridPos;
    private bool _isMoving = false;
    private Coroutine _moveRoutine;
    private Unit _unitComponent;

    private Coroutine _actionRoutine;

    // --- COMMAND PATTERN ENTRY POINT ---
// --- COMMAND ENTRY POINT ---
    public void ExecuteCommand(IUnitCommand command)
    {
        // Stop whatever we were doing (Moving OR Attacking)
        if (_actionRoutine != null) StopCoroutine(_actionRoutine);
        
        command.Execute(this);
    }

    // --- ATTACK LOGIC ---
    public void StartAttacking(Unit target)
    {
        _actionRoutine = StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(Unit target)
    {
        while (target != null && target.currentHealth > 0)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            // 1. CHECK RANGE
            if (dist > stats.attackRange)
            {
                // Too far! Move towards target
                // (Optimization: Only recalculate path every 0.5s, not every frame)
                if (!_isMoving) 
                {
                     // Reuse your existing move logic, but don't start a new coroutine, 
                     // just call the pathfinder helper directly or use a simplified move here.
                     // For simplicity, let's just move one step towards target:
                     MoveTo(new Vector2Int((int)target.transform.position.x, (int)target.transform.position.z));
                }
                yield return new WaitForSeconds(0.5f); // Wait a bit before checking range again
            }
            else
            {
                // 2. IN RANGE - ATTACK!
                // Stop moving first
                if (_isMoving) 
                {
                    StopCoroutine(_moveRoutine); // Stop the inner movement loop
                    _isMoving = false;
                }

                // Look at target
                transform.LookAt(target.transform);

                // Hit them
                target.TakeDamage(stats.attackDamage);
                
                // Visual Animation trigger would go here
                // animator.SetTrigger("Attack");

                // Wait Cooldown
                yield return new WaitForSeconds(stats.attackSpeed);
            }
        }

        // Target dead or null
        _actionRoutine = null;
    }
    public void Initialize(MapManager map, Vector2Int startPos, UnitStats unitStats)
    {
        _mapRef = map;
        stats = unitStats;
        _currentGridPos = startPos;
        _unitComponent = GetComponent<Unit>();

        // 1. Setup Position
        transform.localPosition = new Vector3(startPos.x, 1f, startPos.y);

        // 2. Register on Grid
        if (_mapRef.map.GetCell(startPos.x, startPos.y).OccupyingObject == null)
        {
            _mapRef.map.GetCell(startPos.x, startPos.y).OccupyingObject = _unitComponent;
        }
    }

    // This is called by the MoveCommand
    public void MoveTo(Vector2Int targetPos)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);

        List<Vector2Int> path = Pathfinding.FindPath(_mapRef.map.GetGrid(), _currentGridPos, targetPos);

        if (path != null && path.Count > 0)
        {
            _moveRoutine = StartCoroutine(FollowPath(path));
        }
    }

    private IEnumerator FollowPath(List<Vector2Int> path)
    {
        _isMoving = true;

        foreach (Vector2Int step in path)
        {
            // Dynamic Collision Check
            CellData nextCell = _mapRef.map.GetCell(step.x, step.y);
            if (nextCell.OccupyingObject != null && nextCell.OccupyingObject != _unitComponent)
            {
                break; // Blocked
            }

            // Update Grid Logic
            _mapRef.map.GetCell(_currentGridPos.x, _currentGridPos.y).OccupyingObject = null;
            nextCell.OccupyingObject = _unitComponent;
            _currentGridPos = step;

            // Visual Movement
            Vector3 start = transform.localPosition;
            Vector3 end = new Vector3(step.x, 1f, step.y);
            float t = 0;

            transform.LookAt(transform.parent.TransformPoint(end));

            while (t < 1f)
            {
                t += Time.deltaTime * stats.moveSpeed; 
                transform.localPosition = Vector3.Lerp(start, end, t);
                yield return null;
            }
            transform.localPosition = end;
        }

        _isMoving = false;
    }

    // --- ISelectable Implementation ---
    public void OnSelect()
    {
        // Visual feedback (Selection Ring) handled by WorldEntity or custom logic
        var entity = GetComponent<WorldEntity>();
        if (entity != null) entity.OnSelect();
    }

    public void OnDeselect()
    {
        var entity = GetComponent<WorldEntity>();
        if (entity != null) entity.OnDeselect();
    }
}