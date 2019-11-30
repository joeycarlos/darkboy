﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // MOVEMENT
    [Header("Moving")]
    [SerializeField] private float moveSpeed = 5.0f;
    private bool isFacingRight;

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

    // ATTACKING
    [Header("Attacking")]
    [SerializeField] private Transform attackPos;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float damage = 3.0f;
    [SerializeField] private float knockbackPower = 5.0f;

    // HEALTH
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    // SELF KNOCKBACK
    [Header("Hurt State")]
    [SerializeField] private float hurtTime = 0.3f;
    [SerializeField] private float selfKnockbackPower = 3.0f;

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

    // INPUTS
    float horizontalInput;
    bool isGrounded;
    float hurtTimeCounter;
    bool attackFinished;

    // -----

    public enum State { Idle, Running, Jumping, Falling, Charging, Attacking, Hurt, Death, Pause };

    void Start() {
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        state = State.Idle;
        isFacingRight = true;
        jumpTimeCounter = 0;
        currentHealth = maxHealth;
        isGroundedRememberCounter = 0;
        hasJumped = false;
        currentSpirit = 0;
        spiritLevel = 1;

        InitPlayerUI();
    }

    void Update() {
        ReadInputs();
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
                if (Input.GetKeyUp(KeyCode.Space))
                    state = State.Attacking;
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

    void ReadInputs() {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void ProcessMovementInput() {

        // Face the correct direction
        if ((isFacingRight == true && horizontalInput < 0) || (isFacingRight == false && horizontalInput > 0)) {
            attackPos.localPosition = new Vector3(-attackPos.localPosition.x, attackPos.localPosition.y, attackPos.localPosition.z);
            isFacingRight = !isFacingRight;
        }

        // Move if no wall and and not in knockback state
        if (horizontalInput != 0) {

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));

            if (hit.collider == null)
                transform.Translate(new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0));
        }
    }

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

    public bool isHittable() {
        if (state == State.Hurt)
            return false;
        else
            return true;
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
        }
        else {
            knockbackVector = new Vector2(-knockbackPower, knockbackPower + 2.0f);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    void Death() {
        Destroy(gameObject);
    }

    void InitPlayerUI() {
        GameplayUI.Instance.GenerateHealthUI(maxHealth);

        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().min = 0;
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().max = spiritLevelReqs[spiritLevel - 1];
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().SetSpirit(currentSpirit);
        
    }

    void AddSpirit(int value) {
        if (currentSpirit + value >= spiritLevelReqs[spiritLevel - 1]) {
            currentSpirit = currentSpirit + value - spiritLevelReqs[spiritLevel - 1];
            LevelUp();
        }
        else {
            currentSpirit += value;
        }
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().SetSpirit(currentSpirit);
    }

    void LevelUp() {
        spiritLevel++;
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Pickup")) {
            Pickup p = col.gameObject.GetComponent<Pickup>();
            AddSpirit(p.spiritValue);
            Destroy(p.gameObject);
        }
    }

    /*
    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        platformLayer = LayerMask.GetMask("Platform");
        isJumping = false;
        isGroundedRemember = 0;
        isFacingRight = true;
        currentHealth = maxHealth;
        knockbackState = false;
        currentSpirit = 0;
        spiritLevel = 1;

        InitPlayerUI();
    }

    void Update() {
        ProcessMovementInput();
        GroundedCheck();
        ProcessJumpInput();
        ProcessAttackInput();
    }

    void ProcessMovementInput() {
        float horizontalInput = Input.GetAxis("Horizontal");

        // Face the correct direction
        if ((isFacingRight == true && horizontalInput < 0) || (isFacingRight == false && horizontalInput > 0)) {
            attackPos.localPosition = new Vector3(-attackPos.localPosition.x, attackPos.localPosition.y, attackPos.localPosition.z);
            isFacingRight = !isFacingRight;
        }

        // Move if no wall and and not in knockback state
        if (horizontalInput != 0 && knockbackState == false) {

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));

            if (hit.collider == null)
                transform.Translate(new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0));
        }
    }

    void GroundedCheck() {
        if (isGroundedRemember > 0)
            isGroundedRemember -= Time.deltaTime;

        bool result1, result2, result3;

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
        }
    }

    void ProcessJumpInput() {

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGroundedRemember > 0) {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (Input.GetKey(KeyCode.UpArrow) && isJumping == true) {
            if (jumpTimeCounter > 0) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
                isJumping = false;
        }

        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            isJumping = false;
        }
    }

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
        StartCoroutine(KnockbackState(knockbackTime));
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
        knockbackState = true;
        sr.color = Color.red;
        yield return new WaitForSeconds(time);

        knockbackState = false;
        sr.color = Color.white;
    }

    void AddSpirit(int value) {
        if (currentSpirit + value >= spiritLevelReqs[spiritLevel - 1]) {
            currentSpirit = currentSpirit + value - spiritLevelReqs[spiritLevel - 1];
            LevelUp();
        } else {
            currentSpirit += value;
        }
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().SetSpirit(currentSpirit);
    }

    void LevelUp() {
        spiritLevel++;
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
    }

    void InitPlayerUI() {
        GameplayUI.Instance.GenerateHealthUI(maxHealth);
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);

        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().min = 0;
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().max = spiritLevelReqs[spiritLevel - 1];
        GameplayUI.Instance.GetComponentInChildren<SpiritBar>().SetSpirit(currentSpirit);
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Pickup")) {
            Pickup p = col.gameObject.GetComponent<Pickup>();
            AddSpirit(p.spiritValue);
            Destroy(p.gameObject);
        }
    }
    */


}
