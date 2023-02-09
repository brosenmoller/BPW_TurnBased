using System.Collections.Generic;
using UnityEngine;

public static class GridRoomCleanup
{
    private static readonly List<Room> rooms = new();

    private static int[] map;
    private static int minRoomSize;
    private static int mapSize;

    public static int[] CleanUpRoomsInGrid(int[] _map, int _minRoomSize, int _mapSize)
    {
        map = _map;
        minRoomSize = _minRoomSize;
        mapSize = _mapSize;
        rooms.Clear();

        RemoveSmallRooms();

        return map;
    }

    private static void RemoveSmallRooms()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                Vector2Int currentPosition = new(x, y);
                if (Room.allCoordinatesWithAssignedRoom.Contains(currentPosition)) { continue; }

                if (map[x + y * mapSize] == 0)
                {
                    rooms.Add(new Room(currentPosition));
                }
            }
        }
    }


    private class Room
    {
        public static readonly HashSet<Vector2Int> allCoordinatesWithAssignedRoom = new();
        public readonly HashSet<Vector2Int> memberCoordinates = new();
        private readonly Queue<Vector2Int> positionsToCheck = new();

        public Room(Vector2Int startPosition)
        {
            positionsToCheck.Enqueue(startPosition);
            
            while(positionsToCheck.Count > 0)
            {
                Vector2Int currentPosition = positionsToCheck.Dequeue();
                memberCoordinates.Add(currentPosition);
                allCoordinatesWithAssignedRoom.Add(currentPosition);

                if (map[(currentPosition.x + 1) + currentPosition.y * mapSize] == 0) 
                { 
                    positionsToCheck.Enqueue(new Vector2Int(currentPosition.x + 1, currentPosition.y)); 
                }
                if (map[(currentPosition.x - 1) + currentPosition.y * mapSize] == 0) 
                { 
                    positionsToCheck.Enqueue(new Vector2Int(currentPosition.x - 1, currentPosition.y)); 
                }
                if (map[currentPosition.x + (currentPosition.y + 1) * mapSize] == 0) 
                { 
                    positionsToCheck.Enqueue(new Vector2Int(currentPosition.x, currentPosition.y + 1)); 
                }
                if (map[currentPosition.x + (currentPosition.y - 1) * mapSize] == 0) 
                { 
                    positionsToCheck.Enqueue(new Vector2Int(currentPosition.x, currentPosition.y - 1)); 
                }
            }

            if (memberCoordinates.Count < minRoomSize)
            {
                foreach (Vector2Int coordinate in memberCoordinates)
                {
                    map[coordinate.x + coordinate.y * mapSize] = 1;
                }

                rooms.Remove(this);
            }
        }
    }
}
