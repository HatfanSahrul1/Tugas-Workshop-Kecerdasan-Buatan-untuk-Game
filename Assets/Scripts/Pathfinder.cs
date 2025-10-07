using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        GridTile startTile = GridManager.Instance.GetTileAt(start);
        GridTile targetTile = GridManager.Instance.GetTileAt(target);

        if (startTile == null || targetTile == null || !targetTile.isWalkable)
            return new List<Vector2Int>();

        // Reset semua tile
        ResetTiles();

        List<GridTile> openSet = new List<GridTile> { startTile };
        HashSet<GridTile> closedSet = new HashSet<GridTile>();

        startTile.gCost = 0;
        startTile.hCost = CalculateHeuristic(startTile.gridPosition, target);

        while (openSet.Count > 0)
        {
            GridTile current = openSet.OrderBy(tile => tile.fCost)
                                      .ThenBy(tile => tile.hCost)
                                      .First();

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetTile)
            {
                return ReconstructPath(current);
            }

            foreach (GridTile neighbor in GridManager.Instance.GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int tentativeGCost = current.gCost + 1;

                if (tentativeGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.parent = current;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = CalculateHeuristic(neighbor.gridPosition, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Vector2Int>(); // Tidak ada jalur
    }

    int CalculateHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    List<Vector2Int> ReconstructPath(GridTile endTile)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        GridTile current = endTile;

        while (current != null)
        {
            path.Add(current.gridPosition);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    void ResetTiles()
    {
        GridManager gridManager = GridManager.Instance;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                GridTile tile = gridManager.GetTileAt(new Vector2Int(x, y));
                tile.gCost = int.MaxValue;
                tile.parent = null;
            }
        }
    }
}