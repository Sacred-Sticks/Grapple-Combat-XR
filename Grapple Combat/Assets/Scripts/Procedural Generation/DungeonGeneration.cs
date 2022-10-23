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
    public List<Branch> Branches { get; private set; } = new();
    public HashSet<Vector3> Destinations { get; private set; } = new();

    public Node()
    {

    }

    public Node(Vector3 position)
    {
        this.Position = position;
    }

    public void AddBranch(Branch branch)
    {
        Branches.Add(branch);
    }
}

public class DungeonGeneration : MonoBehaviour
{
    public List<Node> Nodes { get; private set; } = new();
    public List<Vector3> RoomSizes { get; private set; } = new();
    public List<Branch> Branches { get; private set; } = new();

    private RoomGenerator roomGenerator;
    private Delauney3D delauney;
    private MinSpanTree minSpanTree;

    private void Awake()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        delauney = GetComponent<Delauney3D>();
        minSpanTree = GetComponent<MinSpanTree>();
    }

    private void Start()
    {
        roomGenerator.GenerateRoomData();
        Nodes = roomGenerator.roomOrigins;
        RoomSizes = roomGenerator.roomSizes;

        delauney.BowyerWatson(Nodes);
        Branches = delauney.Edges;

        minSpanTree.GetTree(Nodes, Branches);
        Branches = minSpanTree.MinTreeBranches;
    }

    private void OnDrawGizmosSelected()
    {
        if (Nodes == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < Nodes.Count; i++)
        {
            Gizmos.DrawCube(Nodes[i].Position, RoomSizes[i]);
        }

        Gizmos.color = Color.blue;
        foreach (Branch e in Branches)
        {
            Gizmos.DrawLine(e.u.Position, e.v.Position);
        }
    }
}
