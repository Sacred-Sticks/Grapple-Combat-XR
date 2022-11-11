using PlasticGui.Gluon.WorkspaceWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Grid3D : MonoBehaviour
{
    #region DataSet
    public Vector3 GridSize { get; private set; } = new();
    public Vector3 NumUnits { get; private set; } = new();
    public Vector3Int minCorner { get; private set; } = new();
    public Vector3Int maxCorner { get; private set; } = new();
    #endregion

    [SerializeField] private int unitSize;

    public List<Node> AllPathways { get; private set; } = new();
    public List<Vector3> hallwayCenters { get; private set; } = new();

    public void RunPathfinding(List<Branch> branches, List<Node> nodes)
    {
        int count = 0;
        foreach (Branch branch in branches)
        {
            count++;
            Vector3 startPos = (branch.u.Position - Vector3.up * branch.u.Size.y / 2 + Vector3.up * unitSize / 2);
            Vector3 endPos = (branch.v.Position - Vector3.up * branch.v.Size.y / 2 + Vector3.up * unitSize / 2);
            Node start = new(startPos, unitSize);
            Node end = new(endPos, unitSize);
            AllPathways.AddRange(A_Star(start, end));
        }
    }

    public void SetGridCoordinates(Vector2 unitsX, Vector2 unitsY, Vector2 unitsZ, List<Node> nodes)
    {
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;
        foreach (Node node in nodes)
        {
            float xPoint = node.Position.x;
            float yPoint = node.Position.y;
            float zPoint = node.Position.z;
            if (xPoint - node.Size.x / 2 < minX) minX = xPoint - node.Size.x / 2;
            if (xPoint + node.Size.x / 2 > maxX) maxX = xPoint + node.Size.x / 2;
            if (yPoint - node.Size.y / 2 < minY) minY = yPoint - node.Size.y / 2;
            if (yPoint + node.Size.y / 2 > maxY) maxY = yPoint + node.Size.y / 2;
            if (zPoint - node.Size.z / 2 < minZ) minZ = zPoint - node.Size.z / 2;
            if (zPoint + node.Size.z / 2 > maxZ) maxZ = zPoint + node.Size.z / 2;
        }

        //numUnits = totalSize / unitSize;
        NumUnits = new()
        {
            x = Mathf.Max(unitsX.y - unitsX.x, 1),
            y = Mathf.Max(unitsY.y - unitsX.x, 1),
            z = Mathf.Max(unitsZ.y - unitsZ.x, 1)
        };

        GridSize = new()
        {
            x = NumUnits.x * unitSize,
            y = NumUnits.y * unitSize,
            z = NumUnits.z * unitSize
        };

        minCorner = new()
        {
            x = (int)minX,
            y = (int)minY,
            z = (int)minZ
        };
        minCorner /= unitSize;
        minCorner = Vector3Int.FloorToInt(minCorner);

        maxCorner = new()
        {
            x = (int)maxX,
            y = (int)maxY,
            z = (int)maxZ
        };
        maxCorner /= unitSize;
        maxCorner = Vector3Int.FloorToInt(maxCorner);

    }

    private List<Node> A_Star(Node start, Node end)
    {
        List<Node> openSet = new();
        List<Vector3> openPos = new();
        openSet.Add(start);
        openPos.Add(start.Position);
        
        Dictionary<Node, Node> cameFrom = new();
        Dictionary<Node, float> gScore = new();
        gScore[start] = 0;

        Dictionary<Node, float> fScore = new();
        fScore[start] = H(start, end);

        Node current;

        while (openSet.Count > 0)
        {
            current = GetLowestValueNode(openSet, fScore);
            
            if (current.Position == end.Position)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            openPos.Remove(current.Position);
            foreach (Vector3 neighborPos in current.neighbors)
            {
                Node neighbor = new(neighborPos, unitSize);
                float currentG = gScore[current];
                float dist = Distance(neighbor.Position, current.Position);
                float tentative_gScore = currentG + dist;

                // Add neighbor to openSet if it isn't there and set values appropriately
                
                if (gScore.ContainsKey(neighbor))
                {
                    if (tentative_gScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = gScore[neighbor] + H(neighbor, end);
                        if (!openPos.Contains(neighborPos))
                        {
                            openSet.Add(neighbor);
                            openPos.Add(neighborPos);
                        }
                    }
                } 
                else
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + H(neighbor, end);
                    if (!openPos.Contains(neighborPos))
                    {
                        openSet.Add(neighbor);
                        openPos.Add(neighborPos);
                    }
                }
            }
        }
        return new();
    }

    private float Distance(Vector3 neighborPos, Vector3 position)
    {
        return Vector3.Distance(position, neighborPos);
    }


    private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> path = new();
        if (!hallwayCenters.Contains(current.Position))
        {
            path.Add(current);
            hallwayCenters.Add(current.Position);
        }
        while (cameFrom.Keys.Contains(current))
        {
            current = cameFrom[current];
            if (!hallwayCenters.Contains(current.Position))
            {
                path.Add(current);
                hallwayCenters.Add(current.Position);
            }
        }
        return path;
    }

    private Node GetLowestValueNode(List<Node> openSet, Dictionary<Node, float> fScore)
    {
        if (openSet.Count == 1) return openSet[0];
        int lowest = 0;
        for (int i = 1; i < openSet.Count; i++)
        {
            if (fScore[openSet[i]] < fScore[openSet[lowest]])
            {
                lowest = i;
            }
        }
        return openSet[lowest];
    }

    public static float H(Node current, Node end)
    {
        Vector3 diff = current.Position - end.Position;
        diff = new(MathF.Abs(diff.x), Mathf.Abs(diff.y), Mathf.Abs(diff.z));
        return Mathf.Abs(diff.x + diff.y + diff.z);
    }

    public int GetUnitSize()
    {
        return unitSize;
    }
}
