using Autohand;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    [SerializeField] private Tiles tiles;
    [SerializeField] private RoomData rooms;

    [System.Serializable] private struct Tiles
    {
        public List<GameObject> floorTiles;
        public List<GameObject> wallTiles;
        public List<GameObject> ceilingTiles;
        public Vector3 tileScale;
    }
    [System.Serializable] private struct RoomData
    {
        [Header("Do Not Scale")]
        public int numRooms;
        public GameObject block;
        public float sizeMultiplier;
        [Space]
        [Header("Scale Together")]
        public Vector2 positionRangeX;
        public Vector2 positionRangeY;
        public Vector2 positionRangeZ;
        public float minimumDistance;
        public float minimumSize;
    }

    private void Awake()
    {

    }

    private void Start()
    {
        List<GameObject> rooms = GeneratePlaceHolders();
        Debug.Log("Generated " + rooms.Count + " rooms");
    }

    private List<GameObject> GeneratePlaceHolders()
    {
        List<GameObject> allRooms = new();

        List<Vector3> originPoints = GenerateOriginPoints();
        List<Vector3> roomSizes = GenerateSizes(ref originPoints);

        GameObject room;

        for (int i = 0; i < originPoints.Count; i++)
        {
            room = Instantiate(rooms.block, originPoints[i] + Vector3.up * (int)Random.Range(rooms.positionRangeY.x, rooms.positionRangeY.y), Quaternion.Euler(0, 0, 0));
            room.transform.localScale = roomSizes[i];

            room.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            allRooms.Add(room);
        }


        return allRooms;
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

            // Set all directions to be positive so they can function as side lengths
            roomSize.x = Mathf.Abs(roomSize.x);
            roomSize.y = Mathf.Abs(roomSize.y);
            roomSize.z = Mathf.Abs(roomSize.z);
            roomSize *= rooms.sizeMultiplier;
            roomSize.x = (int)Mathf.Clamp(roomSize.x, rooms.minimumSize, Mathf.Infinity);
            roomSize.y = (int)Mathf.Clamp(roomSize.y, rooms.minimumSize, Mathf.Infinity);
            roomSize.z = (int)Mathf.Clamp(roomSize.z, rooms.minimumSize, Mathf.Infinity);


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

        for (int i = 0; i < rooms.numRooms; i++)
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
            roomOrigin = SetOrigin(rooms);
            tooClose = false;
            foreach (Vector3 pos in roomPositions)
            {
                float distance = Vector3.Distance(roomOrigin, pos);
                if (distance < rooms.minimumDistance)
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

        //roomOrigin.y = 0;

        return roomOrigin;
    }
}
