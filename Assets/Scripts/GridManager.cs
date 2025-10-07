using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;
    public GameObject tilePrefab;

    private GridTile[,] grid;

    void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new GridTile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                GridTile tile = tileObj.GetComponent<GridTile>();
                tile.gridPosition = new Vector2Int(x, y);
                grid[x, y] = tile;
            }
        }
    }

    public GridTile GetTileAt(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            return grid[pos.x, pos.y];
        return null;
    }

    public List<GridTile> GetNeighbors(GridTile tile)
    {
        List<GridTile> neighbors = new List<GridTile>();
        Vector2Int pos = tile.gridPosition;

        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // atas
            new Vector2Int(1, 0),   // kanan
            new Vector2Int(0, -1),  // bawah
            new Vector2Int(-1, 0)   // kiri
        };

        foreach (var dir in directions)
        {
            GridTile neighbor = GetTileAt(pos + dir);
            if (neighbor != null && neighbor.isWalkable)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }
}