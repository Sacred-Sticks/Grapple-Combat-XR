using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch
{
    public Node u;
    public Node v;
    public float weight;

    public Branch()
    {

    }

    public Branch(Node u, Node v)
    {
        this.u = u;
        this.v = v;
        weight = Vector3.Distance(u.Position, v.Position);
    }

    public static bool CompareEdges(Branch left, Branch right)
    {
        return (left.u.Position == right.u.Position || left.u.Position == right.v.Position) &&
            (left.v.Position == right.u.Position || left.v.Position == right.v.Position);
    }
}

public class Node
{
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public List<Branch> Branches { get; private set; } = new();
    public HashSet<Vector3> Destinations { get; private set; } = new();

    public Node()
    {

    }

    public Node(Vector3 position)
    {
        this.Position = position;
    }

    public Node(Vector3 position, Vector3 size)
    {
        this.Position = position;
        this.Size = size;
    }

    public void AddBranch(Branch branch)
    {
        Branches.Add(branch);
    }
}

public class DungeonGeneration : MonoBehaviour
{
    public List<Node> Nodes { get; private set; } = new();
    public List<Branch> Branches { get; private set; } = new();

    private RoomGenerator roomGenerator;
    private Delauney3D delauney;
    private MinSpanTree minSpanTree;
    private Grid3D grid3D;

    private void Awake()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        delauney = GetComponent<Delauney3D>();
        minSpanTree = GetComponent<MinSpanTree>();
        grid3D = GetComponent<Grid3D>();
    }

    private void Start()
    {
        roomGenerator.GenerateRoomData();
        Nodes = roomGenerator.Nodes;
        Vector2Int xSize = new(), ySize = new(), zSize = new();
        roomGenerator.GetGridCoordinates(ref xSize, ref ySize, ref zSize);
        //Destroy(roomGenerator);

        delauney.BowyerWatson(Nodes);
        Branches = delauney.Edges;
        //Destroy(delauney);

        minSpanTree.GetTree(Nodes, Branches);
        Branches = minSpanTree.MinTreeBranches;
        Nodes = minSpanTree.AllNodes;
        //Destroy(minSpanTree);
    }

    private void OnDrawGizmos()
    {
        if (Nodes == null) return;
        
        Gizmos.color = Color.red;
        for (int i = 0; i < Nodes.Count; i++)
        {
            Gizmos.DrawCube(Nodes[i].Position, Nodes[i].Size);
        }

        if (Branches == null) return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < Branches.Count; i++)
        {
            Gizmos.DrawLine(Branches[i].u.Position, Branches[i].v.Position);
        }
    }
}
