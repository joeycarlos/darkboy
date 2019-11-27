using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 10.0f;
    public float knockbackResistance = 0.0f;
    public int damage = 1;

    public HealthBar healthBar;

    private float currentHealth;
    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage) {
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Death();
        }
        healthBar.SetSize(currentHealth / maxHealth);
        Debug.Log("Taking damage");
    }

    public void Knockback(float knockbackPower, bool isRightDirection) {
        Vector2 knockbackVector;
        if (isRightDirection) {
            knockbackVector = new Vector2((knockbackPower - knockbackResistance)/2.0f, knockbackPower - knockbackResistance);
        } else {
            knockbackVector = new Vector2(-(knockbackPower - knockbackResistance)/2.0f, knockbackPower - knockbackResistance);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    void Death() {
        Destroy(gameObject);
        Debug.Log("Enemy has died");
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player")) {
            Player p = col.gameObject.GetComponent<Player>();
            p.TakeDamage(damage);
            if (col.GetContact(0).normal.x == -1.0f || (transform.position.x < p.transform.position.x))
                p.Knockback(true);
            else if (col.GetContact(0).normal.x == 1.0f || (transform.position.x > p.transform.position.x)) {
                p.Knockback(false);
            }
        }
    }
}
