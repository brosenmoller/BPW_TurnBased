using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum GridTileContentType
{
    Player = 0,
    Enemy = 1,
}

public abstract class GridTileContent : MonoBehaviour 
{
    public GridTileContentType ContentType { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] protected int movementRange;

    protected CombatRoomController combatRoomController;
    protected RangeOverlayGenerator rangeOverlayGenerator;
    protected Grid grid;

    protected NavMeshAgent agent;

    protected Vector3Int[] tilesInMovementRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        grid = FindObjectOfType<Grid>();
        combatRoomController = FindObjectOfType<CombatRoomController>();
        rangeOverlayGenerator = FindObjectOfType<RangeOverlayGenerator>();

        agent.isStopped = true;
    }

    public abstract void OnTurnStart();

    protected void GenerateMovementRangeOverlay()
    {
        if (movementRange <= 1) { return; }

        Vector3Int playerGridPosition = grid.WorldToCell(transform.position);
        tilesInMovementRange = CalculateMovementRangeTiles(playerGridPosition);
        rangeOverlayGenerator.GenerateMovementRangeOverlay(tilesInMovementRange);
    }

    protected Vector3Int[] CalculateMovementRangeTiles(Vector3Int referencePosition)
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

        public Node(int x, int y, int cost)
        {
            this.x = x;
            this.y = y;
            this.cost = cost;
        }
    }
}