using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] private int mapSize;
    [SerializeField] private int seed;

    [Header("Cellular Automata")]
    [SerializeField, Range(0, 100)] private int randomFillPercent;
    [SerializeField, Range(0, 100)] private int smoothInterations;
    
    [SerializeField, Range(0, 9)] private int wallCuttoff;

    [Header("Finalization")]
    [SerializeField, Range(0, 100)] private int cleanupIterations;
    [SerializeField, Range(0, 20)] private float minSizeRoomPercentage;

    [Header("Tilemap Generation")]
    [SerializeField] private bool generateTilemap;
    [SerializeField] private TilemapGenerator tilemapGenerator;

    [Header("Compute Shaders")]
    [SerializeField] private ComputeShader cellularAutomataComputeShader;
    [SerializeField] private ComputeShader removeJaggedEdgesComputeShader;
    [SerializeField] private ComputeShader removeOneLineCorridorsComputeShader;
    [SerializeField] private ComputeShader gridCleanupComputeShader;

    private int[] map;
    private int minRoomSize;

    public void GenerateMap()
    {
        map = new int[mapSize * mapSize];
        RandomFillMap();
        ComputeMap();

        minRoomSize = (int)(minSizeRoomPercentage / 100f * (mapSize * mapSize));
        map = GridRoomDetection.CleanUpRoomsInGrid(map, minRoomSize, mapSize);

        if (generateTilemap)
        {
            tilemapGenerator.GenerateTilemap(map, mapSize);
        }
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

    private void ComputeMap()
    {
        ComputeBuffer mapBuffer = new(map.Length, sizeof(int));
        mapBuffer.SetData(map);

        cellularAutomataComputeShader.SetBuffer(0, "map", mapBuffer);
        cellularAutomataComputeShader.SetInt("wallCutoff", wallCuttoff);
        cellularAutomataComputeShader.SetInt("mapSize", mapSize);

        for (int i = 0; i < smoothInterations; i++)
        {
            cellularAutomataComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
        }

        removeJaggedEdgesComputeShader.SetBuffer(0, "map", mapBuffer);
        removeJaggedEdgesComputeShader.SetInt("mapSize", mapSize);

        removeJaggedEdgesComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
        

        removeOneLineCorridorsComputeShader.SetBuffer(0, "map", mapBuffer);
        removeOneLineCorridorsComputeShader.SetInt("mapSize", mapSize);

        removeOneLineCorridorsComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);

        //gridCleanupComputeShader.SetBuffer(0, "map", mapBuffer);
        //gridCleanupComputeShader.SetInt("mapSize", mapSize);

        //for (int i = 0; i < cleanupIterations; i++)
        //{
        //    gridCleanupComputeShader.Dispatch(0, mapSize / 16, mapSize / 16, 1);
        //}

        mapBuffer.GetData(map);
        mapBuffer.Dispose();
    }
}
