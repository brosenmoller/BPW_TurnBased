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
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase floorRuleTile;

    private Controls controls;
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
    }

    private void OnEnable()
    {
        controls = new Controls();
        controls.Enable();

        controls.Default.MouseAiming.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>());
        controls.Default.SelectLocation.performed += _ => GoToCursor();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void FixedUpdate()
    {
        if (cursor.position != cursorTargetPosition) 
        {
            cursor.position = Vector3.Lerp(cursor.position, cursorTargetPosition, Time.deltaTime * cursorDragSpeed);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
        }
    }

    private void GoToCursor()
    {
        agent.SetDestination(cursorTargetPosition);
        agent.isStopped = false;
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
            //mouseGridPosition = FindClosestCellWithinRange(mouseGridPosition, playerGridPosition, .1f);
        }

        cursorTargetPosition = mouseGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
    }

    private Vector3Int FindClosestCellWithinRange(Vector3Int startingCell, Vector3Int originCell, float maxDistanceDelta)
    {
        Vector3 directionToOrigin = ((Vector3)originCell - (Vector3)startingCell).normalized;
        float currentDistance = 0;

        while (Vector3Int.Distance(startingCell, originCell) > movementRange)
        {
            currentDistance += maxDistanceDelta;
            startingCell = grid.WorldToCell(startingCell + directionToOrigin * currentDistance);
        }

        return startingCell;
    }
}

