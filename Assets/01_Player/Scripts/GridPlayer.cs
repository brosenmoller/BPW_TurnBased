using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlayer : GridTileContent, IDamageAble
{
    [Header("Other")]
    [SerializeField] protected float cursorDragSpeed;
    [SerializeField] protected Transform cursor;
    [SerializeField] protected SpriteRenderer cursorSprite;

    private Camera mainCamera;
    private Vector3 cursorTargetPosition;
    private GridTileContent currentTarget;

    public override void OnAwake()
    {
        mainCamera = Camera.main;
        ContentType = GridTileContentType.Player;
    }

    public override void OnTurnStart()
    {
        GenerateMovementRangeOverlay();

        GameManager.InputManager.controls.Default.MouseAiming.performed += MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed += SelectTile;
    }

    private void OnTurnEnd()
    {
        GameManager.InputManager.controls.Default.MouseAiming.performed -= MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed -= SelectTile;

        TurnEnd();
    }

    protected void GenerateMovementRangeOverlay()
    {
        if (movementRange <= 1) { return; }

        Vector3Int playerGridPosition = grid.WorldToCell(transform.position);
        CalculateSurroundingTiles(playerGridPosition);
        rangeOverlayGenerator.GenerateMovementRangeOverlay(surroundingTiles[GridTileContentType.Empty].ToArray(), surroundingTiles[GridTileContentType.Enemy].ToArray());
    }

    private void SelectTile(InputAction.CallbackContext callbackContext)
    {
        if (currentTarget == null || currentTarget.ContentType == GridTileContentType.Empty)
        {
            agent.SetDestination(cursorTargetPosition);
            agent.isStopped = false;
            rangeOverlayGenerator.ClearRangeOverlay();
        }
        
    }

    private void MoveCursor(InputAction.CallbackContext callbackContext)
    {
        Vector2 mouseScreenPosition = callbackContext.ReadValue<Vector2>();
        if (!agent.isStopped) { return; }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector3Int mouseGridPosition = grid.WorldToCell(mousePosition);

        if (!surroundingTiles[GridTileContentType.Empty].Contains(mouseGridPosition)) { return; }

        cursorTargetPosition = mouseGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        currentTarget = combatRoomController.gridTilesContent[mouseGridPosition];

        if (currentTarget == null || currentTarget.ContentType == GridTileContentType.Empty) { cursorSprite.color = Color.blue; }
        else if (currentTarget.ContentType == GridTileContentType.Enemy) { cursorSprite.color = Color.red; }
    }

    private void FixedUpdate()
    {
        if (cursor.position != cursorTargetPosition)
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }

        if (agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
        {
            OnTurnEnd();
        }
    }

    public void Damage(int damage)
    {
        
    }
}

