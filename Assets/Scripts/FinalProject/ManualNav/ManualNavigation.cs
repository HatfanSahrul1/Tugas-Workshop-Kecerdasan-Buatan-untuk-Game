using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualNavigation : MonoBehaviour
{
    [SerializeField] Transform[] leftWaypoints, 
                                rightWaypoints, 
                                topWaypoints, 
                                bottomWaypoints, 
                                leftSideWaypoints, 
                                rightSideWaypoints,
                                entireWaypoints;

    [SerializeField] private float moveSpeed = 5f;

    public void GetLoopPath(Transform agent) => GetPath(agent, entireWaypoints, true);

    public void GetPath(Transform agent, Transform[] waypoints)
    {
        StartCoroutine(MoveAlongPath(agent, waypoints, false));
    }

    public void GetPath(Transform agent, Transform[] waypoints, bool loop)
    {
        StartCoroutine(MoveAlongPath(agent, waypoints, loop));
    }

    IEnumerator MoveAlongPath(Transform agent, Transform[] waypoints, bool loop)
    {
        do
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null || agent == null) continue;

                // Move to current waypoint
                while (Vector2.Distance(agent.position, waypoints[i].position) > 0.2f)
                {
                    Vector2 direction = (waypoints[i].position - agent.position).normalized;
                    agent.position += new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime;
                    yield return null;
                }
            }
        }
        while (loop);
    }
}