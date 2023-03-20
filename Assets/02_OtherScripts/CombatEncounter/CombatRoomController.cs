using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public class CombatRoomController : MonoBehaviour
{
    [SerializeField] private Vector2Int startCoordinate;
    [SerializeField] private Tilemap groundTilemap;

    public Dictionary<Vector3Int, GridTileContent> gridTilesContent = new();
    
    private List<GridEnemy> enemyList;
    private GridPlayer player;

    private List<GridTileContent> turnOrdering = new();
    private int currentTurnIndex;

    private void Awake()
    {
        enemyList = FindObjectsOfType<GridEnemy>().ToList();
        player = FindObjectOfType<GridPlayer>();

        currentTurnIndex = 0;
        turnOrdering.Add(player);
        turnOrdering.AddRange(enemyList);

        DetectRoom(startCoordinate); // Temporary Room Detection
    }

    private void Start()
    {
        turnOrdering[currentTurnIndex].OnTurnStart();
    }

    public void CurrentTurnEnd()
    {
        currentTurnIndex++;
        if (currentTurnIndex >= turnOrdering.Count) { currentTurnIndex = 0; }

        turnOrdering[currentTurnIndex].OnTurnStart();
    }


    #region Temp Room detection
    public readonly HashSet<Vector2Int> memberCoordinates = new();
    private readonly Queue<Vector2Int> coordinatesToCheck = new();
    private void DetectRoom(Vector2Int startCoordinate)
    {
        coordinatesToCheck.Enqueue(startCoordinate);
        memberCoordinates.Add(startCoordinate);

        while (coordinatesToCheck.Count > 0)
        {
            Vector2Int currentCoordinate = coordinatesToCheck.Dequeue();

            CheckCoordinate(new Vector2Int(currentCoordinate.x + 1, currentCoordinate.y));
            CheckCoordinate(new Vector2Int(currentCoordinate.x - 1, currentCoordinate.y));
            CheckCoordinate(new Vector2Int(currentCoordinate.x, currentCoordinate.y + 1));
            CheckCoordinate(new Vector2Int(currentCoordinate.x, currentCoordinate.y - 1));
        }
    }

    private void CheckCoordinate(Vector2Int coordinate)
    {
        if (memberCoordinates.Contains(coordinate)) { return; }

        if (groundTilemap.GetTile((Vector3Int)coordinate) != null)
        {
            memberCoordinates.Add(coordinate);
            coordinatesToCheck.Enqueue(coordinate);
        }
    }
    #endregion
}