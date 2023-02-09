using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] private int mapSize;
    [SerializeField] private int seed;

    [Header("Walls")]
    [SerializeField, Range(0, 100)] private int randomFillPercent;
    [SerializeField, Range(0, 100)] private int smoothInterations;
    [SerializeField, Range(0, 100)] private int minSizeRoomPercentage;
    [SerializeField, Range(0, 9)] private int wallCuttoff;

    [Header("Tilemap Generation")]
    [SerializeField] private bool generateTilemap;
    [SerializeField] private TilemapGenerator tilemapGenerator;

    [Header("Compute Shaders")]
    [SerializeField] private ComputeShader cellularAutomataComputeShader;
    [SerializeField] private ComputeShader removeJaggedEdgesComputeShader;

    private int[] map;
    private int minRoomSize;

    private void Awake()
    {
        minRoomSize = (minSizeRoomPercentage / 100) * (mapSize * mapSize);
    }

    public void GenerateMap()
    {
        map = new int[mapSize * mapSize];
        RandomFillMap();
        ComputeMap();

        //map = GridRoomCleanup.CleanUpRoomsInGrid(map, minRoomSize);

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

        mapBuffer.GetData(map);
        mapBuffer.Dispose();
    }
}
