using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] private int mapSize;
    [SerializeField] private int borderSize;
    [SerializeField] private int seed;
    private int borderedMapSize;

    [Header("Cellular Automata")]
    [SerializeField, Range(0, 100)] private int randomFillPercent;
    [SerializeField, Range(0, 100)] private int smoothInterations;
    
    [SerializeField, Range(0, 9)] private int wallCuttoff;

    [Header("Finalization")]
    [SerializeField, Range(0, 20)] private int cleanupIterations;
    [SerializeField, Range(0, 20)] private float minSizeRoomPercentage;
    [SerializeField, Range(1, 10)] private int maxCorridorSize;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Tilemap Generation")]
    [SerializeField] private bool generateTilemap;
    [SerializeField] private TilemapGenerator tilemapGenerator;

    [Header("Compute Shaders")]
    [SerializeField] private ComputeShader cellularAutomataComputeShader;
    [SerializeField] private ComputeShader removeJaggedEdgesComputeShader;
    [SerializeField] private ComputeShader removeOneLineCorridorsComputeShader;

    private int[] map;
    private int minRoomSize;

    public void GenerateMap(int seed)
    {
        this.seed = seed;
        GenerateMap();
    }

    public void GenerateMap()
    {
        borderedMapSize = mapSize + borderSize * 2;
        map = new int[mapSize * mapSize];

        RandomFillMap();
        ComputeMap();

        minRoomSize = (int)(minSizeRoomPercentage / 100f * (mapSize * mapSize));
        map = GridRoomDetection.CleanUpRoomsInGrid(map, minRoomSize, mapSize, maxCorridorSize);

        ComputeMap(true);

        map = AddBorderToMap();

        PlacePlayer();

        if (generateTilemap)
        {
            tilemapGenerator.GenerateTilemap(map, borderedMapSize);
        }
    }

    private int[] AddBorderToMap()
    {
        int[] borderedMap = new int[borderedMapSize * borderedMapSize];

        for (int x = 0; x < borderedMapSize; x++)
        {
            for (int y = 0; y < borderedMapSize; y++)
            {
                if (x >= borderSize && x < mapSize + borderSize && y >= borderSize && y < mapSize + borderSize)
                {
                    borderedMap[x + y * borderedMapSize] = map[(x - borderSize) + (y - borderSize) * mapSize];
                }
                else
                {
                    borderedMap[x + y * borderedMapSize] = 1;
                }
            }
        }

        return borderedMap;
    }

    private void RandomFillMap()
    {
        System.Random rand = new(seed);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (x == 0 || x == mapSize - 1 || y == 0 || y == mapSize - 1)
                {
                    map[x + y * mapSize] = 1;
                }
                else
                {
                    map[x + y * mapSize] = rand.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    private void ComputeMap(bool onlyCleaning = false)
    {
        ComputeBuffer mapBuffer = new(map.Length, sizeof(int));
        mapBuffer.SetData(map);

        if (!onlyCleaning)
        {
            cellularAutomataComputeShader.SetBuffer(0, "map", mapBuffer);
            cellularAutomataComputeShader.SetInt("wallCutoff", wallCuttoff);
            cellularAutomataComputeShader.SetInt("mapSize", mapSize);

            for (int i = 0; i < smoothInterations; i++)
            {
                cellularAutomataComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
            }
        }

        removeJaggedEdgesComputeShader.SetBuffer(0, "map", mapBuffer);
        removeJaggedEdgesComputeShader.SetInt("mapSize", mapSize);

        removeOneLineCorridorsComputeShader.SetBuffer(0, "map", mapBuffer);
        removeOneLineCorridorsComputeShader.SetInt("mapSize", mapSize);

        for (int i = 0; i < cleanupIterations; i++)
        {
            removeJaggedEdgesComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
            removeOneLineCorridorsComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
        }

        mapBuffer.GetData(map);
        mapBuffer.Dispose();
    }

    private void PlacePlayer()
    {
        bool couldPlacePlayer = TryPlacePlayerWithAdjecentEmptyTiles();

        if (!couldPlacePlayer)
        {
            PlacePlayerOnFirstEmptyTile();
        }
    }

    private bool TryPlacePlayerWithAdjecentEmptyTiles()
    {
        for (int x = 0; x < borderedMapSize; x++)
        {
            for (int y = 0; y < borderedMapSize; y++)
            {
                int adjecentEmptyTileCounter = CountAdjecentEmptyTiles(x, y);

                if (adjecentEmptyTileCounter == 9)
                {
                    playerPrefab.transform.position = new Vector3(x + .5f, y + .5f, 0);
                    playerPrefab.SetActive(true);
                    return true;
                }
            }
        }

        return false;
    }

    private int CountAdjecentEmptyTiles(int centerX, int centerY)
    {
        int adjecentEmptyTileCounter = 0;

        for (int gridX = centerX - 1; gridX <= centerX + 1; gridX++)
        {
            for (int gridY = centerY - 1; gridY <= centerY + 1; gridY++)
            {
                int mapIndex = gridX + gridY * borderedMapSize;
                if (mapIndex >= map.Length || mapIndex < 0) { continue; }

                if (map[mapIndex] == 0)
                {
                    adjecentEmptyTileCounter++;
                }
            }
        }

        return adjecentEmptyTileCounter;
    }

    private void PlacePlayerOnFirstEmptyTile()
    {
        for (int x = 0; x < borderedMapSize; x++)
        {
            for (int y = 0; y < borderedMapSize; y++)
            {
                int mapIndex = x + y * borderedMapSize;
                if (mapIndex >= map.Length || mapIndex < 0) { continue; }

                if (map[mapIndex] == 0)
                {
                    playerPrefab.transform.position = new Vector3(x + .5f, y + .5f, 0);
                    playerPrefab.SetActive(true);
                    return;
                }
            }
        }
    }
}
