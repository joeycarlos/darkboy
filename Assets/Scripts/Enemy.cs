using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private float maxHealth;
    [SerializeField] private float knockbackResistance; // 0...2
    [SerializeField] private float moveSpeed;
    private float currentHealth;
    private bool isMovingRight;
    private float hurtTimeCounter;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D bc;

    public enum State { Pause, Patrol, Hurt, Death };
    [HideInInspector] public State state;

    void Start() {
        LinkComponents();
        InitEnemyVars();
    }

    void Update() {
        switch (state) {
            case State.Patrol:
                CheckDirectionSwitch();
                Patrol(moveSpeed, isMovingRight);
                break;
            case State.Hurt:
                hurtTimeCounter -= Time.deltaTime;
                if (hurtTimeCounter <= 0) {
                    sr.color = Color.white;
                    state = State.Patrol;
                }
                break;
            default:
                break;
        }
    }

    private void LinkComponents() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
    }

    private void InitEnemyVars() {
        currentHealth = maxHealth;
        isMovingRight = true;
        hurtTimeCounter = 0;

        state = State.Patrol;
    }

    private void Patrol(float moveSpeed, bool isMovingRight) {
        if (isMovingRight)
            transform.Translate(new Vector3(moveSpeed * Time.deltaTime, 0, 0));
        else
            transform.Translate(new Vector3(-moveSpeed * Time.deltaTime, 0, 0));
    }

    void CheckDirectionSwitch() {
        bool switchPending = false;

        Vector3 raycastOriginOffset;
        RaycastHit2D hit;

        
        // check if there is a wall

        if (isMovingRight) {
            raycastOriginOffset = new Vector3(bc.bounds.extents.x - 0.1f, 0, 0);
            hit = Physics2D.Raycast(transform.position + raycastOriginOffset, Vector2.right, 0.2f, LayerMask.GetMask("Platform"));
        }        
        else {
            raycastOriginOffset = new Vector3(-bc.bounds.extents.x + 0.1f, 0, 0);
            hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.right, 0.2f, LayerMask.GetMask("Platform"));
        }

        if (hit.collider != null) {
            switchPending = true;
        }

        // check is there is any ground at all

        if (isMovingRight)
            raycastOriginOffset = new Vector3(bc.bounds.extents.x - 0.1f, -bc.bounds.extents.y + 0.05f, 0);
        else
            raycastOriginOffset = new Vector3(-bc.bounds.extents.x + 0.1f, -bc.bounds.extents.y + 0.05f, 0);



        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 0.5f, LayerMask.GetMask("Platform"));
        if (hit.collider == null) switchPending = true;

        if (switchPending == true) {
            isMovingRight = !isMovingRight;
        }
    }

    public void EnterHurtState(float damage, float knockback, float distance) {
        TakeDamage(damage - (1 - distance * 0.05f));
        if (distance >= 0) {
            Knockback(knockback * (1 - ((Mathf.Abs(distance)*0.025f) + (0.25f*knockbackResistance))), true);
        } else {
            Knockback(knockback * (1 - ((Mathf.Abs(distance)*0.025f) + (0.25f*knockbackResistance))), false);
        }
        float hurtTime = 0.5f + (knockback - 3) / 10.0f;
        hurtTimeCounter = hurtTime;

        sr.color = Color.black;

        state = State.Hurt;
    }

    public void TakeDamage(float damage) {
        Debug.Log("Enemy took " + damage + "damage");

        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Death();
        }
        // healthBar.SetSize(currentHealth / maxHealth);
    }

    public void Knockback(float knockbackPower, bool isRightDirection) {
        rb.velocity = Vector2.zero;
        
        Vector2 knockbackVector;
        if (isRightDirection) {
            knockbackVector = new Vector2(knockbackPower/2.0f, knockbackPower);
        } else {
            knockbackVector = new Vector2(-knockbackPower/2.0f, knockbackPower);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
        
    }

    void Death() {
        Destroy(gameObject);
    }
        
}
