using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private Color enemyColor = Color.red;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // Get or add SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set enemy color
        spriteRenderer.color = enemyColor;
        
        // Create a simple square sprite if none assigned
        if (spriteRenderer.sprite == null)
        {
            CreateSimpleSprite();
        }
        
        Debug.Log(gameObject.name + " enemy spawned with " + health + " health");
    }
    
    void CreateSimpleSprite()
    {
        // Create a simple white texture
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Health: " + health);
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log(gameObject.name + " died!");
        Destroy(gameObject);
    }
    
    // Simple AI - move randomly
    void Update()
    {
        // Simple random movement
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        transform.Translate(randomDirection * speed * Time.deltaTime);
    }
    
    // Public method to set enemy type
    public void SetEnemyType(string typeName, Color color)
    {
        gameObject.name = typeName;
        enemyColor = color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}