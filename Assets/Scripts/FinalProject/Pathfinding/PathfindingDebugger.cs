using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingDebugger : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] Transform testStart;
    [SerializeField] Transform testEnd;
    [SerializeField] LayerMask wallLayerMask;
    [SerializeField] float spherecastRadius = 0.2f;
    
    [Header("Visualization")]
    [SerializeField] bool showRaycast = true;
    [SerializeField] bool continuousTest = false;
    
    void OnDrawGizmos()
    {
        if (!showRaycast || testStart == null || testEnd == null) return;
        
        Vector3 direction = testEnd.position - testStart.position;
        float distance = direction.magnitude;
        
        // Test dengan Raycast
        RaycastHit rayHit;
        bool rayBlocked = Physics.Raycast(testStart.position, direction.normalized, out rayHit, distance, wallLayerMask);
        
        // Test dengan SphereCast
        RaycastHit sphereHit;
        bool sphereBlocked = Physics.SphereCast(testStart.position, spherecastRadius, direction.normalized, out sphereHit, distance, wallLayerMask);
        
        // Visualisasi Raycast
        Gizmos.color = rayBlocked ? Color.red : Color.green;
        if (rayBlocked)
        {
            Gizmos.DrawLine(testStart.position, rayHit.point);
            Gizmos.DrawWireSphere(rayHit.point, 0.2f);
        }
        else
        {
            Gizmos.DrawLine(testStart.position, testEnd.position);
        }
        
        // Visualisasi SphereCast
        Gizmos.color = sphereBlocked ? Color.yellow : Color.cyan;
        if (sphereBlocked)
        {
            Gizmos.DrawLine(testStart.position, sphereHit.point);
            Gizmos.DrawWireSphere(sphereHit.point, spherecastRadius);
        }
        
        // Draw start dan end points
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(testStart.position, 0.3f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(testEnd.position, 0.3f);
    }
    
    void Update()
    {
        if (continuousTest && testStart != null && testEnd != null)
        {
            TestConnection();
        }
    }
    
    [ContextMenu("Test Connection")]
    void TestConnection()
    {
        if (testStart == null || testEnd == null)
        {
            Debug.LogError("Test Start or End is not assigned!");
            return;
        }
        
        Vector3 direction = testEnd.position - testStart.position;
        float distance = direction.magnitude;
        
        Debug.Log("=== Testing Connection ===");
        Debug.Log($"Distance: {distance}");
        
        // Test Raycast
        RaycastHit rayHit;
        bool rayBlocked = Physics.Raycast(testStart.position, direction.normalized, out rayHit, distance, wallLayerMask);
        Debug.Log($"Raycast blocked: {rayBlocked}" + (rayBlocked ? $" at {rayHit.distance} (hit: {rayHit.collider.name})" : ""));
        
        // Test SphereCast
        RaycastHit sphereHit;
        bool sphereBlocked = Physics.SphereCast(testStart.position, spherecastRadius, direction.normalized, out sphereHit, distance, wallLayerMask);
        Debug.Log($"SphereCast blocked: {sphereBlocked}" + (sphereBlocked ? $" at {sphereHit.distance} (hit: {sphereHit.collider.name})" : ""));
        
        // Test LayerMask
        Debug.Log($"LayerMask value: {wallLayerMask.value}");
    }
}
