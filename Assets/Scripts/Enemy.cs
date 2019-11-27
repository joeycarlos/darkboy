using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 10.0f;
    public float knockbackResistance = 0.0f;


    public HealthBar healthBar;

    private float currentHealth;
    private Rigidbody2D rb;

    private SpriteRenderer sr;

    private bool knockbackState;
    public float knockbackTime = 0.3f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        knockbackState = false;
    }

    public void TakeDamage(float damage) {
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Death();
        }
        healthBar.SetSize(currentHealth / maxHealth);
        StartCoroutine(KnockbackState(knockbackTime));
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
    }

    IEnumerator KnockbackState(float time) {
        knockbackState = true;
        sr.color = Color.black;
        yield return new WaitForSeconds(time);

        knockbackState = false;
        sr.color = Color.red;
    }
}
