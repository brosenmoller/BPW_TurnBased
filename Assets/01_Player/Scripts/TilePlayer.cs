using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TilePlayer : TileEntity
{
    [Header("Player Settings")]
    [SerializeField] protected float cursorDragSpeed;
    [SerializeField] protected Transform cursor;
    [SerializeField] protected int maxWeapons = 4;

    private Camera mainCamera;
    private Vector3 cursorTargetPosition;
    private SpriteRenderer cursorSpriteRenderer;

    private GameUIView gameUIView;
    private List<Weapon> unlockedWeapons;

    public override void OnAwake()
    {
        base.OnAwake();
        mainCamera = Camera.main;
        ContentType = TileContentType.Player;
        cursorSpriteRenderer = cursor.GetComponent<SpriteRenderer>();

        unlockedWeapons = new List<Weapon>() { selectedWeapon };
    }

    private void Start()
    {
        gameUIView = (GameUIView)GameManager.UIViewManager.GetView(typeof(GameUIView));
        gameUIView.UpdateWeaponDisplay(unlockedWeapons.ToArray(), selectedWeapon, maxWeapons);
    }

    protected override void OnTurnStart()
    {
        OnSwitchMode();
        cursor.gameObject.SetActive(true);

        GameManager.InputManager.controls.Default.MouseAiming.performed += MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed += Select;
        GameManager.InputManager.controls.Default.CycleWeapon.performed += CycleWeapon;

        gameUIView.OnSwitchedToMovement.RemoveAllListeners();
        gameUIView.OnSwitchedToAttack.RemoveAllListeners();
        gameUIView.OnTurnEnd.RemoveAllListeners();
        gameUIView.OnSwitchedToMovement.AddListener(() => { SwitchMode(Mode.Moving); });
        gameUIView.OnSwitchedToAttack.AddListener(() => { SwitchMode(Mode.Attacking); });
        gameUIView.OnTurnEnd.AddListener(StartEndTurn);
    }

    private void StartEndTurn()
    {
        if (!inTurn) { return; }

        rangeOverlayGenerator.ClearRangeOverlay();
        OnTurnEnd();
        TurnEnd();
    }

    private void CycleWeapon(InputAction.CallbackContext callbackContext)
    {
        int nextIndex = unlockedWeapons.FindIndex(a => a == selectedWeapon);
        nextIndex++;
        if (nextIndex >= unlockedWeapons.Count) { nextIndex = 0; }

        selectedWeapon = unlockedWeapons[nextIndex];
        
        CalculateAttackTiles();

        if (currentMode == Mode.Attacking) 
        {
            rangeOverlayGenerator.ClearRangeOverlay();
            GenerateAttackRangeOverlay();
            

            if (surroundingTilesAttackRange[TileContentType.Enemy].Count <= 0)
            {
                SwitchMode(Mode.Moving);
                attackTargetPosition = null;
            }
            else
            {
                attackTargetPosition = surroundingTilesAttackRange[TileContentType.Enemy][Random.Range(0, surroundingTilesAttackRange[TileContentType.Enemy].Count - 1)];
                cursorTargetPosition = (Vector3)attackTargetPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
            }
        }

        gameUIView.UpdateWeaponDisplay(unlockedWeapons.ToArray(), selectedWeapon, maxWeapons);

    }

    public override void SetWeapon(Weapon weapon)
    {
        base.SetWeapon(weapon);
        if (!unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
        }
        gameUIView.UpdateWeaponDisplay(unlockedWeapons.ToArray(), selectedWeapon, maxWeapons);
    }

    protected override void OnTurnEnd()
    {
        cursor.gameObject.SetActive(false);

        GameManager.InputManager.controls.Default.MouseAiming.performed -= MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed -= Select;
        GameManager.InputManager.controls.Default.CycleWeapon.performed -= CycleWeapon;

        gameUIView.OnSwitchedToMovement.RemoveAllListeners();
        gameUIView.OnSwitchedToAttack.RemoveAllListeners();
    }

    protected void GenerateMovementRangeOverlay()
    {
        if (surroundingTilesMovementRange == null) { return; }

        rangeOverlayGenerator.GenerateMovementRangeOverlay(
            surroundingTilesMovementRange[TileContentType.Empty]
            .Concat(surroundingTilesMovementRange[TileContentType.WeaponPickup]).ToArray(), 
            surroundingTilesMovementRange[TileContentType.Enemy].ToArray()
        );
    }

    protected void GenerateAttackRangeOverlay()
    {
        if (surroundingTilesAttackRange == null) { return; }

        rangeOverlayGenerator.GenerateAttackRangeOverlay(
            surroundingTilesAttackRange[TileContentType.Empty]
            .Concat(surroundingTilesAttackRange[TileContentType.Enemy]).ToArray()
        );
    }

    private void Select(InputAction.CallbackContext callbackContext)
    {
        if (executingMode) { return; }

        cursor.gameObject.SetActive(false);
        rangeOverlayGenerator.ClearRangeOverlay();

        if (currentMode == Mode.Moving && movementTargetPosition == null) { return; }
        if (currentMode == Mode.Attacking && attackTargetPosition == null) { return; }

        ExecuteMode();
    }

    protected override void OnSwitchMode()
    {
        cursor.gameObject.SetActive(true);

        if (currentMode == Mode.Moving)
        {
            cursorSpriteRenderer.color = Color.blue;

            if (surroundingTilesMovementRange[TileContentType.Empty].Count <= 0)
            {
                movingModeAvailable = false;
                SwitchMode(Mode.Attacking);
                return;
            }

            movementTargetPosition = surroundingTilesMovementRange[TileContentType.Empty][Random.Range(0, surroundingTilesMovementRange[TileContentType.Empty].Count - 1)];
            cursorTargetPosition = (Vector3)movementTargetPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);

            rangeOverlayGenerator.ClearRangeOverlay();
            gameUIView.HighligtMovementMode();
            GenerateMovementRangeOverlay();
        }
        else if (currentMode == Mode.Attacking)
        {
            cursorSpriteRenderer.color = Color.red;

            if (surroundingTilesAttackRange[TileContentType.Enemy].Count > 0)
            {
                attackTargetPosition = surroundingTilesAttackRange[TileContentType.Enemy][Random.Range(0, surroundingTilesAttackRange[TileContentType.Enemy].Count - 1)];
                cursorTargetPosition = (Vector3)attackTargetPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
            }
            else
            {
                cursor.gameObject.SetActive(false);
            }

            rangeOverlayGenerator.ClearRangeOverlay();
            gameUIView.HighligtAttackMode();
            GenerateAttackRangeOverlay();
        }
    }

    private void MoveCursor(InputAction.CallbackContext callbackContext)
    {
        if (executingMode) { return; }

        Vector2 mouseScreenPosition = callbackContext.ReadValue<Vector2>();
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector3Int mouseGridPosition = grid.WorldToCell(mousePosition);

        if (currentMode == Mode.Moving) { MovementCursor(mouseGridPosition); }
        else if (currentMode == Mode.Attacking) { AttackCursor(mouseGridPosition); }
    }

    private void MovementCursor(Vector3Int newGridPosition)
    {
        if (!surroundingTilesMovementRange[TileContentType.Empty].Contains(newGridPosition) &&
            !surroundingTilesMovementRange[TileContentType.WeaponPickup].Contains(newGridPosition)) { return; }

        cursorTargetPosition = newGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        movementTargetPosition = newGridPosition;

        TileContent currentTarget = combatRoomController.gridTilesContent[newGridPosition];

        if (currentTarget == null || currentTarget.ContentType == TileContentType.Empty) { cursorSpriteRenderer.color = Color.blue; }
        else if (currentTarget.ContentType == TileContentType.Enemy) { cursorSpriteRenderer.color = Color.red; }
    }
    
    private void AttackCursor(Vector3Int newGridPosition)
    {
        if (!surroundingTilesAttackRange[TileContentType.Enemy].Contains(newGridPosition)) { return; }

        cursorTargetPosition = newGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        attackTargetPosition = newGridPosition;
    }

    protected override void OnFixedUpdate()
    {
        if (cursor.position != cursorTargetPosition)
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }
    }

    private void OnDestroy()
    {
        GameManager.InputManager.controls.Default.MouseAiming.performed -= MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed -= Select;
    }

    protected override void OnDeath()
    {
        OnTurnEnd();
        GameManager.Instance.ReloadCurrentScene();
    }
}
