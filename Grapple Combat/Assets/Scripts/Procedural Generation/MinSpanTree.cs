using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MinSpanTree : MonoBehaviour
{
    [SerializeField] private float branchWeightRatio;
    [SerializeField] private float branchPercentage;
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
    public List<Node> BadNodes { get; private set; } = new();
    public List<Branch> AllBranches { get; private set; } = new();
    public List<Branch> MinTreeBranches { get; private set; } = new();
    public List<Branch> NonTreeBranches { get; private set; } = new();

    public void GetTree(List<Node> Nodes, List<Branch> branches)
    {
        AssignNodes(Nodes, branches);
        CreateMinimumSpanningTree();
        RemoveUnconnectedNodes();
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
                        AllBranches.Add(branch);
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

        foreach (Branch branch in AllBranches)
        {
            //NonTreeBranches.Add(branch);
        }

        //while (MinTreeBranches.Count < AllNodes.Count - 1)
        for (int i = 0; i < AllNodes.Count; i++) 
        {
            foreach (Branch branch in AllBranches)
            {
                if (!NonTreeBranches.Contains(branch))
                {
                    if (!uf.Connected(branch.u, branch.v)) {
                        if (branch.u.Position != branch.v.Position) {
                            uf.MakeUnion(branch.u, branch.v);
                            MinTreeBranches.Add(branch);
                            break;
                        }
                    } else
                    {
                        // When it forms a cycle, add to NonTreeBranches to avoid rechecking it
                        NonTreeBranches.Add(branch);
                        if (branch.weight > maxNonTreeWeight) maxNonTreeWeight = branch.weight;
                    }
                }
            }
        }

        AddExtraBranches(maxNonTreeWeight);
    }

    private void SortAllBranches()
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

    private void AddExtraBranches(float maxWeight)
    {
        foreach (Branch branch in NonTreeBranches)
        {
            if ((branch.weight / maxWeight) < branchWeightRatio)
            {
                bool canAdd = true;
                foreach (Branch treeBranch in MinTreeBranches)
                {
                    if (Branch.CompareEdges(branch, treeBranch)) canAdd = false;
                }
                if (Random.Range(0.0f, 1.0f) < branchPercentage)
                    if (canAdd) 
                        MinTreeBranches.Add(branch);
            } else
            {
                break;
            }
        }
    }

    private void RemoveUnconnectedNodes()
    {
        foreach (Node node in AllNodes)
        {
            BadNodes.Add(node);
        }

        foreach (Branch branch in MinTreeBranches)
        {
            if (BadNodes.Contains(branch.u))
            {
                BadNodes.Remove(branch.u);
            }
            if (BadNodes.Contains(branch.v))
            {
                BadNodes.Remove(branch.v);
            }
        }

        foreach (Node node in BadNodes)
        {
            AllNodes.Remove(node);
        }
    }
}
