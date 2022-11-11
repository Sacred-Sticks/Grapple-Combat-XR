using PlasticGui.Gluon.WorkspaceWindow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField] private TileData roomTiles;
    [SerializeField] private TileData hallTiles;

    [System.Serializable]
    private struct TileData
    {
        public List<GameObject> wallTiles;
        public List<GameObject> floorTiles;
        public List<GameObject> ceilingTiles;
    }

    private float unitSize;

    public List<Vector3> YTilePos { get; private set; } = new();
    public List<GameObject> WallTiles { get; private set; } = new();

    private void Awake()
    {
        unitSize = GetComponent<Grid3D>().GetUnitSize();
    }

    public void BuildDungeon(List<Node> allRooms, List<Node> allPathways)
    {
        GameObject AllRooms = new("All Rooms");
        foreach (var room in allRooms)
        {
            GameObject obj = new("Room");
            obj.transform.parent = AllRooms.transform;
            BuildRoom(room, obj);
        }
        GameObject AllHalls = new("All Hallways");
        foreach (var segment in allPathways)
        {
            Node path = new(segment.Position + Vector3.one * 0.5f, unitSize);
            bool buildSegment = true;
            foreach (var room in allRooms)
            {
                if (CheckWithinRoom(room, path))
                {
                    buildSegment = false;
                }
            }
            if (buildSegment)
            {
                GameObject obj = new("Segment");
                obj.transform.parent = AllHalls.transform;
                BuildHallSegment(path, obj);
            }
        }

        RemoveDoubles(WallTiles);
    }

    private void BuildRoom(Node room, GameObject obj)
    {
        XPlaneRoom(room, obj);
        YPlaneRoom(room, obj);
        ZPlaneRoom(room, obj);
    }
    private void BuildHallSegment(Node segment, GameObject obj)
    {
        SpawnHallwayPlanes(segment, obj, Vector3.right, hallTiles.wallTiles, hallTiles.wallTiles);
        SpawnHallwayPlanes(segment, obj, Vector3.up, hallTiles.floorTiles, hallTiles.ceilingTiles);
        SpawnHallwayPlanes(segment, obj, Vector3.forward, hallTiles.wallTiles, hallTiles.wallTiles);
    }

    private void XPlaneRoom(Node room, GameObject parent)
    {
        Vector3 center = room.Position;
        Vector3 roomSize = room.Size;
        Vector3 numTiles = roomSize / unitSize;
        Vector3 planeCenter;
        Vector3 planeSize;
        planeCenter = center - Vector3.right * roomSize.x / 2;
        planeSize = roomSize - Vector3.right * roomSize.x;
        Vector3 planeCorner;
        planeCorner = planeCenter - Vector3.up * (roomSize.y / 2 - unitSize / 2) - Vector3.forward * (roomSize.z / 2 - unitSize / 2);
        List<GameObject> tiles = roomTiles.wallTiles;
        Vector3 currentPos;
        for (int i = 0; i < numTiles.y; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.up * unitSize * i;
            for (int j = 0; j < numTiles.z; j++)
            {
                WallTiles.Add(Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 0, 0), parent.transform));
                currentPos += Vector3.forward * unitSize;
            }
        }
        planeCorner += Vector3.right * roomSize.x;
        for (int i = 0; i < numTiles.y; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.up * unitSize * i;
            for (int j = 0; j < numTiles.z; j++)
            {
                WallTiles.Add(Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 0, 0), parent.transform));
                currentPos += Vector3.forward * unitSize;
            }
        }
    }
    private void YPlaneRoom(Node room, GameObject parent)
    {
        Vector3 center = room.Position;
        Vector3 roomSize = room.Size;
        Vector3 numTiles = roomSize / unitSize;
        Vector3 planeCenter;
        Vector3 planeSize;
        planeCenter = center - Vector3.up * roomSize.y / 2;
        planeSize = roomSize - Vector3.up * roomSize.y;
        Vector3 planeCorner;
        planeCorner = planeCenter - Vector3.right * (roomSize.x / 2 - unitSize / 2) - Vector3.forward * (roomSize.z / 2 - unitSize / 2);
        List<GameObject> tiles = roomTiles.floorTiles;
        Vector3 currentPos;
        for (int i = 0; i < numTiles.x; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.right * unitSize * i;
            for (int j = 0; j < numTiles.z; j++)
            {
                if (!YTilePos.Contains(currentPos))
                {
                    YTilePos.Add(currentPos);
                    Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 0, 0), parent.transform);
                }
                currentPos += Vector3.forward * unitSize;
            }
        }
        planeCorner += Vector3.up * roomSize.y;
        tiles = roomTiles.ceilingTiles;
        for (int i = 0; i < numTiles.x; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.right * unitSize * i;
            for (int j = 0; j < numTiles.z; j++)
            {
                if (!YTilePos.Contains(currentPos))
                {
                    YTilePos.Add(currentPos);
                    Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 0, 0), parent.transform);
                }
                currentPos += Vector3.forward * unitSize;
            }
        }
    }
    private void ZPlaneRoom(Node room, GameObject parent)
    {
        Vector3 center = room.Position;
        Vector3 roomSize = room.Size;
        Vector3 numTiles = roomSize / unitSize;
        Vector3 planeCenter;
        Vector3 planeSize;
        planeCenter = center - Vector3.forward * roomSize.z / 2;
        planeSize = roomSize - Vector3.forward * roomSize.z;
        Vector3 planeCorner;
        planeCorner = planeCenter - Vector3.right * (roomSize.x / 2 - unitSize / 2) - Vector3.up * (roomSize.y / 2 - unitSize / 2);
        List<GameObject> tiles = roomTiles.wallTiles;
        Vector3 currentPos;
        for (int i = 0; i < numTiles.x; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.right * unitSize * i;
            for (int j = 0; j < numTiles.y; j++)
            {
                WallTiles.Add(Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 90, 0), parent.transform));
                currentPos += Vector3.up * unitSize;
            }
        }
        planeCorner += Vector3.forward * roomSize.z;
        for (int i = 0; i < numTiles.x; i++)
        {
            currentPos = planeCorner;
            currentPos += Vector3.right * unitSize * i;
            for (int j = 0; j < numTiles.y; j++)
            {
                WallTiles.Add(Instantiate(tiles[Random.Range(0, tiles.Count - 1)], currentPos, Quaternion.Euler(0, 90, 0), parent.transform));
                currentPos += Vector3.up * unitSize;
            }
        }

    }

    private void SpawnHallwayPlanes(Node room, GameObject parent, Vector3 plane, List<GameObject> tiles1, List<GameObject> tiles2)
    {
        Vector3 center = new() { x = room.Position.x + 2, y = room.Position.y - 0.5f, z = room.Position.z + 2 };
        Vector3 spawnPos;
        spawnPos = center + plane * unitSize / 2;
        Vector2 spawnRot = new();
        if (plane.z != 0) spawnRot.y = 90;
        GameObject obj;

        obj = Instantiate(tiles2[Random.Range(0, tiles1.Count - 1)], spawnPos, Quaternion.Euler(spawnRot), parent.transform);
        if (plane == Vector3.up)
        {
            if (!YTilePos.Contains(spawnPos))
            {
                YTilePos.Add(spawnPos);
            }
        } 
        else
        {
            WallTiles.Add(obj);
        }

        spawnPos += -plane * unitSize;
        obj = Instantiate(tiles1[Random.Range(0, tiles1.Count - 1)], spawnPos, Quaternion.Euler(spawnRot), parent.transform);
        if (plane == Vector3.up)
        {
            if (!YTilePos.Contains(spawnPos))
            {
                YTilePos.Add(spawnPos);
            }
        }
        else
        {
            WallTiles.Add(obj);
        }
    }

    private bool CheckWithinRoom(Node room, Node segment)
    {
        Vector3 roomCenter = room.Position;
        Vector3 roomSize = room.Size;
        Vector3 minCorner = roomCenter - roomSize / 2;
        Vector3 maxCorner = roomCenter + roomSize / 2;
        Vector3 segmentPos = segment.Position;

        if (minCorner.x < segmentPos.x && segmentPos.x < maxCorner.x)
        {
            if (minCorner.y < segmentPos.y && segmentPos.y < maxCorner.y)
            {
                if (minCorner.z < segmentPos.z && segmentPos.z < maxCorner.z)
                {
                    return true;
                }
            }
        }


        return false;
    }

    private bool WithinProximaty(Vector3 left, Vector3 right)
    {
        if (Mathf.Abs(left.x - right.x) < 0.01f)
        {
            if (Mathf.Abs(left.y - right.y) < 0.01f)
            {
                if (Mathf.Abs(left.z - right.z) < 0.01f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void RemoveDoubles(List<GameObject> objects) 
    {
        for (int i = 0; i < objects.Count; i++)
        {
            for (int j = i + 1; j < objects.Count; j++)
            {
                if (WithinProximaty(objects[i].transform.position, objects[j].transform.position))
                {
                    Destroy(objects[i]);
                    Destroy(objects[j]);
                }
            }
        }
    }
}
