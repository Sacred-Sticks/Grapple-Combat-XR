using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomData roomsData;

    [System.Serializable] private struct RoomData
    {
        [Header("Do Not Scale")]
        public int numRooms;
        public float sizeScale;
        [Space]
        [Header("Scale Together")]
        public Vector2 positionRangeX;
        public Vector2 positionRangeY;
        public Vector2 positionRangeZ;
        public float minimumDistance;
    }

    private List<Vertex> roomOrigins = new();
    private List<Vector3> roomSizes = new();

    private Delauney3D delauney;

    private void Awake()
    {
        delauney = GetComponent<Delauney3D>();
    }

    private void Start()
    {
        UpdateRoomScales();
        GenerateRoomData();
        delauney.BowyerWatson(roomOrigins);
    }

    private void UpdateRoomScales()
    {
        roomsData.positionRangeX *= roomsData.sizeScale;
        roomsData.positionRangeY *= roomsData.sizeScale;
        roomsData.positionRangeZ *= roomsData.sizeScale;
        roomsData.minimumDistance *= roomsData.sizeScale;
    }

    private void GenerateRoomData()
    {
        List<Vertex> roomOrigins = new();
        List<Vector3> originPoints = GenerateOriginPoints();
        List<Vector3> roomSizes = GenerateSizes(ref originPoints);

        for (int i = 0; i < originPoints.Count; i++)
        {
            originPoints[i] += Vector3.up * (int)Random.Range(roomsData.positionRangeY.x, roomsData.positionRangeY.y);

            Vertex v = new();
            v.position = originPoints[i];
            roomOrigins.Add(v);
        }

        this.roomOrigins = roomOrigins;
        this.roomSizes = roomSizes;
    }

    private List<Vector3> GenerateSizes(ref List<Vector3> originPoints)
    {
        List<Vector3> roomSizes = new();
        Vector3 closestRoom;
        Vector3 roomSize;

        foreach(Vector3 currentRoom in originPoints)
        {
            closestRoom = FindLowestDistance(originPoints, currentRoom);
            roomSize = (closestRoom - currentRoom);

            // Set Room Sizes based on closest room distance

            float distance = Vector3.Distance(currentRoom, closestRoom);
            int length = (int)Mathf.Sqrt(Mathf.Pow(distance, 2) / 2);
            Vector3 random = new(Random.Range(0.75f, 1), Random.Range(0.25f, 0.5f), Random.Range(0.75f, 1));

            roomSize = new((int)(length * random.x), (int)(length * random.y), (int)(length * random.z));

            roomSizes.Add(roomSize);
        }

        return roomSizes;
    }

    private Vector3 FindLowestDistance(List<Vector3> originPoints, Vector3 currentRoom)
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

    private List<Vector3> GenerateOriginPoints()
    {
        bool tooClose = false;
        Vector3 roomOrigin = new();
        List<Vector3> roomPositions = new();

        for (int i = 0; i < roomsData.numRooms; i++)
        {
            if (!tooClose)
            {
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

    private void CheckDistanceToOtherRooms(ref bool tooClose, ref Vector3 roomOrigin, List<Vector3> roomPositions, ref int loopCounter)
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

    private Vector3 SetOrigin(RoomData rooms)
    {
        Vector3 roomOrigin;
        roomOrigin.x = (int)Random.Range(rooms.positionRangeX.x, rooms.positionRangeX.y + 0.99f);
        roomOrigin.y = 0;
        roomOrigin.z = (int)Random.Range(rooms.positionRangeZ.x, rooms.positionRangeZ.y + 0.99f);
        return roomOrigin;
    }

    private void OnDrawGizmos()
    {
        if (roomOrigins == null) return;
        Gizmos.color = Color.red;
        int rooms = 0;
        for (int i = 0; i < roomOrigins.Count; i++)
        {
            Gizmos.DrawCube(roomOrigins[i].position, roomSizes[i]);
            rooms = i;
        }
    }
}
