using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap colliderTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private RuleTile filledRuleTile;
    [SerializeField] private RuleTile emptyRuleTile;

    public void GenerateTilemap(int[] map, int mapSize)
    {
        groundTilemap.ClearAllTiles();
        colliderTilemap.ClearAllTiles();

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (map[x + y * mapSize] == 1)
                {
                    colliderTilemap.SetTile(new Vector3Int(x, y, 0), filledRuleTile);
                }
                else
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), emptyRuleTile);
                }
            }
        }
    }
}
