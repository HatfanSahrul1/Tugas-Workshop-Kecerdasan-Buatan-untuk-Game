using UnityEngine;

public class CarAI : MonoBehaviour
{
    [Header("Path")]
    public PathCircular path;            // assign GameObject yang punya Path_Circular
    public int startIndex = 0;            // node awal (index)
    public float reachDistance = 1.0f;    // jarak untuk anggap "sampai" node

    [Header("Speed")]
    public float maxSpeed = 5f;
    public float speedSmoothTime = 0.12f;
    [Range(0f, 1f)]
    public float speedReductionOnTurn = 0.6f; // seberapa lambat saat belok tajam

    [Header("Steering")]
    public float steerSmoothTime = 0.12f;      // smoothing untuk rotasi
    public float maxRotateDegreesPerSec = 180f; // limit rotasi per detik

    [Header("Whisker Sensors (Wall Avoidance)")]
    public float whiskerLength = 2f;           // panjang sensor raycast
    public float whiskerAngle = 30f;           // sudut whisker kiri/kanan dari depan
    public LayerMask wallLayerMask = 1 << 6;   // layer Wall (biasanya layer 6)
    public float avoidanceStrength = 2f;       // kekuatan menghindari dinding
    public float wallSpeedReduction = 0.5f;    // pengurangan speed saat dekat dinding

    // runtime state
    int currentIndex;
    float currentSpeed = 0f;
    float speedVelRef = 0f;
    float angVelRef = 0f;
    
    // whisker sensor data
    Vector3 avoidanceForce = Vector3.zero;

    void OnEnable()
    {
        currentIndex = Mathf.Max(0, startIndex);
    }

    void Start()
    {
        if (path == null || path.nodes == null || path.nodes.Length == 0)
        {
            Debug.LogWarning("Path/Circular nodes belum diassign atau kosong.", this);
            enabled = false;
            return;
        }

        currentIndex = Mathf.Clamp(startIndex, 0, path.nodes.Length - 1);
    }

    // Whisker sensor untuk deteksi dinding - memancarkan 3 raycast
    Vector3 DetectWalls()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 pos = transform.position;
        Vector3 forward = transform.up; // dalam 2D, "up" adalah forward
        
        // 3 arah whisker: tengah, kiri, kanan
        Vector3[] directions = {
            forward,                                                    // tengah
            Quaternion.Euler(0, 0, whiskerAngle) * forward,            // kiri
            Quaternion.Euler(0, 0, -whiskerAngle) * forward            // kanan
        };
        
        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, directions[i], whiskerLength, wallLayerMask);
            if (hit.collider != null)
            {
                // Hitung gaya menghindari berdasarkan jarak dan arah
                float distance = hit.distance;
                float avoidForce = (1f - distance / whiskerLength) * avoidanceStrength;
                
                // Arah menghindari = berlawanan dari normal dinding
                Vector3 avoidDirection = -hit.normal;
                avoidance += avoidDirection * avoidForce;
            }
        }
        
        return avoidance;
    }

    void Update()
    {
        if (!enabled) return;
        if (path == null || path.nodes == null || path.nodes.Length == 0) return;

        // Deteksi dinding dengan whisker sensors
        avoidanceForce = DetectWalls();

        Transform targetNode = path.nodes[currentIndex];
        if (targetNode == null)
        {
            currentIndex = (currentIndex + 1) % path.nodes.Length;
            return;
        }

        Vector3 pos = transform.position;
        Vector3 targetPos = targetNode.position;
        Vector3 toTarget = targetPos - pos;
        float distToTarget = toTarget.magnitude;

        // jika sudah mencapai node -> next
        if (distToTarget <= reachDistance)
        {
            currentIndex = (currentIndex + 1) % path.nodes.Length;
            targetNode = path.nodes[currentIndex];
            if (targetNode == null) return;
            targetPos = targetNode.position;
            toTarget = targetPos - pos;
            distToTarget = toTarget.magnitude;
        }

        // hindari perhitungan bila sangat dekat / nol
        if (toTarget.sqrMagnitude < Mathf.Epsilon)
        {
            currentSpeed = 0f;
            return;
        }

        // Combine path following dengan wall avoidance
        Vector3 desiredDirection = toTarget.normalized;
        if (avoidanceForce.magnitude > 0.1f)
        {
            // Blend antara menuju target dan menghindari dinding
            desiredDirection = (desiredDirection + avoidanceForce.normalized).normalized;
        }

        // desired angle dari direction yang sudah di-blend
        float desiredAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.eulerAngles.z;

        // smooth angle
        float smoothed = Mathf.SmoothDampAngle(currentAngle, desiredAngle, ref angVelRef, steerSmoothTime);
        // clamp perubahan per-frame agar stabil
        float maxDelta = maxRotateDegreesPerSec * Time.deltaTime;
        float actualDelta = Mathf.DeltaAngle(currentAngle, smoothed);
        actualDelta = Mathf.Clamp(actualDelta, -maxDelta, maxDelta);
        float newRot = currentAngle + actualDelta;

        // apply rotation
        transform.rotation = Quaternion.Euler(0f, 0f, newRot);

        // forward dari newRot
        Vector3 forward = (Quaternion.Euler(0f, 0f, newRot) * Vector3.up).normalized;

        // turnFactor: seberapa tajam belokan (0..1)
        float angleToTarget = Mathf.DeltaAngle(newRot, desiredAngle);
        float turnFactor = Mathf.Clamp01(Mathf.Abs(angleToTarget) / 90f);

        // wallFactor: seberapa dekat dengan dinding (0..1)
        float wallFactor = Mathf.Clamp01(avoidanceForce.magnitude);

        // target speed dipengaruhi belokan dan kedekatan dinding
        float speedReduction = Mathf.Max(turnFactor * speedReductionOnTurn, wallFactor * wallSpeedReduction);
        float targetSpeed = maxSpeed * (1f - speedReduction);

        // smooth speed
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelRef, Mathf.Max(0.001f, speedSmoothTime));
        currentSpeed = Mathf.Max(0f, currentSpeed);

        // move transform
        transform.position = pos + forward * currentSpeed * Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        if (path == null || path.nodes == null || path.nodes.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.nodes.Length; i++)
        {
            Transform a = path.nodes[i];
            if (a == null) continue;
            Gizmos.DrawSphere(a.position, 0.12f);
            Transform b = path.nodes[(i + 1) % path.nodes.Length];
            if (b != null) Gizmos.DrawLine(a.position, b.position);
        }

        if (Application.isPlaying && path != null && path.nodes != null && path.nodes.Length > 0)
        {
            Gizmos.color = Color.green;
            Transform t = path.nodes[Mathf.Clamp(currentIndex, 0, path.nodes.Length - 1)];
            if (t != null) Gizmos.DrawWireSphere(t.position, reachDistance);
        }

        // Debug whisker sensors
        // if (Application.isPlaying)
        // {
            Vector3 pos = transform.position;
            Vector3 forward = transform.up;
            
            // Draw whisker rays
            Vector3[] directions = {
                forward,                                                    // tengah
                Quaternion.Euler(0, 0, whiskerAngle) * forward,            // kiri  
                Quaternion.Euler(0, 0, -whiskerAngle) * forward            // kanan
            };
            
            Color[] colors = { Color.red, Color.blue, Color.cyan };
            
            for (int i = 0; i < directions.Length; i++)
            {
                Gizmos.color = colors[i];
                Vector3 endPoint = pos + directions[i] * whiskerLength;
                Gizmos.DrawLine(pos, endPoint);
                
                // Highlight jika mendeteksi wall
                RaycastHit2D hit = Physics2D.Raycast(pos, directions[i], whiskerLength, wallLayerMask);
                if (hit.collider != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(hit.point, 0.1f);
                }
            }
            
            // Draw avoidance force vector
            if (avoidanceForce.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(pos, pos + avoidanceForce);
            }
        // }
    }
}
