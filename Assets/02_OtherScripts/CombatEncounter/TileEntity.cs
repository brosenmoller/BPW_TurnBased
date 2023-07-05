using System.Collections.Generic;
using UnityEngine;

public abstract class TileEntity : TileContent, IDamageAble
{
    [Header("Entity Settings")]
    [SerializeField] protected int movementRange;
    [SerializeField] protected Weapon weapon;
    [SerializeField] protected int maxHealth;

    private int _health;
    protected int Health { 
        private set 
        {
            _health = value;

            if (_health <= 0) { Debug.Log("Death"); }
        } 
        get { return _health; } 
    } 

    protected Dictionary<TileContentType, List<Vector3Int>> surroundingTilesMovementRange;
    protected Dictionary<TileContentType, List<Vector3Int>> surroundingTilesAttackRange;

    protected bool attackingModeAvailable;
    protected bool movingModeAvailable;
    protected bool executingMode;
    protected Mode currentMode;

    protected Vector3Int? movementTargetPosition;
    protected Vector3Int? attackTargetPosition;

    public enum Mode
    {
        Moving,
        Attacking
    }

    public override void OnTurnStart()
    {
        attackingModeAvailable = true;
        movingModeAvailable = true;
        currentMode = Mode.Moving;
        executingMode = false;
        movementTargetPosition = null;
        attackTargetPosition = null;
        Health = maxHealth;

        surroundingTilesMovementRange = CalculateSurroundingTiles(grid.WorldToCell(transform.position), movementRange);
        surroundingTilesAttackRange = CalculateSurroundingTiles(grid.WorldToCell(transform.position), weapon.attackRange);
        Debug.Log(surroundingTilesAttackRange[TileContentType.Enemy].Count);
    }

    protected virtual void OnTurnEnd() { }

    private void FixedUpdate()
    {
        if (executingMode)
        {
            if (!agent.isStopped && agent.remainingDistance <= agent.stoppingDistance)
            {
                EndExecution();
            }
        }
    }

    public void SwitchMode(Mode mode)
    {
        Debug.Log("Switching Mode To: " + mode);
        if (mode == Mode.Moving && movingModeAvailable) { currentMode = mode; OnSwitchMode(); }
        if (mode == Mode.Attacking && attackingModeAvailable) { currentMode = mode; OnSwitchMode(); }
    }

    protected virtual void OnSwitchMode() { }

    public void ExecuteMode()
    {
        if (currentMode == Mode.Moving) 
        {
            movingModeAvailable = false;
            executingMode = true;
            ExcuteMovingMode();
        }
        else if (currentMode == Mode.Attacking)
        {
            attackingModeAvailable = false;
            executingMode = true;
            ExecuteAttackMode();
        }
    }

    private void ExcuteMovingMode()
    {
        Debug.Log("Executing Movemnt");
        agent.SetDestination((Vector3)movementTargetPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0));
        agent.isStopped = false;
    }

    private void ExecuteAttackMode()
    {
        Debug.Log("Executing Attack");

        IDamageAble damageAble = (IDamageAble)combatRoomController.gridTilesContent[(Vector3Int)attackTargetPosition];
        damageAble.ApplyDamge(weapon.damage);
        EndExecution();
    }

    public void EndExecution()
    {
        executingMode = false;
        if (attackingModeAvailable) { SwitchMode(Mode.Attacking); }
        else if (movingModeAvailable) { SwitchMode(Mode.Moving); }
        else { OnTurnEnd(); TurnEnd(); }
    }

    public void ApplyDamge(int damage)
    {
        Health -= damage;
    }
}

