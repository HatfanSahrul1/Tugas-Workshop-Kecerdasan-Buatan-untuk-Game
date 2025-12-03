using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePathfinding : MonoBehaviour
{
    [Header("Waypoint Setup")]
    [SerializeField] Transform[] points;
    
    [Header("Wall Detection")]
    [SerializeField] string wallLayerName = "Wall";
    [SerializeField] float spherecastRadius = 0.3f;
    [SerializeField] float raycastOffset = 0.5f;
    [SerializeField] int multipleRaycastCount = 5;
    [SerializeField] QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
    
    [Header("Visualization")]
    [SerializeField] bool visualizeConnections = true;
    [SerializeField] bool showDebugRays = false;
    [SerializeField] Color validConnectionColor = Color.green;
    [SerializeField] Color invalidConnectionColor = Color.red;
    [SerializeField] Color waypointColor = Color.yellow;
    
    [Header("Debug Info")]
    [SerializeField] bool verboseLogging = true;
    
    private Dictionary<Transform, List<Transform>> graph = new Dictionary<Transform, List<Transform>>();
    private int wallLayerMask;
    private List<Collider> allWallColliders = new List<Collider>();
    
    void Awake()
    {
        SetupLayerMask();
        CacheAllWallColliders();
    }
    
    void Start()
    {
        BuildGraph();
    }
    
    void SetupLayerMask()
    {
        int layerIndex = LayerMask.NameToLayer(wallLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"❌ Layer '{wallLayerName}' tidak ditemukan! Buat layer di Project Settings > Tags and Layers");
            return;
        }
        
        wallLayerMask = 1 << layerIndex;
        
        if (verboseLogging)
        {
            Debug.Log($"✅ Layer mask setup: '{wallLayerName}' (Index: {layerIndex}, Mask: {wallLayerMask})");
        }
    }
    
    // Cache semua collider yang ada di layer Wall (termasuk child objects)
    void CacheAllWallColliders()
    {
        allWallColliders.Clear();
        
        // Cari SEMUA collider di scene
        Collider[] allColliders = FindObjectsOfType<Collider>();
        int layerIndex = LayerMask.NameToLayer(wallLayerName);
        
        if (layerIndex == -1) return;
        
        foreach (Collider col in allColliders)
        {
            // Cek apakah collider atau parent-nya ada di layer Wall
            if (IsInWallLayer(col.gameObject, layerIndex))
            {
                allWallColliders.Add(col);
            }
        }
        
        if (verboseLogging)
        {
            Debug.Log($"📦 Found {allWallColliders.Count} wall colliders (including nested children)");
            
            if (allWallColliders.Count == 0)
            {
                Debug.LogWarning("⚠️ TIDAK ADA COLLIDER ditemukan di layer Wall!");
                Debug.LogWarning("Pastikan GameObject dengan collider (atau parent-nya) ada di layer 'Wall'");
            }
        }
    }
    
    // Cek apakah GameObject atau salah satu parent-nya ada di layer tertentu
    bool IsInWallLayer(GameObject obj, int layerIndex)
    {
        Transform current = obj.transform;
        
        // Cek object itu sendiri dan semua parent-nya
        while (current != null)
        {
            if (current.gameObject.layer == layerIndex)
            {
                return true;
            }
            current = current.parent;
        }
        
        return false;
    }
    
    [ContextMenu("Rebuild Graph")]
    public void RebuildGraph()
    {
        SetupLayerMask();
        CacheAllWallColliders();
        BuildGraph();
    }
    
    void BuildGraph()
    {
        if (wallLayerMask == 0)
        {
            Debug.LogError("❌ Wall Layer Mask tidak valid!");
            return;
        }
        
        if (allWallColliders.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada wall collider! Graph mungkin tidak akurat.");
        }
        
        graph.Clear();
        int totalConnections = 0;
        int blockedConnections = 0;
        
        foreach (Transform point in points)
        {
            if (point == null) continue;
            
            graph[point] = new List<Transform>();
            
            foreach (Transform otherPoint in points)
            {
                if (point == otherPoint || otherPoint == null) continue;
                
                if (IsPathClear(point.position, otherPoint.position))
                {
                    graph[point].Add(otherPoint);
                    totalConnections++;
                }
                else
                {
                    blockedConnections++;
                }
            }
        }
        
        Debug.Log($"<color=cyan>╔══════════════════════════════════╗</color>");
        Debug.Log($"<color=cyan>║   GRAPH BUILD COMPLETE           ║</color>");
        Debug.Log($"<color=cyan>╚══════════════════════════════════╝</color>");
        Debug.Log($"📊 Nodes: {graph.Count}");
        Debug.Log($"✅ Valid connections: {totalConnections}");
        Debug.Log($"❌ Blocked connections: {blockedConnections}");
        
        // Warning untuk waypoint tanpa koneksi
        int isolatedNodes = 0;
        foreach (var kvp in graph)
        {
            if (kvp.Value.Count == 0)
            {
                Debug.LogWarning($"⚠️ Waypoint '{kvp.Key.name}' TERISOLASI (tidak ada koneksi)!");
                isolatedNodes++;
            }
        }
        
        if (isolatedNodes > 0)
        {
            Debug.LogWarning($"⚠️ {isolatedNodes} waypoint terisolasi! Cek posisi atau perkecil Spherecast Radius");
        }
    }
    
    // Method utama untuk cek apakah path clear
    bool IsPathClear(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        
        if (distance < 0.1f) return false;
        
        // Tambahkan offset untuk menghindari self-collision
        Vector3 startOffset = start + direction.normalized * raycastOffset;
        float checkDistance = distance - (raycastOffset * 2);
        
        if (checkDistance <= 0) return true;
        
        // Method 1: SphereCast (paling akurat)
        RaycastHit sphereHit;
        bool sphereBlocked = Physics.SphereCast(
            startOffset, 
            spherecastRadius, 
            direction.normalized, 
            out sphereHit, 
            checkDistance, 
            wallLayerMask,
            queryTriggerInteraction
        );
        
        if (sphereBlocked)
        {
            if (showDebugRays)
            {
                Debug.DrawLine(start, sphereHit.point, Color.red, 2f);
                Debug.Log($"❌ SphereCast blocked by: {sphereHit.collider.name} at {sphereHit.distance:F2}");
            }
            return false;
        }
        
        // Method 2: Multiple parallel raycasts untuk double check
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up);
        if (perpendicular.magnitude < 0.01f)
        {
            perpendicular = Vector3.Cross(direction.normalized, Vector3.forward);
        }
        perpendicular = perpendicular.normalized;
        
        for (int i = 0; i < multipleRaycastCount; i++)
        {
            float offsetAmount = ((i / (float)(multipleRaycastCount - 1)) - 0.5f) * spherecastRadius * 2f;
            Vector3 rayStart = start + perpendicular * offsetAmount;
            Vector3 rayEnd = end + perpendicular * offsetAmount;
            Vector3 rayDir = (rayEnd - rayStart).normalized;
            float rayDist = Vector3.Distance(rayStart, rayEnd);
            
            RaycastHit rayHit;
            bool rayBlocked = Physics.Raycast(
                rayStart, 
                rayDir, 
                out rayHit, 
                rayDist, 
                wallLayerMask,
                queryTriggerInteraction
            );
            
            if (rayBlocked)
            {
                if (showDebugRays)
                {
                    Debug.DrawLine(rayStart, rayHit.point, Color.yellow, 2f);
                }
                return false;
            }
        }
        
        // Method 3: Manual check dengan cached colliders (fallback)
        foreach (Collider wallCol in allWallColliders)
        {
            if (wallCol == null) continue;
            
            // Cek apakah line segment intersect dengan collider bounds
            if (DoesBoundsIntersectLine(wallCol.bounds, start, end))
            {
                // Double check dengan raycast langsung ke collider ini
                Ray ray = new Ray(start, direction.normalized);
                RaycastHit hit;
                if (wallCol.Raycast(ray, out hit, distance))
                {
                    if (showDebugRays)
                    {
                        Debug.DrawLine(start, hit.point, Color.magenta, 2f);
                    }
                    return false;
                }
            }
        }
        
        if (showDebugRays)
        {
            Debug.DrawLine(start, end, Color.green, 2f);
        }
        
        return true;
    }
    
    // Helper: Cek apakah line intersect dengan bounds
    bool DoesBoundsIntersectLine(Bounds bounds, Vector3 start, Vector3 end)
    {
        // Simplified bounds-line intersection test
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        
        // Expand bounds sedikit untuk spherecast radius
        extents += Vector3.one * spherecastRadius;
        
        Vector3 direction = end - start;
        float length = direction.magnitude;
        direction.Normalize();
        
        // AABB ray intersection
        Vector3 invDir = new Vector3(
            direction.x != 0 ? 1f / direction.x : float.MaxValue,
            direction.y != 0 ? 1f / direction.y : float.MaxValue,
            direction.z != 0 ? 1f / direction.z : float.MaxValue
        );
        
        Vector3 min = center - extents;
        Vector3 max = center + extents;
        
        float t1 = (min.x - start.x) * invDir.x;
        float t2 = (max.x - start.x) * invDir.x;
        float t3 = (min.y - start.y) * invDir.y;
        float t4 = (max.y - start.y) * invDir.y;
        float t5 = (min.z - start.z) * invDir.z;
        float t6 = (max.z - start.z) * invDir.z;
        
        float tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
        float tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));
        
        if (tmax < 0 || tmin > tmax || tmin > length)
        {
            return false;
        }
        
        return true;
    }
    
    public List<Vector3> CalculatePath(Vector3 startPos, Vector3 targetPos)
    {
        if (graph.Count == 0)
        {
            Debug.LogError("❌ Graph kosong! Jalankan Rebuild Graph.");
            return new List<Vector3>();
        }
        
        Transform startPoint = GetNearestPoint(startPos);
        Transform endPoint = GetNearestPoint(targetPos);
        
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning("❌ Start/End waypoint tidak ditemukan!");
            return new List<Vector3>();
        }
        
        if (!IsPathClear(startPos, startPoint.position))
        {
            Debug.LogWarning($"❌ Tidak bisa reach waypoint '{startPoint.name}' dari start position!");
            return new List<Vector3>();
        }
        
        List<Transform> path = FindPath(startPoint, endPoint);
        
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"❌ Tidak ada path dari {startPoint.name} ke {endPoint.name}!");
            return new List<Vector3>();
        }
        
        List<Vector3> vectorPath = new List<Vector3>();
        vectorPath.Add(startPos);
        
        foreach (Transform point in path)
        {
            vectorPath.Add(point.position);
        }
        
        if (IsPathClear(path[path.Count - 1].position, targetPos))
        {
            vectorPath.Add(targetPos);
        }
        
        if (verboseLogging)
        {
            Debug.Log($"<color=green>✅ Path found: {vectorPath.Count} waypoints</color>");
        }
        
        return vectorPath;
    }
    
    List<Transform> FindPath(Transform start, Transform end)
    {
        if (!graph.ContainsKey(start) || !graph.ContainsKey(end))
        {
            return null;
        }
        
        Dictionary<Transform, float> gScore = new Dictionary<Transform, float>();
        Dictionary<Transform, float> fScore = new Dictionary<Transform, float>();
        Dictionary<Transform, Transform> cameFrom = new Dictionary<Transform, Transform>();
        List<Transform> openSet = new List<Transform>();
        HashSet<Transform> closedSet = new HashSet<Transform>();
        
        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Vector3.Distance(start.position, end.position);
        
        while (openSet.Count > 0)
        {
            Transform current = openSet[0];
            float lowestF = fScore[current];
            
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fScore[openSet[i]] < lowestF)
                {
                    current = openSet[i];
                    lowestF = fScore[current];
                }
            }
            
            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            if (!graph.ContainsKey(current)) continue;
            
            foreach (Transform neighbor in graph[current])
            {
                if (closedSet.Contains(neighbor)) continue;
                
                float tentativeGScore = gScore[current] + Vector3.Distance(current.position, neighbor.position);
                
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (gScore.ContainsKey(neighbor) && tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }
                
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Vector3.Distance(neighbor.position, end.position);
            }
        }
        
        return null;
    }
    
    List<Transform> ReconstructPath(Dictionary<Transform, Transform> cameFrom, Transform current)
    {
        List<Transform> path = new List<Transform>();
        path.Add(current);
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        
        return path;
    }
    
    Transform GetNearestPoint(Vector3 position)
    {
        Transform nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (Transform point in points)
        {
            if (point == null) continue;
            
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = point;
            }
        }
        
        return nearest;
    }
    
    void OnDrawGizmos()
    {
        if (points == null || points.Length == 0) return;
        
        // Gambar waypoints
        foreach (Transform point in points)
        {
            if (point != null)
            {
                Gizmos.color = waypointColor;
                Gizmos.DrawSphere(point.position, 0.3f);
                Gizmos.DrawWireSphere(point.position, spherecastRadius);
            }
        }
        
        if (!visualizeConnections) return;
        
        // Setup layer mask jika belum
        if (wallLayerMask == 0)
        {
            int layerIndex = LayerMask.NameToLayer(wallLayerName);
            if (layerIndex != -1)
            {
                wallLayerMask = 1 << layerIndex;
            }
        }
        
        // Gambar koneksi
        if (Application.isPlaying && graph.Count > 0)
        {
            foreach (var kvp in graph)
            {
                if (kvp.Key == null) continue;
                
                foreach (Transform neighbor in kvp.Value)
                {
                    if (neighbor == null) continue;
                    
                    Gizmos.color = validConnectionColor;
                    DrawArrow(kvp.Key.position, neighbor.position);
                }
            }
        }
        else if (wallLayerMask != 0)
        {
            // Preview mode
            foreach (Transform point in points)
            {
                if (point == null) continue;
                
                foreach (Transform otherPoint in points)
                {
                    if (point == otherPoint || otherPoint == null) continue;
                    
                    // Test di edit mode (simplified)
                    Vector3 dir = otherPoint.position - point.position;
                    float dist = dir.magnitude;
                    
                    RaycastHit hit;
                    bool blocked = Physics.SphereCast(point.position, spherecastRadius, dir.normalized, out hit, dist, wallLayerMask);
                    
                    Gizmos.color = blocked ? invalidConnectionColor : validConnectionColor;
                    
                    if (blocked)
                    {
                        // Garis putus-putus untuk blocked
                        int segments = 10;
                        for (int i = 0; i < segments; i++)
                        {
                            if (i % 2 == 0)
                            {
                                Vector3 segStart = Vector3.Lerp(point.position, hit.point, i / (float)segments);
                                Vector3 segEnd = Vector3.Lerp(point.position, hit.point, (i + 1) / (float)segments);
                                Gizmos.DrawLine(segStart, segEnd);
                            }
                        }
                    }
                    else
                    {
                        DrawArrow(point.position, otherPoint.position);
                    }
                }
            }
        }
    }
    
    void DrawArrow(Vector3 start, Vector3 end)
    {
        Gizmos.DrawLine(start, end);
        
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;
        
        Gizmos.DrawRay(end, right * 0.3f);
        Gizmos.DrawRay(end, left * 0.3f);
    }
    
    [ContextMenu("Debug: List All Wall Colliders")]
    void DebugListWallColliders()
    {
        CacheAllWallColliders();
        
        Debug.Log("═══════════════════════════════════");
        Debug.Log($"Found {allWallColliders.Count} wall colliders:");
        Debug.Log("═══════════════════════════════════");
        
        for (int i = 0; i < allWallColliders.Count; i++)
        {
            Collider col = allWallColliders[i];
            if (col != null)
            {
                string hierarchy = GetHierarchyPath(col.transform);
                Debug.Log($"{i + 1}. {col.GetType().Name} - {hierarchy}");
            }
        }
    }
    
    string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}