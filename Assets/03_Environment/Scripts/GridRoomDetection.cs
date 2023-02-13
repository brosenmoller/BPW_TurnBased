using System.Collections.Generic;
using UnityEngine;

public static class GridRoomDetection
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
        Room.allCoordinatesWithAssignedRoom.Clear();

        DetectAllRooms();
        RemoveSmallRooms();

        return map;
    }

    private static void RemoveSmallRooms()
    {
        foreach (Room room in rooms)
        {
            if (room.memberCoordinates.Count < minRoomSize)
            {
                foreach (Vector2Int coordinate in room.memberCoordinates)
                {
                    map[coordinate.x + coordinate.y * mapSize] = 1;
                }
            }
        }
    }

    private static void DetectAllRooms()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                Vector2Int currentPosition = new(x, y);
                if (Room.allCoordinatesWithAssignedRoom.Contains(currentPosition)) { continue; }

                if (map[x + y * mapSize] == 0)
                {
                    rooms.Add(new Room(currentPosition, 0));
                }
            }
        }
    }

    private class Room
    {
        public static readonly HashSet<Vector2Int> allCoordinatesWithAssignedRoom = new();
        public readonly HashSet<Vector2Int> memberCoordinates = new();
        private readonly Queue<Vector2Int> positionsToCheck = new();

        private readonly int roomType;

        public Room(Vector2Int startPosition, int roomType)
        {
            this.roomType = roomType;

            positionsToCheck.Enqueue(startPosition);
            memberCoordinates.Add(startPosition);
            allCoordinatesWithAssignedRoom.Add(startPosition);

            DetectRoom();
        }

        private void DetectRoom()
        {
            while (positionsToCheck.Count > 0)
            {
                Vector2Int currentPosition = positionsToCheck.Dequeue();

                CheckPosition(new Vector2Int(currentPosition.x + 1, currentPosition.y));
                CheckPosition(new Vector2Int(currentPosition.x - 1, currentPosition.y));
                CheckPosition(new Vector2Int(currentPosition.x, currentPosition.y + 1));
                CheckPosition(new Vector2Int(currentPosition.x, currentPosition.y - 1));
            }
        }

        private void CheckPosition(Vector2Int position)
        {
            if (memberCoordinates.Contains(position)) { return; }

            int mapIndex = position.x + position.y * mapSize;
            if (mapIndex >= map.Length || mapIndex < 0) { return; }

            if (map[position.x + position.y * mapSize] == roomType)
            {
                memberCoordinates.Add(position);
                allCoordinatesWithAssignedRoom.Add(position);
                positionsToCheck.Enqueue(position);
            }
        }
    }
}
