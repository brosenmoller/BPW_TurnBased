using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public class CombatRoomController : MonoBehaviour
{
    [SerializeField] private Vector2Int startCoordinate;
    [SerializeField] private Tilemap groundTilemap;
    
    private Grid grid;

    public Dictionary<Vector3Int, TileContent> gridTilesContent = new();
    
    private List<TileEnemy> enemyList;
    private TilePlayer player;

    private readonly List<TileContent> turnOrdering = new();
    private int currentTurnIndex;

    private void Awake()
    {
        grid = FindAnyObjectByType<Grid>();
        enemyList = FindObjectsOfType<TileEnemy>().ToList();
        player = FindObjectOfType<TilePlayer>();

        currentTurnIndex = 0;
        turnOrdering.Add(player);
        turnOrdering.AddRange(enemyList);

        DetectRoom(startCoordinate); // Temporary Room Detection

        foreach (Vector2Int coordinate in memberCoordinates)
        {
            gridTilesContent.Add((Vector3Int)coordinate, null);
        }

        foreach (TileEnemy enemy in enemyList)
        {
            gridTilesContent[grid.WorldToCell(enemy.transform.position)] = enemy;
        }

        gridTilesContent[grid.WorldToCell(player.transform.position)] = player;
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

    private void OnDrawGizmos()
    {
        foreach (Vector3Int key in gridTilesContent.Keys)
        {
            if (gridTilesContent[key] == null) { Gizmos.color = Color.blue; }
            else if (gridTilesContent[key].ContentType == TileContentType.Player) { Gizmos.color = Color.green; }
            else if (gridTilesContent[key].ContentType == TileContentType.Enemy) { Gizmos.color = Color.red; }

            Gizmos.DrawWireCube(key + new Vector3(.5f, .5f), new Vector3(.8f, .8f, 0));
        }
    }


    #region Temporary Room detection
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