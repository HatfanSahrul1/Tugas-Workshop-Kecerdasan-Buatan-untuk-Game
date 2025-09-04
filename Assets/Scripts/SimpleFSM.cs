using UnityEngine;

public class SimpleFSM : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float idleDuration = 2f;
    public float walkDuration = 3f;
    public Vector2 teleportArea = new Vector2(5f, 5f); // area teleport relatif dari posisi awal

    private enum State { Idle, Walk, Teleport }
    private State currentState;

    private float stateTimer;
    private Vector2 walkDirection;
    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;
        ChangeState(State.Idle);
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                if (stateTimer <= 0)
                {
                    ChangeState(State.Walk);
                }
                break;

            case State.Walk:
                transform.Translate(walkDirection * moveSpeed * Time.deltaTime);

                if (stateTimer <= 0)
                {
                    ChangeState(State.Teleport);
                }
                break;

            case State.Teleport:
                // langsung teleport ke lokasi random
                Vector2 randomOffset = new Vector2(
                    Random.Range(-teleportArea.x, teleportArea.x),
                    Random.Range(-teleportArea.y, teleportArea.y)
                );
                transform.position = startPosition + randomOffset;

                ChangeState(State.Idle);
                break;
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                stateTimer = idleDuration;
                break;

            case State.Walk:
                stateTimer = walkDuration;
                walkDirection = Random.insideUnitCircle.normalized; // arah random
                break;

            case State.Teleport:
                stateTimer = 0.1f; // instant
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Area teleport
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(startPosition, teleportArea * 2);
    }
}
