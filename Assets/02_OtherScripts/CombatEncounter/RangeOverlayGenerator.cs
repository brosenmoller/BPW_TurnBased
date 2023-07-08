using UnityEngine;
using UnityEngine.Tilemaps;

public class RangeOverlayGenerator : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private Tilemap rangeOverlayTilemap;
    [SerializeField] private TileBase movementRangeOverlayTile;
    [SerializeField] private TileBase enemyHighlightOverlayTile;
    [SerializeField] private TileBase attackRangeOverlayTile;

    public void GenerateMovementRangeOverlay(Vector3Int[] WalkableTiles, Vector3Int[] enemyTiles)
    {
        foreach (Vector3Int coordinate in WalkableTiles)
        {
            rangeOverlayTilemap.SetTile(coordinate, movementRangeOverlayTile);
        }

        foreach (Vector3Int coordinate in enemyTiles)
        {
            rangeOverlayTilemap.SetTile(coordinate, enemyHighlightOverlayTile);
        }
    }

    public void GenerateAttackRangeOverlay(Vector3Int[] attackRangeTiles)
    {
        foreach (Vector3Int coordinate in attackRangeTiles)
        {
            rangeOverlayTilemap.SetTile(coordinate, attackRangeOverlayTile);
        }
    }

    public void ClearRangeOverlay()
    {
        rangeOverlayTilemap.ClearAllTiles();
    }
}

