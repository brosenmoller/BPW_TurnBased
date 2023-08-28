using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public class CombatRoomController : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Enemies")]
    [SerializeField] private Transform enemyParent;
    [SerializeField] private GameObject enemy1Prefab;
    [SerializeField] private GameObject enemy2Prefab;
    
    private Grid grid;

    public Dictionary<Vector3Int, TileContent> gridTilesContent = new();
    
    private List<TileEnemy> enemyList;
    private List<TilePlayer> playerList;

    private readonly List<TileEntity> turnOrdering = new();
    private int currentTurnIndex;
    private Vector2Int startCoordinate = new(0, 0);

    public void RemoveTileEntity(TileEntity tile)
    {
        turnOrdering.Remove(tile);
    }

    public void Setup()
    {
        grid = FindAnyObjectByType<Grid>();
        enemyList = FindObjectsOfType<TileEnemy>().ToList();
        playerList = FindObjectsOfType<TilePlayer>().ToList();

        currentTurnIndex = 0;
        turnOrdering.AddRange(playerList);

        startCoordinate = (Vector2Int)grid.WorldToCell(playerList[0].transform.position);

        DetectRoom(startCoordinate); // Temporary Room Detection

        foreach (Vector2Int coordinate in memberCoordinates)
        {
            if (gridTilesContent.Keys.Contains((Vector3Int)coordinate)) { continue; }
            
            gridTilesContent.Add((Vector3Int)coordinate, null);
        }

        foreach (TilePlayer player in playerList)
        {
            gridTilesContent[grid.WorldToCell(player.transform.position)] = player;
        }

        int enemyCount = Random.Range(2, 5);
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3Int randomPosition;
            do
            {
                randomPosition = gridTilesContent.ElementAt(Random.Range(0, gridTilesContent.Count)).Key;
            }
            while (gridTilesContent[randomPosition] != null);

            int randomEnemy = Random.Range(0, 2);
            GameObject enemyPrefab = randomEnemy == 1 ? enemy1Prefab : enemy2Prefab;
            GameObject newEnemy = Instantiate(enemyPrefab, randomPosition, Quaternion.Euler(-90, 0, 0), enemyParent);
            enemyList.Add(newEnemy.GetComponent<TileEnemy>());
        }

        turnOrdering.AddRange(enemyList);

        Invoke(nameof(StartNextTurn), .1f);
    }

    public void CurrentTurnEnd()
    {
        currentTurnIndex++;
        if (currentTurnIndex >= turnOrdering.Count) { currentTurnIndex = 0; }

        Invoke(nameof(StartNextTurn), .001f);
    }

    private void StartNextTurn()
    {
        turnOrdering[currentTurnIndex].TurnStart();
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