using UnityEngine;

public class GridTile : MonoBehaviour
{
    public bool isWalkable = true;
    public Vector2Int gridPosition;
    public int gCost, hCost;
    public int fCost => gCost + hCost;
    public GridTile parent;

    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}