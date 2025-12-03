using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Idle,
    Spawn
}

public class StateMachine : MonoBehaviour
{
    [Header("State Settings")]
    [SerializeField] private float idleTime = 7f;
    [SerializeField] private float spawnTime = 2f;
    
    [Header("References")]
    [SerializeField] private FinalSpawner spawner;
    
    private BossState currentState;
    private float stateTimer;
    
    void Start()
    {
        // Start in Idle state
        ChangeState(BossState.Idle);
    }
    
    void Update()
    {
        stateTimer -= Time.deltaTime;
        
        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdleState();
                break;
                
            case BossState.Spawn:
                UpdateSpawnState();
                break;
        }
    }
    
    void ChangeState(BossState newState)
    {
        // Exit current state
        ExitState(currentState);
        
        // Change to new state
        currentState = newState;
        
        // Enter new state
        EnterState(newState);
    }
    
    void EnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                Debug.Log("Boss entering Idle state");
                stateTimer = idleTime;
                break;
                
            case BossState.Spawn:
                Debug.Log("Boss entering Spawn state");
                stateTimer = spawnTime;
                // Call spawner when entering spawn state
                if (spawner != null)
                {
                    spawner.SpawnEnemies();
                }
                break;
        }
    }
    
    void UpdateIdleState()
    {
        // Just wait for timer to finish
        if (stateTimer <= 0f)
        {
            ChangeState(BossState.Spawn);
        }
    }
    
    void UpdateSpawnState()
    {
        // Wait for spawn animation/time to finish
        if (stateTimer <= 0f)
        {
            ChangeState(BossState.Idle);
        }
    }
    
    void ExitState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                Debug.Log("Boss exiting Idle state");
                break;
                
            case BossState.Spawn:
                Debug.Log("Boss exiting Spawn state");
                break;
        }
    }
    
    // Public method to get current state (for debugging or UI)
    public BossState GetCurrentState()
    {
        return currentState;
    }
    
    // Public method to get remaining time in current state
    public float GetStateTimeRemaining()
    {
        return stateTimer;
    }
}
