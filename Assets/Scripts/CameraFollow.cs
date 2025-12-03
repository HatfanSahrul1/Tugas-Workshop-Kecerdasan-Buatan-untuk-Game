using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Drag player GameObject di sini
    
    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f; // Kecepatan smooth (0-1)
    public Vector3 offset = new Vector3(0, 0, -10); // Offset camera dari target
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Posisi yang diinginkan
        Vector3 desiredPosition = target.position + offset;
        
        // Smooth movement menggunakan Lerp
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply posisi ke camera
        transform.position = smoothedPosition;
    }
}