using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; 

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical");   

        Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }
}
