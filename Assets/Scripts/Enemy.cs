using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 10.0f;
    public float knockbackResistance = 0.0f;

    private float currentHealth;
    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update() {
        
    }

    public void TakeDamage(float damage) {
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Death();
        }
        Debug.Log("Taking damage");
    }

    public void Knockback(float knockbackPower, bool isRightDirection) {
        Vector2 knockbackVector;
        if (isRightDirection) {
            knockbackVector = new Vector2((knockbackPower - knockbackResistance)/2.0f, knockbackPower - knockbackResistance);
            Debug.Log(knockbackVector);
        } else {
            knockbackVector = new Vector2(-(knockbackPower - knockbackResistance)/2.0f, knockbackPower - knockbackResistance);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    void Death() {
        Destroy(gameObject);
        Debug.Log("Enemy has died");
    }
}
