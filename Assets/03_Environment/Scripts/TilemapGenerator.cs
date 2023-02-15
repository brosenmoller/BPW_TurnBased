using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap colliderTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private RuleTile roofRuleTile;
    [SerializeField] private RuleTile floorRuleTile;
    [SerializeField] private RuleTile wallRuleTile;

    public void GenerateTilemap(int[] map, int mapSize)
    {
        groundTilemap.ClearAllTiles();
        colliderTilemap.ClearAllTiles();

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (map[x + y * mapSize] == 0)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), floorRuleTile);
                    continue;
                }

                
                int mapIndex = x + (y - 1) * mapSize;
                if (mapIndex < map.Length && mapIndex >= 0)
                {
                    if (map[mapIndex] == 0)
                    {
                        colliderTilemap.SetTile(new Vector3Int(x, y, 0), wallRuleTile);
                        continue;
                    }
                }

                colliderTilemap.SetTile(new Vector3Int(x, y, 0), roofRuleTile);
            }
        }
    }
}
