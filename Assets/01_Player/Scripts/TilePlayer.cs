using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TilePlayer : TileEntity
{
    [Header("Player Settings")]
    [SerializeField] protected float cursorDragSpeed;
    [SerializeField] protected Transform cursor;
    [SerializeField] protected SpriteRenderer cursorSprite;

    private Camera mainCamera;
    private Vector3 cursorTargetPosition;

    private GameUIView gameUIView;

    public override void OnAwake()
    {
        mainCamera = Camera.main;
        ContentType = TileContentType.Player;
    }

    private void Start()
    {
        gameUIView = (GameUIView)GameManager.UIViewManager.GetView(typeof(GameUIView));
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        OnSwitchMode();
        cursor.gameObject.SetActive(true);

        GameManager.InputManager.controls.Default.MouseAiming.performed += MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed += Select;

        gameUIView.OnSwitchedToMovement.RemoveAllListeners();
        gameUIView.OnSwitchedToAttack.RemoveAllListeners();
        gameUIView.OnSwitchedToMovement.AddListener(() => { SwitchMode(Mode.Moving); });
        gameUIView.OnSwitchedToAttack.AddListener(() => { SwitchMode(Mode.Attacking); });
    }

    protected override void OnTurnEnd()
    {
        cursor.gameObject.SetActive(false);

        GameManager.InputManager.controls.Default.MouseAiming.performed -= MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed -= Select;

        gameUIView.OnSwitchedToMovement.RemoveAllListeners();
        gameUIView.OnSwitchedToAttack.RemoveAllListeners();
    }

    protected void GenerateMovementRangeOverlay()
    {
        if (surroundingTilesMovementRange == null) { return; }

        rangeOverlayGenerator.GenerateMovementRangeOverlay(
            surroundingTilesMovementRange[TileContentType.Empty].ToArray(), 
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
        if (currentMode == Mode.Moving)
        {    
            rangeOverlayGenerator.ClearRangeOverlay();
            gameUIView.HighligtMovementMode();
            GenerateMovementRangeOverlay();
        }
        else if (currentMode == Mode.Attacking)
        {
            gameUIView.HighligtAttackMode();
            rangeOverlayGenerator.ClearRangeOverlay();
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
        if (currentMode == Mode.Attacking) { AttackCursor(mouseGridPosition); }
    }

    private void MovementCursor(Vector3Int newGridPosition)
    {
        if (!surroundingTilesMovementRange[TileContentType.Empty].Contains(newGridPosition)) { return;}

        cursorTargetPosition = newGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        movementTargetPosition = newGridPosition;

        TileContent currentTarget = combatRoomController.gridTilesContent[newGridPosition];

        if (currentTarget == null || currentTarget.ContentType == TileContentType.Empty) { cursorSprite.color = Color.blue; }
        else if (currentTarget.ContentType == TileContentType.Enemy) { cursorSprite.color = Color.red; }
    }
    
    private void AttackCursor(Vector3Int newGridPosition)
    {
        if (!surroundingTilesAttackRange[TileContentType.Enemy].Contains(newGridPosition)) { return; }

        cursorTargetPosition = newGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        attackTargetPosition = newGridPosition;
    }



    private void FixedUpdate()
    {
        if (cursor.position != cursorTargetPosition)
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }
    }
}

