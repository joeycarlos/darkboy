using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    // MOVEMENT
    [Header("Moving")]
    [SerializeField] private float moveSpeed = 5.0f;
    private bool isFacingRight;
    private float horizontalInput;

    // JUMPING
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float maxJumpTime = 0.7f;
    private float jumpTimeCounter;
    private bool hasJumped;

    // GROUND CHECK
    [Header("Ground Check")]
    [SerializeField] private float isGroundedRememberTime = 0.15f;
    private float isGroundedRememberCounter;
    private int platformLayer;
    private bool isGrounded;

    // ATTACKING
    [Header("Attacking")]
    [SerializeField] private Transform attackPos;
    [SerializeField] private Vector3 attackBoxSize;
    [SerializeField] private float impactBoxUnitWidth;
    [SerializeField] private float impactBoxSizeHeight;
    [SerializeField] private float damage = 3.0f;
    [SerializeField] private float knockbackPower = 5.0f;
    private float attackChargeValue;
    private bool attackFinished;

    // HEALTH
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    // HURT STATE
    [Header("Hurt State")]
    [SerializeField] private float hurtTime = 0.3f;
    [SerializeField] private float selfKnockbackPower = 3.0f;
    private float hurtTimeCounter;

    // SPIRIT
    [Header("Spirit")]
    [SerializeField] private int[] spiritLevelReqs = new int[4];
    private int currentSpirit;
    private int spiritLevel;

    // COMPONENTS
    private BoxCollider2D bc;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // STATE
    [HideInInspector] public State state;

    public enum State { Idle, Running, Jumping, Falling, Charging, Attacking, Hurt, Death, Pause };

    void Start() {
        LinkComponents();
        InitPlayerVars();
        InitPlayerUI();
    }

    void Update() {
        horizontalInput = Input.GetAxis("Horizontal");
        isGrounded = CheckGround();

        switch (state) {
            case State.Idle:
                if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
                    state = State.Charging;
                else if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped) {
                    EnterJumpState();
                    state = State.Jumping;
                }
                else if (horizontalInput != 0)
                    state = State.Running;
                break;
            case State.Running:
                ProcessMovementInput();

                if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
                    state = State.Charging;
                else if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped) {
                    EnterJumpState();
                    state = State.Jumping;
                }
                else if (horizontalInput == 0)
                    state = State.Idle;
                break;
            case State.Jumping:
                ProcessMovementInput();
                isGroundedRememberCounter -= Time.deltaTime;

                hasJumped = true;
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;

                if (Input.GetKeyUp(KeyCode.UpArrow) || jumpTimeCounter <= 0) {
                    state = State.Falling;
                }

                break;
            case State.Falling:
                ProcessMovementInput();
                isGroundedRememberCounter -= Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped) {
                    EnterJumpState();
                    state = State.Jumping;
                }
                else if (horizontalInput != 0 && isGrounded == true)
                    state = State.Running;
                else if (horizontalInput == 0 && isGrounded == true)
                    state = State.Idle;
                break;
            case State.Hurt:
                hurtTimeCounter = hurtTimeCounter - Time.deltaTime;
                isGroundedRememberCounter -= Time.deltaTime;

                if (hurtTimeCounter <= 0) {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped)
                        state = State.Jumping;
                    else if (horizontalInput != 0 && isGrounded == true)
                        state = State.Running;
                    else if (horizontalInput == 0 && isGrounded == true)
                        state = State.Idle;
                    else if (isGrounded == false)
                        state = State.Falling;

                    ExitHurtState();
                }
                break;
            // FIX STATES BELOW
            case State.Charging:
                attackChargeValue += Time.deltaTime;

                if (Input.GetKeyUp(KeyCode.Space)) {
                    Attack(damage + attackChargeValue, knockbackPower + attackChargeValue);
                    state = State.Attacking;
                    attackChargeValue = 0;
                }
                break;
            case State.Attacking:
                if (attackFinished) {
                    if (horizontalInput == 0)
                        state = State.Idle;
                    else if (horizontalInput != 0)
                        state = State.Running;
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                        state = State.Jumping;
                    else if (Input.GetKeyDown(KeyCode.Space))
                        state = State.Charging;
                }
                break;
            default:
                break;
        }
    }

    // INIT HELPER FUNCTIONS

    void LinkComponents() {
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void InitPlayerUI() {
        GameplayUI.Instance.GenerateHealthUI(maxHealth);
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
        GameplayUI.Instance.spiritBarMin = 0;
        GameplayUI.Instance.spiritBarMax = spiritLevelReqs[spiritLevel - 1];
        GameplayUI.Instance.SetSpirit(currentSpirit);
    }

    void InitPlayerVars() {
        state = State.Idle;
        isFacingRight = true;
        jumpTimeCounter = 0;
        currentHealth = maxHealth;
        isGroundedRememberCounter = 0;
        hasJumped = false;
        currentSpirit = 0;
        spiritLevel = 1;
        attackChargeValue = 0;
        attackFinished = true;
    }

    // MOVEMENT HELPER FUNCTIONS

    void ProcessMovementInput() {
        if ((isFacingRight == true && horizontalInput < 0) || (isFacingRight == false && horizontalInput > 0)) {
            attackPos.localPosition = new Vector3(-attackPos.localPosition.x, attackPos.localPosition.y, attackPos.localPosition.z);
            isFacingRight = !isFacingRight;
        }

        if (horizontalInput != 0) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));
            if (hit.collider == null)
                transform.Translate(new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0));
        }
    }

    // JUMPING AND GROUNDED HELPER FUNCTIONS

    void EnterJumpState() {
        jumpTimeCounter = maxJumpTime;
        rb.velocity = Vector2.up * jumpForce;
    }

    bool CheckGround() {
        bool result1, result2, result3;

        Vector3 raycastOriginOffset = new Vector3(-bc.bounds.extents.x, -bc.bounds.extents.y + 0.05f, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 0.4f, LayerMask.GetMask("Platform"));
        if (hit.collider == null) result1 = false;
        else result1 = true;

        raycastOriginOffset = new Vector3(bc.bounds.extents.x, -bc.bounds.extents.y + 0.05f, 0);
        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 0.4f, LayerMask.GetMask("Platform"));
        if (hit.collider == null) result2 = false;
        else result2 = true;

        raycastOriginOffset = new Vector3(0, -bc.bounds.extents.y + 0.05f, 0);
        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 0.4f, LayerMask.GetMask("Platform"));
        if (hit.collider == null) result3 = false;
        else result3 = true;

        if (result1 == true || result2 == true || result3 == true) {
            isGroundedRememberCounter = isGroundedRememberTime;
            if (hasJumped == true)
                hasJumped = false;
            return true;
        }
        else
            return false;
    }

    // HURT STATE HELPER FUNCTIONS

    public bool isHittable() {
        if (state == State.Hurt) return false;
        else return true;
    }

    public void EnterHurtState(int damage, bool isRightDirection) {
        state = State.Hurt;
        TakeDamage(damage);
        Knockback(isRightDirection);
        sr.color = Color.red;
        hurtTimeCounter = hurtTime;
    }

    public void ExitHurtState() {
        sr.color = Color.white;
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
        } else {
            knockbackVector = new Vector2(-knockbackPower, knockbackPower + 2.0f);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    void Death() {
        Destroy(gameObject);
    }

    // SPIRIT AND HEALTH HELPER FUNCTIONS

    void AddSpirit(int value) {
        if (currentSpirit + value >= spiritLevelReqs[spiritLevel - 1]) {
            currentSpirit = currentSpirit + value - spiritLevelReqs[spiritLevel - 1];
            LevelUp();
        } else {
            currentSpirit += value;
        }
        GameplayUI.Instance.SetSpirit(currentSpirit);
    }

    void LevelUp() {
        spiritLevel++;
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
    }

    public void AddHealth(int value) {
        int originalHealth = currentHealth;
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        GameplayUI.Instance.AddHealthIcon(currentHealth - originalHealth);
    }

    // ATTACKING

    private void Attack(float damage, float knockbackPower) {
        Collider2D[] enemiesToDamage = Physics2D.OverlapBoxAll(attackPos.position, new Vector2(attackBoxSize.x, attackBoxSize.y), 0, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < enemiesToDamage.Length; i++) {
            if (enemiesToDamage[i].GetComponent<Enemy>().knockbackState == false) {
                enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
                enemiesToDamage[i].GetComponent<Enemy>().Knockback(knockbackPower, isFacingRight);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos.position, new Vector3(attackBoxSize.x, attackBoxSize.y, attackBoxSize.z));

        if (isFacingRight) {
            int numRaycasts = 10;
            float horizontalDistance = impactBoxUnitWidth * 5;
            float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

            for (int i = 0; i < numRaycasts; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(attackPos.position.x + attackBoxSize.x / 2 + i * distBetweenRaycasts, attackPos.position.y), -Vector2.up, attackBoxSize.y, LayerMask.GetMask("Platform"));
                if (hit.collider == null) {
                    horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                    break;
                }
            }

            Vector2 boxSpawnStartLocation = new Vector2(attackPos.position.x + attackBoxSize.x / 2, attackPos.position.y - attackBoxSize.y / 2);
            Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x + horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
            Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
            Gizmos.DrawWireCube(boxCenterLocation, boxDimensions);
        }
        else {
            int numRaycasts = 10;
            float horizontalDistance = impactBoxUnitWidth * 5;
            float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

            for (int i = 0; i < numRaycasts; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(attackPos.position.x - attackBoxSize.x / 2 - i * distBetweenRaycasts, attackPos.position.y), -Vector2.up, attackBoxSize.y, LayerMask.GetMask("Platform"));
                if (hit.collider == null) {
                    horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                    break;
                }
            }

            Vector2 boxSpawnStartLocation = new Vector2(attackPos.position.x - attackBoxSize.x / 2, attackPos.position.y - attackBoxSize.y / 2);
            Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x - horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
            Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
            Gizmos.DrawWireCube(boxCenterLocation, boxDimensions);
        }

    }

    // TRIGGERS AND COLLISIONS

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Pickup")) {
            Pickup p = col.gameObject.GetComponent<Pickup>();
            AddSpirit(p.spiritValue);
            Destroy(p.gameObject);
        }
    }
}
    /*

    void ProcessAttackInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, LayerMask.GetMask("Enemy"));
            for (int i = 0; i < enemiesToDamage.Length; i++) {
                if (enemiesToDamage[i].GetComponent<Enemy>().knockbackState == false) {
                    enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
                    enemiesToDamage[i].GetComponent<Enemy>().Knockback(knockbackPower, isFacingRight);
                }
            }
        }
    }
*/