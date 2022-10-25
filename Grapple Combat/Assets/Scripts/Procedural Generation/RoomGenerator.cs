using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomData roomsData;

    #region Data
    public List<Node> roomOrigins { get; private set; } = new();
    public List<Vector3Int> roomSizes { get; private set; } = new();
    #endregion

    private Grid3D grid;
    int unitSize;

    [System.Serializable] private struct RoomData
    {
        [Header("Do Not Scale")]
        public int numRooms;
        public int sizeScale;
        [Space]
        [Header("Scale Together")]
        public Vector2Int positionRangeX;
        public Vector2Int positionRangeY;
        public Vector2Int positionRangeZ;
        public float minimumDistance;
    }

    private void Awake()
    {
        grid = GetComponent<Grid3D>();
        UpdateRoomScales();
    }

    public void GenerateRoomData()
    {
        roomOrigins = new();
        List<Vector3Int> originPoints = GenerateOriginPoints();
        roomSizes = GenerateSizes(ref originPoints);

        for (int i = 0; i < originPoints.Count; i++)
        {
            int y = Random.Range(roomsData.positionRangeY.x, roomsData.positionRangeY.y);
            y /= unitSize;
            y *= unitSize;
            originPoints[i] += Vector3Int.up * y;

            Node v = new(originPoints[i]);
            roomOrigins.Add(v);
        }
    }

    public void GetGridCoordinates(ref Vector2Int x, ref Vector2Int y, ref Vector2Int z)
    {
        x = roomsData.positionRangeX;
        y = roomsData.positionRangeY;
        z = roomsData.positionRangeZ;
    }

    private void UpdateRoomScales()
    {
        roomsData.positionRangeX *= roomsData.sizeScale;
        roomsData.positionRangeY *= roomsData.sizeScale;
        roomsData.positionRangeZ *= roomsData.sizeScale;
        roomsData.minimumDistance *= roomsData.sizeScale;
        unitSize = grid.GetUnitSize();
        roomsData.minimumDistance = Mathf.Max(unitSize, roomsData.minimumDistance);
    }

    private List<Vector3Int> GenerateSizes(ref List<Vector3Int> originPoints)
    {
        List<Vector3Int> roomSizes = new();
        Vector3 closestRoom;
        Vector3Int roomSize;

        foreach(Vector3Int currentRoom in originPoints)
        {
            // Set Room Sizes based on closest room distance
            closestRoom = FindLowestDistance(originPoints, currentRoom);
            float distance = Vector3.Distance(currentRoom, closestRoom);
            int length = (int)Mathf.Sqrt(Mathf.Pow(distance, 2) / 2);
            Vector3 random = new(Random.Range(0.75f, 1), Random.Range(1, 3), Random.Range(0.75f, 1));

            roomSize = new(
                (int)(length * random.x), 
                (int)random.y * unitSize, 
                (int)(length * random.z));

            roomSize /= unitSize;
            roomSize *= unitSize;
            roomSizes.Add(roomSize);
        }

        return roomSizes;
    }

    private Vector3 FindLowestDistance(List<Vector3Int> originPoints, Vector3Int currentRoom)
    {
        float minimumDistance = Mathf.Infinity;
        Vector3 closestRoom = new();
        foreach (Vector3 comparisonRoom in originPoints)
        {
            if (currentRoom != comparisonRoom)
            {
                float dist = Vector3.Distance(currentRoom, comparisonRoom);
                if (dist < minimumDistance)
                {
                    minimumDistance = dist;
                    closestRoom = comparisonRoom;
                }
            }
        }
        return closestRoom;
    }

    private List<Vector3Int> GenerateOriginPoints()
    {
        bool tooClose = false;
        Vector3Int roomOrigin = new();
        List<Vector3Int> roomPositions = new();

        for (int i = 0; i < roomsData.numRooms; i++)
        {
            if (!tooClose)
            {
                roomOrigin /= unitSize * 2;
                roomOrigin *= unitSize * 2;
                roomPositions.Add(roomOrigin);
            }

            // reset data for the distance-checking while-loop
            tooClose = true;
            int loopCounter = 0;

            // check to see that origin isn't too close to any already existing points
            CheckDistanceToOtherRooms(ref tooClose, ref roomOrigin, roomPositions, ref loopCounter);
        }
        return roomPositions;
    }

    private void CheckDistanceToOtherRooms(ref bool tooClose, ref Vector3Int roomOrigin, List<Vector3Int> roomPositions, ref int loopCounter)
    {
        while (tooClose)
        {
            roomOrigin = SetOrigin(roomsData);
            tooClose = false;
            foreach (Vector3 pos in roomPositions)
            {
                float distance = Vector3.Distance(roomOrigin, pos);
                if (distance < roomsData.minimumDistance)
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

    private Vector3Int SetOrigin(RoomData rooms)
    {
        Vector3Int roomOrigin = new();
        roomOrigin.x = (int)Random.Range(rooms.positionRangeX.x, rooms.positionRangeX.y + 0.99f);
        roomOrigin.y = 0;
        roomOrigin.z = (int)Random.Range(rooms.positionRangeZ.x, rooms.positionRangeZ.y + 0.99f);
        return roomOrigin;
    }
}
