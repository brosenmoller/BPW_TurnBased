using System.Collections.Generic;
using UnityEngine;

public enum GridTileContentType
{
    Player = 0,
    Enemy = 1,
}

public class GridTileContent
{

}


public class CombatRoomController : MonoBehaviour
{
    public Dictionary<Vector3Int, (GridTileContentType, GridTileContent)> gridTilesContent = new();


}



