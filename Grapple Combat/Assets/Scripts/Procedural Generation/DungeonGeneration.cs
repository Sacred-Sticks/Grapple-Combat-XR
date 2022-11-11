using System.Collections.Generic;
using UnityEngine;

public class Branch
{
    public Node u;
    public Node v;
    public float weight;

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

    public List<Node> endPoints = new();

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
    public Node(Vector3 position, float unitSize)
    {
        this.Position = position;
        InitializeNeighbors(unitSize);
    }

    private void InitializeNeighbors(float unitSize)
    {
        neighbors = new();
        Vector3 neighbor;

        neighbor = this.Position + Vector3.forward * unitSize;
        neighbors.Add(neighbor);
        neighbor = this.Position + Vector3.right * unitSize;
        neighbors.Add(neighbor);
        neighbor = this.Position - Vector3.forward * unitSize;
        neighbors.Add(neighbor);
        neighbor = this.Position - Vector3.right * unitSize;
        neighbors.Add(neighbor);
        neighbor = this.Position + Vector3.up * unitSize;
        neighbors.Add(neighbor);
        neighbor = this.Position - Vector3.up * unitSize;
        neighbors.Add(neighbor);
    }
    public List<Vector3> neighbors;
}

public class DungeonGeneration : MonoBehaviour
{
    public List<Node> Nodes { get; private set; } = new();
    public List<Branch> Branches { get; private set; } = new();
    public List<Node> AllPathways { get; private set; } = new();

    [SerializeField] private GameObject player;

    private RoomGenerator roomGenerator;
    private Delauney3D delauney;
    private MinSpanTree minSpanTree;
    private Grid3D grid3D;
    private Builder builder;

    private void Awake()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        delauney = GetComponent<Delauney3D>();
        minSpanTree = GetComponent<MinSpanTree>();
        grid3D = GetComponent<Grid3D>();
        builder = GetComponent<Builder>();
    }

    private void Start()
    {
        roomGenerator.GenerateRoomData();
        Nodes = roomGenerator.Nodes;
        Vector2Int xSize = new(), ySize = new(), zSize = new();
        roomGenerator.GetGridCoordinates(ref xSize, ref ySize, ref zSize);
        Destroy(roomGenerator);

        delauney.BowyerWatson(Nodes);
        Branches = delauney.Edges;
        Destroy(delauney);

        minSpanTree.GetTree(Nodes, Branches);
        Branches = minSpanTree.MinTreeBranches;
        Nodes = minSpanTree.AllNodes;
        Destroy(minSpanTree);

        grid3D.RunPathfinding(Branches, Nodes);
        AllPathways = grid3D.AllPathways;
        Destroy(grid3D);

        builder.BuildDungeon(Nodes, AllPathways);
        Destroy(builder);

        player.transform.position = Nodes[Random.Range(0, Nodes.Count - 1)].Position;
    }
}
