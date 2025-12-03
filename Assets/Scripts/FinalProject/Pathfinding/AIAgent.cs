using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SimplePathfinding pathfinding;
    [SerializeField] Transform target; // Target yang akan dikejar (bisa player)
    
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float waypointReachDistance = 0.5f;
    [SerializeField] float recalculatePathInterval = 1f;
    
    [Header("Visualization")]
    [SerializeField] bool visualizePath = true;
    [SerializeField] Color pathColor = Color.cyan;
    
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float lastPathCalculationTime;
    private bool isMoving = false;
    
    void Start()
    {
        if (pathfinding == null)
        {
            pathfinding = FindObjectOfType<SimplePathfinding>();
        }
        
        if (target != null)
        {
            CalculateNewPath();
        }
    }
    
    void Update()
    {
        if (target == null || pathfinding == null) return;
        
        // Recalculate path secara periodik
        if (Time.time - lastPathCalculationTime > recalculatePathInterval)
        {
            CalculateNewPath();
        }
        
        // Follow path
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
    }
    
    void CalculateNewPath()
    {
        currentPath = pathfinding.CalculatePath(transform.position, target.position);
        currentWaypointIndex = 0;
        lastPathCalculationTime = Time.time;
        isMoving = currentPath != null && currentPath.Count > 0;
        
        if (!isMoving)
        {
            Debug.LogWarning("AI: No valid path found!");
        }
    }
    
    void FollowPath()
    {
        if (currentWaypointIndex >= currentPath.Count)
        {
            isMoving = false;
            return;
        }
        
        Vector3 targetWaypoint = currentPath[currentWaypointIndex];
        Vector3 direction = targetWaypoint - transform.position;
        direction.y = 0; // Jaga movement di plane horizontal
        
        // Cek apakah sudah sampai di waypoint
        if (direction.magnitude < waypointReachDistance)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= currentPath.Count)
            {
                isMoving = false;
                return;
            }
            
            targetWaypoint = currentPath[currentWaypointIndex];
            direction = targetWaypoint - transform.position;
            direction.y = 0;
        }
        
        // Rotate menuju waypoint
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Move menuju waypoint
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }
    
    // Method publik untuk set target baru
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        CalculateNewPath();
    }
    
    // Method untuk memaksa recalculate path
    public void ForceRecalculatePath()
    {
        CalculateNewPath();
    }
    
    // Visualisasi path di Scene view
    void OnDrawGizmos()
    {
        if (!visualizePath || currentPath == null || currentPath.Count == 0) return;
        
        // Gambar garis path
        Gizmos.color = pathColor;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
        
        // Gambar sphere di setiap waypoint
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (i == currentWaypointIndex)
            {
                Gizmos.color = Color.red; // Waypoint saat ini
                Gizmos.DrawSphere(currentPath[i], 0.3f);
            }
            else
            {
                Gizmos.color = pathColor;
                Gizmos.DrawSphere(currentPath[i], 0.2f);
            }
        }
        
        // Gambar garis dari AI ke waypoint berikutnya
        if (currentWaypointIndex < currentPath.Count)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentPath[currentWaypointIndex]);
        }
    }
}