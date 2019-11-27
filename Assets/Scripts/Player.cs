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

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        platformLayer = LayerMask.GetMask("Platform");
        isJumping = false;
        isGroundedRemember = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovementInput();

        isGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded() || isGroundedRemember > 0)) {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping == true) {
            if (jumpTimeCounter > 0) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space)) {
            isJumping = false;
        }

    }

    void ProcessMovementInput() {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(horizontalInput) * Vector2.right, (bc.bounds.size.x / 2.0f + 0.5f), LayerMask.GetMask("Wall"));
            if (hit.collider == null) {
                Move(horizontalInput, moveSpeed);
            }
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

        Vector3 raycastOriginOffset = new Vector3(-(bc.size.x), -bc.size.y + 0.05f, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 1.0f, platformLayer);
        if (hit.collider == null) result1 = false;
        else result1 = true;

        raycastOriginOffset = new Vector3(bc.size.x, -bc.size.y + 0.05f, 0);
        hit = Physics2D.Raycast(transform.position + raycastOriginOffset, -Vector2.up, 1.0f, platformLayer);
        if (hit.collider == null) result2 = false;
        else result2 = true;

        raycastOriginOffset = new Vector3(0, -bc.size.y, 0);
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
}
