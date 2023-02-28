using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class GridBasedMovement : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int movementRange;

    [Header("Other")]
    [SerializeField] private float cursorDragSpeed;

    [Header("References")]
    [SerializeField] private Transform cursor;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap movementRangeOverlayTilemap;
    [SerializeField] private TileBase movementRangeOverlayRuleTile;

    private NavMeshAgent agent;
    private Camera mainCamera;

    private Vector3 cursorTargetPosition;

    private void Awake()
    {
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.isStopped = true;

        GameManager.InputManager.controls.Default.MouseAiming.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>());
        GameManager.InputManager.controls.Default.SelectLocation.performed += _ => GoToCursor();
    }

    private void FixedUpdate()
    {
        if (cursor.position != cursorTargetPosition) 
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }

        if (agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
        {
            GenerateMovementRangeOverlay();
            agent.isStopped = true;
        }
    }

    private void GoToCursor()
    {
        agent.SetDestination(cursorTargetPosition);
        agent.isStopped = false;
        movementRangeOverlayTilemap.ClearAllTiles();
    }

    private void MoveCursor(Vector2 mouseScreenPosition)
    {
        if (!agent.isStopped) { return; }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector3Int mouseGridPosition = grid.WorldToCell(mousePosition);
        Vector3Int playerGridPosition = grid.WorldToCell(transform.position);
        
        if (Vector3Int.Distance(mouseGridPosition, playerGridPosition) > movementRange) 
        {
            return;
        }

        cursorTargetPosition = mouseGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
    }

    private void GenerateMovementRangeOverlay()
    {
        if (movementRange <= 1) { return; }

        Vector3Int playerGridPosition = grid.WorldToCell(transform.position);

        for (int x = -movementRange; x < movementRange; x++)
        {
            for (int y = -movementRange; y < movementRange; y++)
            {
                Vector3Int gridPosition = new Vector3Int(x, y) + playerGridPosition;
                Debug.Log(gridPosition);
                if (Vector3Int.Distance(gridPosition, playerGridPosition) > movementRange) { continue; }
                
                movementRangeOverlayTilemap.SetTile(gridPosition, movementRangeOverlayRuleTile);
            }
        }
    }
}

