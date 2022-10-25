using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid3D : MonoBehaviour
{
    public class GridUnit
    {
        public int unitSize;
        public bool tempOccupied = false;
        public bool isOccupied = false;
        public Vector3 position;

        public GridUnit()
        {

        }

        public GridUnit(int unitSize)
        {
            this.unitSize = unitSize;
        }
    }

    [SerializeField] private int unitSize;

    public List<List<List<GridUnit>>> Grid { get; private set; }
    public Vector2Int[] GridCoordinates { get; private set; } = new Vector2Int[3];
    public Vector3Int GridSize { get; private set; } = new();
    public Vector3Int NumUnits { get; private set; } = new();

    public static readonly Vector3Int[] neighbors =
    {
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        
        new Vector3Int(0, 1, 2),
        new Vector3Int(0, 1, -2),
        new Vector3Int(2, 1, 0),
        new Vector3Int(-2, 1, 0),
        new Vector3Int(0, -1, 2),
        new Vector3Int(0, -1, -2),
        new Vector3Int(2, -1, 0),
        new Vector3Int(-2, -1, 0),

    };

    public void SetGridCoordinates(Vector2Int x, Vector2Int y, Vector2Int z)
    {
        //numUnits = totalSize / unitSize;
        GridCoordinates[0] = x;
        GridCoordinates[1] = y;
        GridCoordinates[2] = z;

        GridSize = new(
            GridCoordinates[0].y - GridCoordinates[0].x, 
            GridCoordinates[1].y - GridCoordinates[1].x, 
            GridCoordinates[2].y - GridCoordinates[2].x);

        NumUnits = GridSize / unitSize;
    }

    public void InitializeGrid()
    {
        Grid = new();

        for (int i = 0; i < GridSize.x; i++)
        {
            Grid[i] = new();
            for (int j = 0; j < GridSize.y; j++)
            {
                Grid[i][j] = new();
                for (int k = 0; k < GridSize.z; k++)
                {
                    Grid[i][j][k] = new(unitSize);
                }
            }
        }
    }

    public void ConnectNodes(Node left, Node right)
    {
        
    }

    public int GetUnitSize()
    {
        return unitSize;
    }
}
