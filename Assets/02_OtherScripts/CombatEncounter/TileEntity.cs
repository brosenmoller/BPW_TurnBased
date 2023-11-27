using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class TileEntity : TileContent, IDamageAble
{
    [Header("Entity Settings")]
    [SerializeField] protected int movementRange;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected Material whiteFlashMaterial;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Slider healthBar;
    public Weapon selectedWeapon;

    private int _health;
    protected int Health { 
        private set 
        {
            _health = value;

            if (_health <= 0) { OnDeath(); }
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

    private Material normalMaterial;
    protected NavMeshAgent agent;
    protected RangeOverlayGenerator rangeOverlayGenerator;

    public enum Mode
    {
        Moving,
        Attacking
    }

    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        rangeOverlayGenerator = FindObjectOfType<RangeOverlayGenerator>();

        normalMaterial = spriteRenderer.material;
        Health = maxHealth;
    }

    public void TurnStart()
    {
        attackingModeAvailable = true;
        movingModeAvailable = true;
        currentMode = Mode.Moving;
        executingMode = false;
        movementTargetPosition = null;
        attackTargetPosition = null;
        agent.enabled = true;

        CalculateMovementTiles();
        CalculateAttackTiles();
        OnTurnStart();
    }

    protected void TurnEnd()
    {
        UpdateCombatRoom();
        combatRoomController.CurrentTurnEnd();
    }

    protected void CalculateMovementTiles()
    {
        surroundingTilesMovementRange = CalculateSurroundingTiles(grid.WorldToCell(transform.position), movementRange);
    }

    protected void CalculateAttackTiles()
    {
        surroundingTilesAttackRange = CalculateSurroundingTiles(grid.WorldToCell(transform.position), selectedWeapon.attackRange, true);
    }

    protected virtual void FixedUpdate()
    {
        if (executingMode)
        {
            if (currentMode == Mode.Moving && !agent.isStopped && agent.remainingDistance <= agent.stoppingDistance)
            {
                EndExecution();
            }
        }
        OnFixedUpdate();
    }

    protected virtual void OnTurnStart() { }
    protected virtual void OnTurnEnd() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDeath() { }

    public void SwitchMode(Mode mode)
    {
        if (mode == Mode.Moving && movingModeAvailable) { currentMode = mode; OnSwitchMode(); }
        else if (mode == Mode.Attacking && attackingModeAvailable) { currentMode = mode; OnSwitchMode(); }
        else if (!movingModeAvailable && !attackingModeAvailable)
        {
            OnTurnEnd();
            TurnEnd();
        }
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
        agent.SetDestination((Vector3)movementTargetPosition + new Vector3(.5f, .5f, 0));
        agent.isStopped = false;
    }

    private void ExecuteAttackMode()
    {
        IDamageAble damageAble = (IDamageAble)combatRoomController.gridTilesContent[(Vector3Int)attackTargetPosition];
        selectedWeapon.StartUseWeapon(transform.position, (Vector3)attackTargetPosition + new Vector3(.5f, .5f, 0), damageAble, EndExecution);
    }

    public void EndExecution()
    {
        executingMode = false;
        agent.isStopped = true;
        UpdateCombatRoom();
        if (attackingModeAvailable) 
        { 
            CalculateAttackTiles();
            SwitchMode(Mode.Attacking);
        }
        else if (movingModeAvailable) 
        {
            CalculateMovementTiles();
            SwitchMode(Mode.Moving); 
        }
        else
        {
            OnTurnEnd();
            TurnEnd();
        }
    }

    public void ApplyDamge(int damage)
    {
        Health -= damage;
        healthBar.value = Mathf.InverseLerp(0, maxHealth, Health);
    }

    public void GetHitEffects()
    {
        spriteRenderer.material = whiteFlashMaterial;

        Invoke(nameof(EndEffects), .2f);
    }

    private void EndEffects()
    {
        spriteRenderer.material = normalMaterial;
    }
}

