using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    private BoxCollider2D bc;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovementInput();
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
}
