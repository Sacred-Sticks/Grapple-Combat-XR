using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomData roomsData;
    [SerializeField] private LocationData locationData;

    #region Data
    public List<Node> Nodes { get; private set; } = new();
    #endregion

    private Grid3D grid;
    int unitSize;
    Vector3Int roomSection = new();

    [System.Serializable] private struct RoomData
    {
        [Header("Do Not Scale")]
        public int numRooms;
        [Space]
        [Header("Scaling Data")]
        public float minimumUnitsDistance;
    }
    [System.Serializable] private struct LocationData
    {
        [Header("Size of Grid for room Spawning")]
        public Vector2Int tileRangeX;
        public Vector2Int tileRangeY;
        public Vector2Int tileRangeZ;
        [Header("Spread Spawning by Grid Cells")]
        public Vector3Int sectionsGrid;
    }

    private void Awake()
    {
        grid = GetComponent<Grid3D>();
        UpdateRoomScales();
    }

    public void GenerateRoomData()
    {
        Nodes = new();
        List<Vector3> originPoints = GenerateOriginPoints();
        List<Vector3> roomSizes = GenerateSizes(ref originPoints);

        for (int i = 0; i < originPoints.Count; i++)
        {
            int y = (int)Random.Range(locationData.tileRangeY.x, locationData.tileRangeY.y + unitSize - 0.1f);
            y /= unitSize;
            y *= unitSize;
            originPoints[i] += Vector3.up.y * roomSizes[i] / 2 + Vector3.up * y;

            Node v = new(originPoints[i], roomSizes[i]);
            Nodes.Add(v);
        }
    }

    public void GetGridCoordinates(ref Vector2Int x, ref Vector2Int y, ref Vector2Int z)
    {
        x = locationData.tileRangeX;
        y = locationData.tileRangeY;
        z = locationData.tileRangeZ;
    }

    private void UpdateRoomScales()
    {
        roomsData.minimumUnitsDistance *= grid.GetUnitSize();
        unitSize = grid.GetUnitSize();
        locationData.tileRangeX *= unitSize;
        locationData.tileRangeY *= unitSize;    
        locationData.tileRangeZ *= unitSize;
        //roomsData.minimumDistance = Mathf.Max(unitSize, roomsData.minimumDistance);
    }

    private List<Vector3> GenerateSizes(ref List<Vector3> originPoints)
    {
        List<Vector3> roomSizes = new();
        Vector3 closestRoom;
        Vector3Int roomSize;

        foreach(Vector3 currentRoom in originPoints)
        {
            // Set Room Sizes based on closest room distance
            // Add Modification to delete rooms that are too small are recalculate size
            closestRoom = FindClosestRoom(originPoints, currentRoom);
            float distance = Vector3.Distance(currentRoom, closestRoom);
            float length = Mathf.Sqrt(Mathf.Pow(distance, 2) / 2);
            Vector3 random = new(Random.Range(0.75f, 1), Random.Range(0.2f, 0.4f), Random.Range(0.75f, 1));

            roomSize = new()
            {
                x = Mathf.Max((int)(length * random.x), unitSize),
                y = Mathf.Max((int)(length * random.y), unitSize),
                z = Mathf.Max((int)(length * random.z), unitSize)
            };

            roomSize /= unitSize;
            roomSize *= unitSize;
            roomSizes.Add(roomSize);
        }

        return roomSizes;
    }

    private Vector3 FindClosestRoom(List<Vector3> originPoints, Vector3 currentRoom)
    {
        double lowestDistance = Mathf.Infinity;
        Vector3 closestRoom = new();
        foreach (Vector3 comparisonRoom in originPoints)
        {
            if (currentRoom != comparisonRoom)
            {
                float dist = Vector3.Distance(currentRoom, comparisonRoom);
                if (dist < lowestDistance)
                {
                    lowestDistance = dist;
                    closestRoom = comparisonRoom;
                }
            }
        }
        return closestRoom;
    }

    private List<Vector3> GenerateOriginPoints()
    {
        bool tooClose = false;
        Vector3 roomOrigin = SetOrigin(locationData);
        List<Vector3> roomPositions = new();

        for (int i = 0; i < roomsData.numRooms; i++)
        {
            if (!tooClose)
            {
                roomPositions.Add(roomOrigin);
                roomSection.x++;
                if (roomSection.x >= locationData.sectionsGrid.x)
                {
                    roomSection.x = 0;
                    roomSection.z++;
                }
                if (roomSection.z >= locationData.sectionsGrid.z) roomSection.z = 0;
            }

            // reset data for the distance-checking while-loop
            tooClose = true;
            int loopCounter = 0;

            // check to see that origin isn't too close to any already existing points
            CheckDistanceToOtherRooms(ref tooClose, ref roomOrigin, roomPositions, ref loopCounter);
        }
        return roomPositions;
    }

    private void CheckDistanceToOtherRooms(ref bool tooClose, ref Vector3 roomOrigin, List<Vector3> roomPositions, ref int loopCounter)
    {
        while (tooClose)
        {
            roomOrigin = SetOrigin(locationData);
            tooClose = false;
            foreach (Vector3 pos in roomPositions)
            {
                float distance = Vector3.Distance(roomOrigin, pos);
                if (distance < roomsData.minimumUnitsDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            // break the loop if a spot for the room cannot be found
            loopCounter++;
            if (loopCounter > roomPositions.Count * 2) break;
        }
    }

    private Vector3Int SetOrigin(LocationData locationData)
    {
        Vector3Int roomOrigin = new();
        //roomOrigin.x = Random.Range(locationData.positionRangeX.x, locationData.positionRangeX.y);
        //roomOrigin.y = 0;
        //roomOrigin.z = Random.Range(locationData.positionRangeZ.x, locationData.positionRangeZ.y);

        int xDif = (locationData.tileRangeX.y - locationData.tileRangeX.x) / locationData.sectionsGrid.x;
        int yDif = (locationData.tileRangeY.y - locationData.tileRangeY.x) / locationData.sectionsGrid.y;
        int zDif = (locationData.tileRangeZ.y - locationData.tileRangeZ.x) / locationData.sectionsGrid.z;

        // New Origin Test
        roomOrigin.x = xDif * roomSection.x + Random.Range(0, xDif);
        roomOrigin.y = 0;
        roomOrigin.z = zDif * roomSection.z + Random.Range(0, zDif);

        roomOrigin /= unitSize * 2;
        roomOrigin *= unitSize * 2;
        return roomOrigin;
    }
}
