using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float chaseSpeed = 3.5f; 
    public float detectionRadius = 5f;

    [Header("Patrol Settings")]
    public Vector2 patrolOffset = new Vector2(3f, 0); 
    private Vector2 startPoint;
    private Vector2 endPoint;
    private Vector2 targetPoint;

    private enum State { Patrol, Chase }
    private State currentState = State.Patrol;

    void Start()
    {
        startPoint = transform.position;
        endPoint = startPoint + patrolOffset;
        targetPoint = endPoint; 
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();

                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                Chase();

                if (distanceToPlayer > detectionRadius)
                {
                    currentState = State.Patrol;
                    targetPoint = (Vector2.Distance(transform.position, startPoint) < Vector2.Distance(transform.position, endPoint)) ? endPoint : startPoint;
                }
                break;
        }
    }

    void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

        // Flip sesuai arah targetPoint
        Flip(targetPoint.x - transform.position.x);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            targetPoint = (targetPoint == endPoint) ? startPoint : endPoint;
        }
    }

    void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);

        // Flip sesuai arah player
        Flip(player.position.x - transform.position.x);
    }

    void Flip(float direction)
    {
        if (direction == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = (direction < 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        // Radius deteksi
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Area patrol (hanya terlihat saat game running / ada posisi start)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint, 0.1f);
            Gizmos.DrawWireSphere(endPoint, 0.1f);
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
