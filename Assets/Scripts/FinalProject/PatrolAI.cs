using System.Collections;
using UnityEngine;

public class PatrolAI : MonoBehaviour
{
    public ManualNavigation manualNavigation;
    [SerializeField] float moveSpeed = 3f;
    
    private int currentIndex = 0;
    
    void Start()
    {
        if(manualNavigation != null) manualNavigation.GetLoopPath(this.transform);
    }

    public void StartPatrol() => Start();
}