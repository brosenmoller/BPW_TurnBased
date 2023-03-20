using UnityEngine;
using UnityEngine.Tilemaps;

public class RangeOverlayGenerator : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private Tilemap movementRangeOverlayTilemap;
    [SerializeField] private TileBase movementRangeOverlayRuleTile;

    public void GenerateMovementRangeOverlay(Vector3Int[] tilePositions)
    {
        foreach (Vector3Int coordinate in tilePositions)
        {
            movementRangeOverlayTilemap.SetTile(coordinate, movementRangeOverlayRuleTile);
        }
    }

    public void ClearMovementRangeOverlay()
    {
        movementRangeOverlayTilemap.ClearAllTiles();
    }
}

