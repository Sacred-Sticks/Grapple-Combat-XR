using Autohand;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    [SerializeField] private RoomData rooms;

    [System.Serializable] private struct RoomData
    {
        [Header("Do Not Scale")]
        public int numRooms;
        public GameObject block;
        public float sizeScale;
        [Space]
        [Header("Scale Together")]
        public Vector2 positionRangeX;
        public Vector2 positionRangeY;
        public Vector2 positionRangeZ;
        public float minimumDistance;
    }

    private void Awake()
    {

    }

    private void Start()
    {
        UpdateRoomScales();
        List<GameObject> rooms = GeneratePlaceHolders();
        Debug.Log("Generated " + rooms.Count + " rooms");
    }

    private void UpdateRoomScales()
    {
        rooms.positionRangeX *= rooms.sizeScale;
        rooms.positionRangeY *= rooms.sizeScale;
        rooms.positionRangeZ *= rooms.sizeScale;
        rooms.minimumDistance *= rooms.sizeScale;
    }

    private List<GameObject> GeneratePlaceHolders()
    {
        List<GameObject> allRooms = new();

        List<Vector3> originPoints = GenerateOriginPoints();
        List<Vector3> roomSizes = GenerateSizes(ref originPoints);

        GameObject room;

        for (int i = 0; i < originPoints.Count; i++)
        {
            room = Instantiate(rooms.block, originPoints[i] + 
                Vector3.up * (int)Random.Range(rooms.positionRangeY.x, rooms.positionRangeY.y), 
                Quaternion.Euler(0, 0, 0));
            room.transform.localScale = roomSizes[i];

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

    private Vector3 GetCircumsphereOrigin(
        double ax, double ay, double az, 
        double bx, double by, double bz, 
        double cx, double cy, double cz, 
        double dx, double dy, double dz)
    {
        double denominator;

        double bax = bx - ax;
        double bay = by - ay;
        double baz = bz - az;
        double cax = cx - ax;
        double cay = cy - ay;
        double caz = cz - az;
        double dax = dx - ax;
        double day = dy - ay;
        double daz = dz - az;


        // Square the lengths of each edge
        double lenBA = bax * bax + bay * bay + baz * baz;
        double lenCA = cax * cax + cay * cay + caz * caz;
        double lenDA = dax * dax + day * day + daz * daz;

        // Cross the products of the edges

        // B cross C

        double crossBCX = (bay * caz - cay * baz);
        double crossBCY = (baz * cax - caz * bax);
        double crossBCZ = (bax * cay - cax * bay);

        // C cross D

        double crossCDX = (cay * daz - day * caz);
        double crossCDY = (caz * dax - daz * cax);
        double crossCDZ = (cax * day - dax * cay);

        // D cross B

        double crossDBX = (day * baz - bay * daz);
        double crossDBY = (daz * bax - baz * dax);
        double crossDBZ = (dax * bay - bax * day);

        denominator = 0.5f / (bax * crossCDX + bay * crossCDY + baz * crossCDZ);

        // Calculate offset of circumcenter from a
        Vector3 circumcenter = new
            ((float)((lenBA * crossCDX + lenCA * crossDBX + lenDA * crossBCX) * denominator + ax),
            (float)((lenBA * crossCDY + lenCA * crossDBY + lenDA * crossBCY) * denominator + ay),
            (float)((lenBA * crossCDZ + lenCA * crossDBZ + lenDA * crossBCZ) * denominator + az));

        return circumcenter;
    }
}
