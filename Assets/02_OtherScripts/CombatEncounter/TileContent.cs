using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum TileContentType
{
    Empty = 0,
    Player = 1,
    Enemy = 2,
}

public abstract class TileContent : MonoBehaviour 
{
    public TileContentType ContentType { get; protected set; }

    protected CombatRoomController combatRoomController;
    protected RangeOverlayGenerator rangeOverlayGenerator;
    protected Grid grid;
    protected NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        grid = FindObjectOfType<Grid>();
        combatRoomController = FindObjectOfType<CombatRoomController>();
        rangeOverlayGenerator = FindObjectOfType<RangeOverlayGenerator>();
        ContentType = TileContentType.Empty;

        agent.isStopped = true;

        OnAwake();
    }

    public abstract void OnAwake();

    protected Dictionary<TileContentType, List<Vector3Int>> CalculateSurroundingTiles(Vector3Int referencePosition, int range, bool sightLineRequired = false)
    {
        Queue<Node> nodeQueue = new();
        List<Vector3Int> validPositions = new();

        Dictionary<TileContentType, List<Vector3Int>> surroundingTiles = new()
        {
            { TileContentType.Empty, new List<Vector3Int>() },
            { TileContentType.Player, new List<Vector3Int>() },
            { TileContentType.Enemy, new List<Vector3Int>() }
        };

        nodeQueue.Enqueue(new Node(0, 0, 0));
        validPositions.Add(referencePosition);

        while (nodeQueue.Count > 0)
        {
            Node currentNode = nodeQueue.Dequeue();

            if (currentNode.cost >= range) { continue; }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (Mathf.Abs(x) == Mathf.Abs(y)) { continue; } // Exclude Diagonals

                    Vector3Int coordinate = new Vector3Int(currentNode.x, currentNode.y) + new Vector3Int(x, y) + referencePosition;
                    if (validPositions.Contains(coordinate)) { continue; }

                    if (!combatRoomController.gridTilesContent.ContainsKey(coordinate)) { continue; }

                    if (sightLineRequired)
                    {
                        Vector3 origin = transform.position;
                        Vector3 target = coordinate + new Vector3(.5f, .5f, 0);

                        RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, target - origin, Vector2.Distance(target, origin));
                        if (raycastHit2D.collider != null) { continue; }
                    }

                    if (combatRoomController.gridTilesContent[coordinate] == null ||
                        combatRoomController.gridTilesContent[coordinate].ContentType == TileContentType.Empty)
                    {
                        surroundingTiles[TileContentType.Empty].Add(coordinate);
                    }
                    else if (combatRoomController.gridTilesContent[coordinate].ContentType == TileContentType.Player) 
                    {
                        surroundingTiles[TileContentType.Player].Add(coordinate); 
                    }
                    else if (combatRoomController.gridTilesContent[coordinate].ContentType == TileContentType.Enemy)
                    {
                        surroundingTiles[TileContentType.Enemy].Add(coordinate);
                    }

                    validPositions.Add(coordinate);

                    Node newNode = new(currentNode.x + x, currentNode.y + y, currentNode.cost + 1);
                    nodeQueue.Enqueue(newNode);
                }
            }
        }

        return surroundingTiles;
    }

    protected void UpdateCombatRoom()
    {
        Vector3Int key = combatRoomController.gridTilesContent.FirstOrDefault(keyValuePair => keyValuePair.Value == this).Key;
        combatRoomController.gridTilesContent[key] = null;
        combatRoomController.gridTilesContent[grid.WorldToCell(transform.position + new Vector3(.2f, .2f, 0))] = this;
    }

    protected void TurnEnd()
    {
        UpdateCombatRoom();
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