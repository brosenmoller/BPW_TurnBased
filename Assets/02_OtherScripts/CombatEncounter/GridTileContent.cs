using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum GridTileContentType
{
    Empty = 0,
    Player = 1,
    Enemy = 2,
}

public abstract class GridTileContent : MonoBehaviour 
{
    public GridTileContentType ContentType { get; protected set; }

    [Header("Grid Settings")]
    [SerializeField] protected int movementRange;

    protected CombatRoomController combatRoomController;
    protected RangeOverlayGenerator rangeOverlayGenerator;
    protected Grid grid;

    protected NavMeshAgent agent;

    protected Dictionary<GridTileContentType, List<Vector3Int>> surroundingTiles = new();

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        grid = FindObjectOfType<Grid>();
        combatRoomController = FindObjectOfType<CombatRoomController>();
        rangeOverlayGenerator = FindObjectOfType<RangeOverlayGenerator>();
        ContentType = GridTileContentType.Empty;

        agent.isStopped = true;

        OnAwake();
    }

    public abstract void OnAwake();

    public abstract void OnTurnStart();

    protected void CalculateSurroundingTiles(Vector3Int referencePosition)
    {
        Queue<Node> nodeQueue = new();
        List<Vector3Int> validPositions = new();

        surroundingTiles = new()
        {
            { GridTileContentType.Empty, new List<Vector3Int>() },
            { GridTileContentType.Player, new List<Vector3Int>() },
            { GridTileContentType.Enemy, new List<Vector3Int>() }
        };

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

                    if (!combatRoomController.gridTilesContent.ContainsKey(coordinate)) { continue; }

                    if (combatRoomController.gridTilesContent[coordinate] == null ||
                        combatRoomController.gridTilesContent[coordinate].ContentType == GridTileContentType.Empty)
                    {
                        surroundingTiles[GridTileContentType.Empty].Add(coordinate);
                    }
                    else if (combatRoomController.gridTilesContent[coordinate].ContentType == GridTileContentType.Player) 
                    {
                        surroundingTiles[GridTileContentType.Player].Add(coordinate); 
                    }
                    else if (combatRoomController.gridTilesContent[coordinate].ContentType == GridTileContentType.Enemy)
                    {
                        surroundingTiles[GridTileContentType.Enemy].Add(coordinate);
                    }

                    validPositions.Add(coordinate);

                    Node newNode = new(currentNode.x + x, currentNode.y + y, currentNode.cost + 1);
                    nodeQueue.Enqueue(newNode);
                }
            }
        }
    }

    protected void TurnEnd()
    {
        agent.isStopped = true;
        Vector3Int key = combatRoomController.gridTilesContent.FirstOrDefault(keyValuePair => keyValuePair.Value == this).Key;
        combatRoomController.gridTilesContent[key] = null;
        combatRoomController.gridTilesContent[grid.WorldToCell(transform.position)] = this;
        combatRoomController.CurrentTurnEnd();
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