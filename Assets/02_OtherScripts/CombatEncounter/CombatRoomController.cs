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

    [Header("Weapons")]
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private Weapon[] possibleWeapons;
    
    private Grid grid;
    private DungeonManager dungeonManager;

    public Dictionary<Vector3Int, TileContent> gridTilesContent;
    
    private List<TileEnemy> enemyList;
    private List<TilePlayer> playerList;

    private readonly List<TileEntity> turnOrdering = new();
    private int currentTurnIndex;
    private Vector2Int startCoordinate = new(0, 0);

    public void RemoveTileContent(TileContent tile)
    {
        Vector3Int key = gridTilesContent.FirstOrDefault(keyValuePair => keyValuePair.Value == tile).Key;
        gridTilesContent[key] = null;
        Destroy(tile.gameObject);
    }

    public void RemoveTileEntity(TileEntity tile)
    {
        turnOrdering.Remove(tile);
        RemoveTileContent(tile);
    }

    public void RemoveEnemy(TileEnemy tile)
    {
        if (enemyList.Contains(tile)) { enemyList.Remove(tile);}
        RemoveTileEntity(tile);
    }

    public void Setup(DungeonManager dungeonManager)
    {
        this.dungeonManager = dungeonManager;
        grid = FindAnyObjectByType<Grid>();
        enemyList = new List<TileEnemy>();
        playerList = FindObjectsOfType<TilePlayer>().ToList();
        turnOrdering.Clear();

        currentTurnIndex = 0;
        turnOrdering.AddRange(playerList);

        startCoordinate = (Vector2Int)grid.WorldToCell(playerList[0].transform.position);

        gridTilesContent = new Dictionary<Vector3Int, TileContent>();
        DetectRoom(startCoordinate); 

        foreach (Vector2Int coordinate in memberCoordinates)
        {
            if (gridTilesContent.Keys.Contains((Vector3Int)coordinate)) { continue; }
            
            gridTilesContent.Add((Vector3Int)coordinate, null);
        }

        foreach (TilePlayer player in playerList)
        {
            gridTilesContent[grid.WorldToCell(player.transform.position)] = player;
        }

        SpawnEnemies();
        SpawnWeaponPickups();

        turnOrdering.AddRange(enemyList);

        Invoke(nameof(StartNextTurn), .1f);
    }

    public void CurrentTurnEnd()
    {
        currentTurnIndex++;
        if (currentTurnIndex >= turnOrdering.Count) { currentTurnIndex = 0; }

        if (enemyList.Count <= 0)
        {
            dungeonManager.FinishStage();
        }
        else
        {
            Invoke(nameof(StartNextTurn), .001f);
        }
    }

    private void StartNextTurn()
    {
        turnOrdering[currentTurnIndex].TurnStart();
    }

    private void SpawnEnemies()
    {
        int enemyCount = Random.Range(1, 2);
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
            GameObject newEnemy = Instantiate(enemyPrefab, randomPosition + new Vector3(.5f, .5f), Quaternion.Euler(-90, 0, 0), enemyParent);
            TileEnemy enemyTile = newEnemy.GetComponent<TileEnemy>();
            enemyList.Add(enemyTile);
            gridTilesContent[randomPosition] = enemyTile;
        }
    }

    private void SpawnWeaponPickups()
    {
        int weaonCount = Random.Range(0, 2);
        for (int i = 0; i < weaonCount; i++)
        {
            Vector3Int randomPosition;
            do
            {
                randomPosition = gridTilesContent.ElementAt(Random.Range(0, gridTilesContent.Count)).Key;
            }
            while (gridTilesContent[randomPosition] != null);

            GameObject newWeapon = Instantiate(weaponPickupPrefab, randomPosition + new Vector3(.5f, .5f), Quaternion.Euler(0, 0, -90));
            TileWeaponPickup weaponTile = newWeapon.GetComponent<TileWeaponPickup>();
            
            Weapon randomWeapon = possibleWeapons[Random.Range(0, possibleWeapons.Length)];
            weaponTile.SetWeapon(randomWeapon);

            gridTilesContent[randomPosition] = weaponTile;
        }
    }

    private void OnDrawGizmos()
    {
        if (gridTilesContent == null) { return; }

        foreach (Vector3Int key in gridTilesContent.Keys)
        {
            if (gridTilesContent[key] == null || gridTilesContent[key].ContentType == TileContentType.Empty) { Gizmos.color = Color.blue; }
            else if (gridTilesContent[key].ContentType == TileContentType.Player) { Gizmos.color = Color.green; }
            else if (gridTilesContent[key].ContentType == TileContentType.Enemy) { Gizmos.color = Color.red; }
            else if (gridTilesContent[key].ContentType == TileContentType.WeaponPickup) { Gizmos.color = Color.yellow; }

            Gizmos.DrawWireCube(key + new Vector3(.5f, .5f), new Vector3(.8f, .8f, 0));
        }
    }

    #region Room detection
    public HashSet<Vector2Int> memberCoordinates;
    private Queue<Vector2Int> coordinatesToCheck;
    private void DetectRoom(Vector2Int startCoordinate)
    {
        memberCoordinates = new HashSet<Vector2Int>();
        coordinatesToCheck = new Queue<Vector2Int>();

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