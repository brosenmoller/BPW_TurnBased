using System.Collections.Generic;
using System.Linq;
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

    private CombatRoomController combatRoomController;

    private NavMeshAgent agent;
    private Camera mainCamera;

    private Vector3 cursorTargetPosition;

    private Vector3Int[] tilesInMovementRange;

    private void Awake()
    {
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        combatRoomController = FindObjectOfType<CombatRoomController>();
    }

    private void Start()
    {
        agent.isStopped = true;
        GenerateMovementRangeOverlay();

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
        
        
        if (tilesInMovementRange.Contains(mouseGridPosition)) 
        {
            cursorTargetPosition = mouseGridPosition + new Vector3(grid.cellSize.x / 2f, grid.cellSize.y / 2f, 0);
        }    
    }

    private void GenerateMovementRangeOverlay()
    {
        if (movementRange <= 1) { return; }

        Vector3Int playerGridPosition = grid.WorldToCell(transform.position);
        tilesInMovementRange = CalculateMovementRangeTiles(playerGridPosition);

        foreach (Vector3Int coordinate in tilesInMovementRange)
        {
            movementRangeOverlayTilemap.SetTile(coordinate, movementRangeOverlayRuleTile);
        }
    }

    private Vector3Int[] CalculateMovementRangeTiles(Vector3Int referencePosition)
    {
        Queue<Node> nodeQueue = new();
        List<Vector3Int> validPositions = new();

        nodeQueue.Enqueue(new Node(0, 0, 0));
        validPositions.Add(referencePosition);

        while (nodeQueue.Count > 0)
        {
            Node currentNode = nodeQueue.Dequeue();

            if (currentNode.cost >= movementRange) { continue; }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (Mathf.Abs(x) == Mathf.Abs(y)) { continue; }

                    Vector3Int coordinate = new Vector3Int(currentNode.x, currentNode.y) + new Vector3Int(x, y) + referencePosition;
                    if (validPositions.Contains(coordinate)) { continue; }
                    if (!combatRoomController.memberCoordinates.Contains((Vector2Int)coordinate)) { continue; }

                    validPositions.Add(coordinate);

                    Node newNode = new(currentNode.x + x, currentNode.y + y, currentNode.cost + 1);
                    nodeQueue.Enqueue(newNode);
                }
            }
        }

        return validPositions.ToArray();
    }

    private struct Node
    {
        public int x;
        public int y;
        public int cost;

        public Node(int x, int y, int cost = int.MaxValue)
        {
            this.x = x;
            this.y = y;
            this.cost = cost;
        }
    }
}

