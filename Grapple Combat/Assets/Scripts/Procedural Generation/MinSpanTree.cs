using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MinSpanTree : MonoBehaviour
{
    public class UnionFind
    {
        public List<Node> AllNodes { get; private set; } = new();
        public Dictionary<Node, int> id { get; private set; } = new();
        public Dictionary<int, int> sizes = new();

        public UnionFind(List<Node> nodes)
        {
            int newestGroup = 0;
            foreach (Node node in nodes)
            {
                AllNodes.Add(node);
                id.Add(node, newestGroup);
                sizes.Add(newestGroup, 1);
                newestGroup++;
            }
        }

        public bool Connected(Node left, Node right)
        {
            return id[left] == id[right];
        }

        public void MakeUnion(Node left, Node right)
        {
            if (Connected(left, right)) return;

            int leftID = id[left];
            int rightID = id[right];
            int leftSize = sizes[leftID];
            int rightSize = sizes[rightID];

            foreach (Node node in AllNodes)
            {
                if (leftSize > rightSize)
                {
                    if (id[node] == rightID)
                    {
                        id[node] = leftID;
                    }
                } else
                {
                    if (id[node] == leftID)
                    {
                        id[node] = rightID;
                    }
                }
            }
        }
    }


    public List<Node> AllNodes { get; private set; } = new();
    public List<Node> OpenNodes { get; private set; } = new();
    public List<Node> ConnectedNodes { get; private set; } = new();
    public List<Branch> AllBranches { get; private set; } = new();
    public List<Branch> MinTreeBranches { get; private set; } = new();
    public List<Branch> NonTreeBranches { get; private set; } = new();

    public void GetTree(List<Node> Nodes, List<Branch> branches)
    {
        AssignNodes(Nodes, branches);
        CreateMinimumSpanningTree();
    }

    private void AssignNodes(List<Node> Nodes, List<Branch> branches)
    {
        foreach (Node node in Nodes)
        {
            AllNodes.Add(node);
        } // Adds vertices to nodes

        foreach (Node n in AllNodes) // Loop through each node
        {
            foreach(Branch branch in branches) { // Loop throuh each branch
                if (branch.u.Position != branch.v.Position)
                {
                    if (n.Position == branch.u.Position) // Check that the branch starts at the node
                    {
                        if (!n.Destinations.Contains(branch.v.Position)) // Check that the node doesn't already contain the branch
                        {
                            n.Destinations.Add(branch.v.Position);
                            n.Branches.Add(branch);
                            AllBranches.Add(branch);
                        }
                    }
                }
            }
        } // Adds branches to nodes
    }

    private void CreateMinimumSpanningTree()
    {
        SortAllBranches();

        UnionFind uf = new(AllNodes);
        float maxNonTreeWeight = -Mathf.Infinity;

        //while (MinTreeBranches.Count < AllNodes.Count - 1)
        for (int i = 0; i < AllNodes.Count; i++)
        {
            foreach (Branch branch in AllBranches)
            {
                if (!NonTreeBranches.Contains(branch))
                {
                    if (!uf.Connected(branch.u, branch.v)) {
                        uf.MakeUnion(branch.u, branch.v);
                        MinTreeBranches.Add(branch);
                        break;
                    }

                    // When it forms a cycle, add to NonTreeBranches to avoid rechecking it
                    NonTreeBranches.Add(branch);
                    if (branch.weight > maxNonTreeWeight) maxNonTreeWeight = branch.weight;
                }
            }
        }

        AddExtraBranches(maxNonTreeWeight);
    }

    private void AddExtraBranches(float maxWeight)
    {
        foreach (Branch branch in NonTreeBranches)
        {
            if ((branch.weight / maxWeight) < .25)
            {
                MinTreeBranches.Add(branch);
            }
        }
    }

    public void SortAllBranches()
    {
        for (var i = 0; i < AllBranches.Count; i++)
        {
            var min = i;
            for (var j = i + 1; j < AllBranches.Count; j++)
            {
                if (AllBranches[min].weight > AllBranches[j].weight)
                {
                    min = j;
                }
            }

            if (min != i)
            {
                var lowerValue = AllBranches[min];
                AllBranches[min] = AllBranches[i];
                AllBranches[i] = lowerValue;
            }
        }
    }
}
