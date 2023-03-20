using UnityEngine;
using System.Linq;
using static UnityEngine.InputSystem.InputAction;

public class GridPlayer : GridTileContent
{
    [Header("Other")]
    [SerializeField] protected float cursorDragSpeed;
    [SerializeField] protected Transform cursor;

    private Camera mainCamera;

    private Vector3 cursorTargetPosition;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public override void OnTurnStart()
    {
        GenerateMovementRangeOverlay();

        GameManager.InputManager.controls.Default.MouseAiming.performed += MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed += GoToCursor;
    }

    private void OnTurnEnd()
    {
        GameManager.InputManager.controls.Default.MouseAiming.performed -= MoveCursor;
        GameManager.InputManager.controls.Default.SelectLocation.performed -= GoToCursor;

        combatRoomController.CurrentTurnEnd();
    }

    private void GoToCursor(CallbackContext callbackContext)
    {
        agent.SetDestination(cursorTargetPosition);
        agent.isStopped = false;
        rangeOverlayGenerator.ClearMovementRangeOverlay();
    }

    private void MoveCursor(CallbackContext callbackContext)
    {
        Vector2 mouseScreenPosition = callbackContext.ReadValue<Vector2>();
        if (!agent.isStopped) { return; }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector3Int mouseGridPosition = grid.WorldToCell(mousePosition);


        if (tilesInMovementRange.Contains(mouseGridPosition))
        {
            cursorTargetPosition = mouseGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        }
    }

    private void FixedUpdate()
    {
        if (cursor.position != cursorTargetPosition)
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }

        if (agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
        {
            agent.isStopped = true;
            OnTurnEnd();
        }
    }
}

