using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonTilemapGenerator : MonoBehaviour
{
    [Header("General")]
    public bool autoUpdate;
    [SerializeField] private int mapSize;
    [SerializeField] private int seed;

    [Header("Walls")]
    [SerializeField, Range(0, 100)] private int randomFillPercent;
    [SerializeField, Range(0, 100)] private int smoothInterations;
    //[SerializeField, Range(0, 10)] private int jaggerdIterations;
    [SerializeField, Range(0, 9)] private int wallCuttoff;

    [Header("Tilemap Generation")]
    [SerializeField] private bool generateTilemap;
    [SerializeField] private TilemapGenerator tilemapGenerator;

    [Header("Compute Shader")]
    [SerializeField] private ComputeShader cellularAutomataComputeShader;
    [SerializeField] private ComputeShader removeJaggedEdgesComputeShader;

    private int[] map;

    private void Start()
    {
        
    }

    public void GenerateMap()
    {
        map = new int[mapSize * mapSize];
        RandomFillMap();

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

    private void OnDrawGizmos()
    {
        if (map != null && !generateTilemap)
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Gizmos.color = map[x + y * mapSize] == 1 ? Color.black : Color.green;
                    Vector3 position = new(-mapSize / 2 + x + .5f, -mapSize / 2 + y + .5f, 0);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }
}
