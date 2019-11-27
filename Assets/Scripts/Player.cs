using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;

    private BoxCollider2D bc;
    private Rigidbody2D rb;

    private int platformLayer;

    public float maxJumpTime = 0.7f;
    private float jumpTimeCounter;
    private bool isJumping;

    public float isGroundedRememberTime = 0.15f;
    private float isGroundedRemember;

    public Transform attackPos;
    public float attackRange;
    public float damage = 3.0f;
    public float knockbackPower = 5.0f;

    public float selfKnockbackPower = 3.0f;
    public int maxHealth = 5;
    private int currentHealth;

    private bool isFacingRight;

    private bool isControllable;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        platformLayer = LayerMask.GetMask("Platform");
        isJumping = false;
        isGroundedRemember = 0;
        isFacingRight = true;
        currentHealth = maxHealth;
        isControllable = true;
        GameplayUI.Instance.GenerateHealthUI(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovementInput();

        isGrounded();
        if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded() || isGroundedRemember > 0)) {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (Input.GetKey(KeyCode.UpArrow) && isJumping == true) {
            if (jumpTimeCounter > 0) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            isJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, LayerMask.GetMask("Enemy"));
            for (int i = 0; i < enemiesToDamage.Length; i++) {
                enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
                enemiesToDamage[i].GetComponent<Enemy>().Knockback(knockbackPower, isFacingRight);
            }
        }

    }

    void ProcessMovementInput() {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));
            if (hit.collider == null) {
                if (isControllable == true) {
                    Move(horizontalInput, moveSpeed);
                }

            }
        }

        if ((isFacingRight == true && horizontalInput < 0) || (isFacingRight == false && horizontalInput > 0)) {
            attackPos.localPosition = new Vector3(-attackPos.localPosition.x, attackPos.localPosition.y, attackPos.localPosition.z);
            isFacingRight = !isFacingRight;
        }
    }

    void Move(float horizontalInput, float moveSpeed) {
        Vector3 moveVector = new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0);
        transform.Translate(moveVector);
    }

    bool isGrounded() {
        if (isGroundedRemember > 0)
            isGroundedRemember -= Time.deltaTime;

        bool result1;
        bool result2;
        bool result3;

        Vector3 raycastOriginOffset = new Vector3(-(bc.size.x), -bc.size.y/2.0f + 0.05f, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 1.0f, platformLayer);
        if (hit.collider == null) result1 = false;
        else result1 = true;

        raycastOriginOffset = new Vector3(bc.size.x, -bc.size.y / 2.0f + 0.05f, 0);
        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 1.0f, platformLayer);
        if (hit.collider == null) result2 = false;
        else result2 = true;

        raycastOriginOffset = new Vector3(0, -bc.size.y / 2.0f, 0);
        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 1.0f, platformLayer);
        if (hit.collider == null) result3 = false;
        else result3 = true;

        if (result1 == true || result2 == true || result3 == true) {
            isGroundedRemember = isGroundedRememberTime;
            return true;
        }
        else
            return false;
    }

    public void TakeDamage(int damage) {
        int originalHealth = currentHealth;

        currentHealth = currentHealth - damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        GameplayUI.Instance.RemoveHealthIcon(-(currentHealth - originalHealth));

        if (currentHealth <= 0) {
            Death();
        }
    }

    public void Knockback(bool isRightDirection) {
        rb.velocity = Vector2.zero;

        Vector2 knockbackVector;
        if (isRightDirection) {
            knockbackVector = new Vector2(knockbackPower, knockbackPower + 2.0f);
            Debug.Log(knockbackVector);
        }
        else {
            knockbackVector = new Vector2(-knockbackPower, knockbackPower + 2.0f);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
        StartCoroutine(KnockbackState(0.3f));
    }

    public void AddHealth(int value) {
        int originalHealth = currentHealth;
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        GameplayUI.Instance.AddHealthIcon(currentHealth - originalHealth);
    }

    void Death() {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

    IEnumerator KnockbackState(float time) {
        isControllable = false;
        yield return new WaitForSeconds(time);

        isControllable = true;
    }

}
