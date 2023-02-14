using System.Collections.Generic;
using UnityEngine;

public static class GridRoomDetection
{
    private static readonly List<Room> rooms = new();

    private static int[] map;
    private static int minRoomSize;
    private static int mapSize;

    public static int[] CleanUpRoomsInGrid(int[] map, int minRoomSize, int mapSize)
    {
        GridRoomDetection.map = map;
        GridRoomDetection.minRoomSize = minRoomSize;
        GridRoomDetection.mapSize = mapSize;

        rooms.Clear();
        Room.allCoordinatesWithAssignedRoom.Clear();
        Room.largestRoom = null;

        DetectAllRooms();
        RemoveSmallRooms();
        ConnectClosestRooms();

        return map;
    }

    private static void DetectAllRooms()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                Vector2Int currentCoodinate = new(x, y);
                if (Room.allCoordinatesWithAssignedRoom.Contains(currentCoodinate)) { continue; }

                if (map[x + y * mapSize] == 0)
                {
                    rooms.Add(new Room(currentCoodinate));
                }
            }
        }
    }
    private static void RemoveSmallRooms()
    {
        HashSet<Room> roomsToRemove = new();

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].roomSize < minRoomSize)
            {
                foreach (Vector2Int coordinate in rooms[i].memberCoordinates)
                {
                    map[coordinate.x + coordinate.y * mapSize] = 1;
                }

                roomsToRemove.Add(rooms[i]);
                Room.largestRoom.isAccesableFromLargestRoom = true;
            }
        }

        foreach (Room roomMarkedForRemoval in roomsToRemove)
        {
            rooms.Remove(roomMarkedForRemoval);
        }
    }

    private static void ConnectClosestRooms()
    {
        if (rooms.Count <= 1) { return; }

        FindConnectionsBetweenRooms(rooms, rooms);

        List<Room> unconnectedRooms;
        List<Room> connectedRooms;

        GetConnectedAndUnConnectedLists(out connectedRooms, out unconnectedRooms);

        while (unconnectedRooms.Count > 0)
        {
            Debug.Log(unconnectedRooms.Count);
            FindConnectionsBetweenRooms(connectedRooms, unconnectedRooms, true);
            GetConnectedAndUnConnectedLists(out connectedRooms, out unconnectedRooms);
        }
    }

    private static void GetConnectedAndUnConnectedLists(out List<Room> connectedRooms, out List<Room> unconnectedRooms)
    {
        connectedRooms = new List<Room>();
        unconnectedRooms = new List<Room>();

        foreach (Room room in rooms)
        {
            if (room.isAccesableFromLargestRoom) { connectedRooms.Add(room); }
            else { unconnectedRooms.Add(room); }
        }
    }

    private static void FindConnectionsBetweenRooms(List<Room> roomListA, List<Room> roomListB, bool searchForBestOverallConnection = false)
    {
        Vector2Int closestEdgeRoomA = Vector2Int.zero;
        Vector2Int closestEdgeRoomB = Vector2Int.zero;
        Room closestRoomA = null;
        Room closestRoomB = null;

        bool connectionFound = false;
        int smallestDistance = mapSize * mapSize;

        foreach (Room roomA in roomListA)
        {
            if (!searchForBestOverallConnection) 
            { 
                connectionFound = false;
                smallestDistance = mapSize * mapSize;
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB)) { continue; }

                foreach (Vector2Int edgeCoordinateRoomA in roomA.edgeCoordinates)
                {
                    foreach (Vector2Int edgeCoordinateRoomB in roomB.edgeCoordinates)
                    {
                        int sqrDistance = (int)(Mathf.Pow(edgeCoordinateRoomA.x - edgeCoordinateRoomB.x, 2) +
                                                  Mathf.Pow(edgeCoordinateRoomA.y - edgeCoordinateRoomB.y, 2));

                        if (sqrDistance < smallestDistance)
                        {
                            closestEdgeRoomA = edgeCoordinateRoomA;
                            closestEdgeRoomB = edgeCoordinateRoomB;
                            closestRoomA = roomA;
                            closestRoomB = roomB;
                            smallestDistance = sqrDistance;
                            connectionFound = true;
                        }
                    }
                }
            }

            if (connectionFound && !searchForBestOverallConnection) 
            { 
                Room.ConnectRooms(roomA, closestRoomB);
                Debug.DrawLine(
                    CoordinateToWorldPosition(closestEdgeRoomA),
                    CoordinateToWorldPosition(closestEdgeRoomB),
                    Color.yellow,
                    10
                );
            }
        }

        if (connectionFound && searchForBestOverallConnection)
        {
            Room.ConnectRooms(closestRoomA, closestRoomB);
            Debug.DrawLine(
                CoordinateToWorldPosition(closestEdgeRoomA),
                CoordinateToWorldPosition(closestEdgeRoomB),
                Color.yellow,
                10
            );
        }
    }

    private static Vector3 CoordinateToWorldPosition(Vector2Int coordinate)
    {
        return new Vector3(.5f + coordinate.x, .5f + coordinate.y, 0);
    }

    private class Room
    {
        public static readonly HashSet<Vector2Int> allCoordinatesWithAssignedRoom = new();
        public readonly HashSet<Vector2Int> memberCoordinates = new();
        private readonly Queue<Vector2Int> coordinatesToCheck = new();

        public HashSet<Room> connectedRooms = new();
        public readonly HashSet<Vector2Int> edgeCoordinates = new();

        public static Room largestRoom = null;
        public bool isAccesableFromLargestRoom = false;
        public int roomSize;

        public Room(Vector2Int startCoordinate)
        {
            DetectRoom(startCoordinate);
            roomSize = memberCoordinates.Count;
            if (largestRoom == null || roomSize > largestRoom.roomSize)
            {
                largestRoom = this;
            }

            DetectEdgeCoordinates();
        }
        private void DetectRoom(Vector2Int startCoordinate)
        {
            coordinatesToCheck.Enqueue(startCoordinate);
            memberCoordinates.Add(startCoordinate);
            allCoordinatesWithAssignedRoom.Add(startCoordinate);

            while (coordinatesToCheck.Count > 0)
            {
                Vector2Int currentCoordinate = coordinatesToCheck.Dequeue();

                CheckCoordinate(new Vector2Int(currentCoordinate.x + 1, currentCoordinate.y));
                CheckCoordinate(new Vector2Int(currentCoordinate.x - 1, currentCoordinate.y));
                CheckCoordinate(new Vector2Int(currentCoordinate.x, currentCoordinate.y + 1));
                CheckCoordinate(new Vector2Int(currentCoordinate.x, currentCoordinate.y - 1));
            }
        }

        private void CheckCoordinate(Vector2Int coordinate)
        {
            if (memberCoordinates.Contains(coordinate)) { return; }

            int mapIndex = coordinate.x + coordinate.y * mapSize;
            if (mapIndex >= map.Length || mapIndex < 0) { return; }

            if (map[coordinate.x + coordinate.y * mapSize] == 0)
            {
                memberCoordinates.Add(coordinate);
                allCoordinatesWithAssignedRoom.Add(coordinate);
                coordinatesToCheck.Enqueue(coordinate);
            }
        }

        private void DetectEdgeCoordinates()
        {
            foreach (Vector2Int coordinate in memberCoordinates)
            {
                if (map[(coordinate.x - 1) + coordinate.y * mapSize] == 1 ||
                    map[(coordinate.x + 1) + coordinate.y * mapSize] == 1 ||
                    map[coordinate.x + (coordinate.y - 1) * mapSize] == 1 ||
                    map[coordinate.x + (coordinate.y + 1) * mapSize] == 1)
                {
                    edgeCoordinates.Add(coordinate);
                }
            }
        }

        public void SetIsAccesableFromLargestRoom()
        {
            if (isAccesableFromLargestRoom) { return; }

            isAccesableFromLargestRoom = true;

            foreach (Room connectedRoom in connectedRooms)
            {
                connectedRoom.SetIsAccesableFromLargestRoom();
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccesableFromLargestRoom) { roomB.SetIsAccesableFromLargestRoom(); }
            else if (roomB.isAccesableFromLargestRoom) { roomA.SetIsAccesableFromLargestRoom(); }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }
        public bool IsConnected(Room otherRoom) => connectedRooms.Contains(otherRoom);
    }
}
