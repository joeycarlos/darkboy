﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    [SerializeField] private Vector3 attackBoxSize;
    [SerializeField] private float impactBoxUnitWidth;
    [SerializeField] private float impactBoxSizeHeight;
    [SerializeField] private float damage = 3.0f;
    [SerializeField] private float knockbackPower = 5.0f;
    [SerializeField] private float maxChargeTime = 3.0f;
    [SerializeField] private float attackAnimTime = 0.25f;
    [SerializeField] private GameObject attackIndicator;
    private float attackChargeValue;
    private float attackAnimCounter;
    private GameObject iAttackIndicator;
    private bool maxIndicatorRangeReached;
    private bool attackPending;
    private bool maxChargeReached;

    [SerializeField] private GameObject chargingParticleEffect;
    private GameObject iChargingParticleEffect;
    [SerializeField] private GameObject impactParticleEffect;
    private GameObject iImpactParticleEffect;

    // HEALTH
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    // KNOCKBACK STATE
    [Header("Knockback State")]
    [SerializeField] private float knockbackTime = 0.3f;
    [SerializeField] private float selfKnockbackPower = 3.0f;
    private float knockbackTimeCounter;

    // RECOVERY STATUS
    [Header("Recovery Status")]
    [SerializeField] private float recoveryTime = 1.5f;
    [SerializeField] private float recoveryAlphaFlashTime = 0.05f;
    [HideInInspector] public bool isRecovering;

    // SPIRIT
    [Header("Spirit")]
    [SerializeField] private int[] spiritLevelReqs = new int[4];
    private int currentSpirit;
    private int spiritLevel;

    // COMPONENTS
    private BoxCollider2D bc;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private GameObject attackPos;
    private SpriteRenderer[] spriteComponents;
    private DestructibleTiles destructibleTiles;

    // ANIMATIONS
    private Animator anim;
    [SerializeField] private SpriteRenderer headSprite;

    // STATE
    [HideInInspector] public State state;

    public enum State { Idle, Running, Jumping, Falling, Charging, Attacking, Knockback, Death, Pause };

    void Start() {
        LinkComponents();
        InitPlayerVars();
        InitPlayerUI();
    }

    void Update() {
        horizontalInput = Input.GetAxis("Horizontal");
        isGrounded = CheckGround();
        anim.SetInteger("State", (int)state);
        anim.SetBool("maxChargeReached", maxChargeReached);

        switch (state) {
            case State.Idle:
                if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
                    EnterChargingState();
                    state = State.Charging;
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped) {
                    EnterJumpState();
                    state = State.Jumping;
                }
                else if (horizontalInput != 0)
                    state = State.Running;
                break;
            case State.Running:
                ProcessMovementInput();

                if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) {
                    EnterChargingState();
                    state = State.Charging;
                }
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
            case State.Knockback:
                knockbackTimeCounter = knockbackTimeCounter - Time.deltaTime;
                isGroundedRememberCounter -= Time.deltaTime;

                if (knockbackTimeCounter <= 0) {
                    if (Input.GetKeyDown(KeyCode.UpArrow) && (isGrounded == true || isGroundedRememberCounter >= 0) && !hasJumped)
                        state = State.Jumping;
                    else if (horizontalInput != 0 && isGrounded == true)
                        state = State.Running;
                    else if (horizontalInput == 0 && isGrounded == true)
                        state = State.Idle;
                    else if (isGrounded == false)
                        state = State.Falling;
                }
                break;
            // FIX STATES BELOW
            case State.Charging:
                attackChargeValue = Mathf.Clamp(attackChargeValue += Time.deltaTime, 0.0f, maxChargeTime);
                // UpdateAttackIndicator(CalcRange(attackChargeValue / maxChargeTime), isFacingRight);
                headSprite.color = new Color(1, 0.9f - Mathf.Clamp(attackChargeValue/5.0f, 0, 0.3f), 0.9f - Mathf.Clamp(attackChargeValue / 5.0f, 0, 0.3f));

                if (maxChargeReached == false && attackChargeValue >= maxChargeTime) {
                    maxChargeReached = true;
                    Destroy(iChargingParticleEffect);
                }

                if (Input.GetKeyUp(KeyCode.Space)) {
                    headSprite.color = new Color(1, 1, 1);
                    attackAnimCounter = attackAnimTime;
                    attackPending = true;
                    maxChargeReached = false;
                    
                    state = State.Attacking;
                    if (iChargingParticleEffect != null)
                        Destroy(iChargingParticleEffect);
                }
                break;
            case State.Attacking:
                attackAnimCounter -= Time.deltaTime;

                if (attackAnimCounter <= attackAnimTime - 0.05f && attackPending == true) {
                    // Attack(CalcRange(attackChargeValue / maxChargeTime), CalcDamage(attackChargeValue / maxChargeTime), CalcKnockback(attackChargeValue / maxChargeTime), isFacingRight);
                    attackChargeValue = Mathf.Clamp(attackChargeValue, 0, maxChargeTime);
                    Attack(CalcRange(attackChargeValue), CalcDamage(attackChargeValue), CalcKnockback(attackChargeValue), isFacingRight);
                    attackPending = false;
                    attackChargeValue = 0;
                    Destroy(iAttackIndicator);
                }

                if (attackAnimCounter <= 0) {
                    if (horizontalInput == 0)
                        state = State.Idle;
                    else if (horizontalInput != 0)
                        state = State.Running;
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                        state = State.Jumping;
                    else if (Input.GetKeyDown(KeyCode.Space)) {
                        state = State.Charging;
                    }
                        
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
        headSprite = transform.Find("Head").GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        attackPos = transform.Find("AttackPos").gameObject;
        spriteComponents = GetComponentsInChildren<SpriteRenderer>();

        // Find the destructible tilemap
        Tilemap[] tilemapsList;
        tilemapsList = FindObjectsOfType<Tilemap>();
        foreach (Tilemap tm in tilemapsList) {
            if (tm.tag == "Destructible") {
                destructibleTiles = tm.GetComponent<DestructibleTiles>();
            }
        }
    }

    void InitPlayerUI() {
        GameplayUI.Instance.GenerateHealthUI(maxHealth);
        /*
        GameplayUI.Instance.UpdateSpiritLevelValue(spiritLevel);
        GameplayUI.Instance.spiritBarMin = 0;
        GameplayUI.Instance.spiritBarMax = spiritLevelReqs[spiritLevel - 1];
        GameplayUI.Instance.SetSpirit(currentSpirit);
        */
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
        attackAnimCounter = 0;
        isRecovering = false;
        maxChargeReached = false;
    }

    // MOVEMENT HELPER FUNCTIONS

    void ProcessMovementInput() {
        if ((isFacingRight == true && horizontalInput < 0) || (isFacingRight == false && horizontalInput > 0)) {
            SwitchDirection();
        }

        if (horizontalInput != 0) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));
            if (hit.collider == null)
                transform.Translate(new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0));
        }
    }

    void SwitchDirection() {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
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

    // KNOCKBACK STATE HELPER FUNCTIONS

    public bool isHittable() {
        if (isRecovering == true) return false;
        else return true;
    }

    public void EnterKnockbackState(int damage, bool isRightDirection) {
        state = State.Knockback;
        
        TakeDamage(damage);
        Knockback(isRightDirection);
        knockbackTimeCounter = knockbackTime;
        StartCoroutine(RecoveryAlphaFlash(recoveryTime, recoveryAlphaFlashTime));
        StartCoroutine(RecoveryStatus(recoveryTime));
    }

    IEnumerator RecoveryStatus(float recoveryTime) {
        isRecovering = true;
        yield return new WaitForSeconds(recoveryTime);
        isRecovering = false;
    }

    IEnumerator RecoveryAlphaFlash(float recoveryTime, float recoveryAlphaFlashTime) {
        float recoveryTimeCounter = recoveryTime - 0.05f; // to ensure flashing is shorter than actual hurt state
        bool pendingAlpha = true;
        Color pendingColor;

        while (recoveryTimeCounter > 0) {
            foreach (SpriteRenderer sr in spriteComponents) {
                if (pendingAlpha) {
                    pendingColor = new Color(1, 1, 1, 0.3f);
                } else {
                    pendingColor = new Color(1, 1, 1, 1);
                }

                sr.color = pendingColor;
            }
            pendingAlpha = !pendingAlpha;
            recoveryTimeCounter -= recoveryAlphaFlashTime;

            yield return new WaitForSeconds(recoveryAlphaFlashTime);
        }
    }

    public void DisableRecoveryStatus() {
        foreach (SpriteRenderer sr in spriteComponents) {
            sr.color = new Color(1, 1, 1, 1);
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
            knockbackVector = new Vector2(selfKnockbackPower, selfKnockbackPower);
        } else {
            knockbackVector = new Vector2(-selfKnockbackPower, selfKnockbackPower);
        }
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);
    }

    void Death() {
        Destroy(gameObject);
    }

    // HEALTH HELPER FUNCTIONS



    public void AddHealth(int value) {
        int originalHealth = currentHealth;
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        GameplayUI.Instance.AddHealthIcon(currentHealth - originalHealth);
    }

    // ATTACKING

    private void EnterChargingState() {
        iChargingParticleEffect = Instantiate(chargingParticleEffect, transform.position, Quaternion.identity);
    }

    private void UpdateAttackIndicator(float range, bool isFacingRight) {
        if (iAttackIndicator == null) {
            iAttackIndicator = Instantiate(attackIndicator, new Vector3(transform.position.x + bc.bounds.extents.x, transform.position.y, 0), Quaternion.identity);
            maxIndicatorRangeReached = false;
        }

        if (maxIndicatorRangeReached == false) {
            if (isFacingRight) {
                int numRaycasts = 30;
                float horizontalDistance = range;
                float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

                for (int i = 0; i < numRaycasts; i++) {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                    if (hit.collider == null) {
                        horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                        maxIndicatorRangeReached = true;
                        break;
                    }
                }
                Vector2 boxSpawnStartLocation = new Vector2(transform.position.x, transform.position.y - bc.bounds.extents.y);
                Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x + horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);

                iAttackIndicator.transform.position = boxCenterLocation;
                iAttackIndicator.transform.localScale = new Vector3(horizontalDistance, impactBoxSizeHeight, 0);
            }
            else {
                int numRaycasts = 30;
                float horizontalDistance = range; // will be calculated based off of charge and level
                float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

                for (int i = 0; i < numRaycasts; i++) {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x - i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                    if (hit.collider == null) {
                        horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                        maxIndicatorRangeReached = true;
                        break;
                    }
                }

                Vector2 boxSpawnStartLocation = new Vector2(transform.position.x, transform.position.y - bc.bounds.extents.y);
                Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x - horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);

                iAttackIndicator.transform.position = boxCenterLocation;
                iAttackIndicator.transform.localScale = new Vector3(horizontalDistance, impactBoxSizeHeight, 0);
            }
        }

    }

    private void Attack(float range, float damage, float knockback, bool isFacingRight) {
        Collider2D[] enemiesToDamage;

        if (isFacingRight) {
            int numRaycasts = 30;
            float horizontalDistance = range; // will be calculated based off of charge and level
            float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

            for (int i = 0; i < numRaycasts; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(attackPos.transform.position.x + i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                if (hit.collider == null) {
                    horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                    break;
                }
            }

            Vector2 boxSpawnStartLocation = new Vector2(attackPos.transform.position.x, transform.position.y - bc.bounds.extents.y);
            Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x + horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
            Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
            enemiesToDamage = Physics2D.OverlapBoxAll(boxCenterLocation, boxDimensions, 0, LayerMask.GetMask("Enemy"));

            // insert check and call to destroy tiles here
            destructibleTiles.DestroyTiles(boxSpawnStartLocation, boxDimensions);
                
            SpawnImpactParticles(boxCenterLocation, boxDimensions);

            
        }
        else {
            int numRaycasts = 30;
            float horizontalDistance = range; // will be calculated based off of charge and level
            float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

            for (int i = 0; i < numRaycasts; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(attackPos.transform.position.x - i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                if (hit.collider == null) {
                    horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                    break;
                }
            }

            Vector2 boxSpawnStartLocation = new Vector2(attackPos.transform.position.x, transform.position.y - bc.bounds.extents.y);
            Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x - horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
            Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
            enemiesToDamage = Physics2D.OverlapBoxAll(boxCenterLocation, boxDimensions, 0, LayerMask.GetMask("Enemy"));

            destructibleTiles.DestroyTiles(new Vector2(boxSpawnStartLocation.x - boxDimensions.x, boxSpawnStartLocation.y), boxDimensions);

            SpawnImpactParticles(boxCenterLocation, boxDimensions);
        }

        for (int i = 0; i < enemiesToDamage.Length; i++) {
            Enemy enemy = enemiesToDamage[i].GetComponent<Enemy>();
            if (enemy.state != Enemy.State.Hurt) {
                enemy.EnterHurtState(damage, knockback, enemy.transform.position.x - transform.position.x);
            }
        }

        CameraController.Instance.gameObject.GetComponent<Shaker>().Shake(0.3f, CameraController.Instance.transform.position);

    }

    private float CalcRange(float attackChargeValue) {
        // return 1 + (0.5f * spiritLevel + 0.5f) * (2.0f * attackChargeValue + 1.0f);
        return 2 + attackChargeValue * 2;
    }

    private float CalcDamage(float attackChargeValue) {
        // return spiritLevel * (6.0f * attackChargeValue + 1);
        return 2 + attackChargeValue * 2;
    }

    private float CalcKnockback(float attackChargeValue) {
        // return 3 + (0.5f * attackChargeValue + (spiritLevel - 1) * 0.125f) * 5.0f;
        return 5 + attackChargeValue * 3;
    }

    private void SpawnImpactParticles(Vector3 position, Vector2 dimensions) {
        iImpactParticleEffect = Instantiate(impactParticleEffect, position, Quaternion.identity);
        ParticleSystem.ShapeModule ps = iImpactParticleEffect.GetComponent<ParticleSystem>().shape;
        ps.scale = new Vector3(dimensions.x, 1, 1);
        Destroy(iImpactParticleEffect, 1.0f);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;

        if (bc != null) {
            if (isFacingRight) {
                int numRaycasts = 30;
                float horizontalDistance = 10.0f; // will be calculated based off of charge and level
                float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

                for (int i = 0; i < numRaycasts; i++) {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                    if (hit.collider == null) {
                        horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                        break;
                    }
                }

                Vector2 boxSpawnStartLocation = new Vector2(transform.position.x, transform.position.y - bc.bounds.extents.y);
                Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x + horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
                Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
                Gizmos.DrawWireCube(boxCenterLocation, boxDimensions);
            }
            else {
                int numRaycasts = 30;
                float horizontalDistance = 10.0f; // will be calculated based off of charge and level
                float distBetweenRaycasts = horizontalDistance / (float)numRaycasts;

                for (int i = 0; i < numRaycasts; i++) {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x - i * distBetweenRaycasts, transform.position.y), -Vector2.up, bc.bounds.extents.y + 0.1f, LayerMask.GetMask("Platform"));
                    if (hit.collider == null) {
                        horizontalDistance = Mathf.Clamp(horizontalDistance, 0, (i - 1) * distBetweenRaycasts);
                        break;
                    }
                }

                Vector2 boxSpawnStartLocation = new Vector2(transform.position.x, transform.position.y - bc.bounds.extents.y);
                Vector3 boxCenterLocation = new Vector3(boxSpawnStartLocation.x - horizontalDistance / 2, boxSpawnStartLocation.y + impactBoxSizeHeight / 2, 0);
                Vector2 boxDimensions = new Vector2(horizontalDistance, impactBoxSizeHeight);
                Gizmos.DrawWireCube(boxCenterLocation, boxDimensions);
            }
        }
    }

    // TRIGGERS AND COLLISIONS

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("Pickup")) {
            Pickup p = col.gameObject.GetComponent<Pickup>();
            // AddSpirit(p.spiritValue);
            Destroy(p.gameObject);
        }
    }
}

/*
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
*/
