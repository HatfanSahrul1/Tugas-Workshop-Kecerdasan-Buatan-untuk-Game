using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControllerAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Pathfinder pathfinder;
    private Vector2Int currentTarget;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        pathfinder = GetComponent<Pathfinder>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null && hit.collider.CompareTag("Tile"))
            {
                GridTile clickedTile = hit.collider.GetComponent<GridTile>();
                if (clickedTile.isWalkable)
                {
                    Vector2Int playerGridPos = WorldToGrid(transform.position);
                    Vector2Int targetGridPos = clickedTile.gridPosition;

                    List<Vector2Int> path = pathfinder.FindPath(playerGridPos, targetGridPos);
                    if (path.Count > 0)
                    {
                        StartCoroutine(MoveAlongPath(path));
                    }
                }
            }
        }
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float tileSize = GridManager.Instance.tileSize;
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.y / tileSize);
        return new Vector2Int(x, y);
    }

    IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        foreach (Vector2Int gridPos in path)
        {
            if (gridPos == WorldToGrid(transform.position)) continue; // skip posisi awal

            Vector3 worldPos = new Vector3(gridPos.x * GridManager.Instance.tileSize,
                                           gridPos.y * GridManager.Instance.tileSize,
                                           transform.position.z);
            while (Vector3.Distance(transform.position, worldPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, worldPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = worldPos;
        }
    }
}