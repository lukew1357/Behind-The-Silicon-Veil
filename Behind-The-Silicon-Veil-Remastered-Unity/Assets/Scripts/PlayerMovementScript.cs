using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("Player Movement")]
    //player statistics
    public float playerSpeed = 10f;
    public float playerJumpPower = 15f;
    public int extraJumps = 1;

    //objects and components
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform playerFeet;

    //variables that change in real time
    private int jumpCount = 0;
    private bool isGrounded;
    private float mx;
    private float jumpCoolDown;
    private bool isFacingRight = true;

    [Header("Wall Jump")]
    //player statistics
    public float wallSlideSpeed = 0.3f;
    public float wallDistance = 0.55f;

    //variables that change in real time
    public float wallJumpTime = 0.15f;
    private bool isWallSliding = false;
    private RaycastHit2D wallCheckHit;
    private float jumpTime;
    private bool canWallJump = true;
    private string previousJumpedWall = "";

    [Header("Dash")]
    //player statistics
    public float dashSpeed = 50f;
    public float dashCooldown = 3f;

    //variables that change in real time
    public float dashDuration = 0.1f;
    private float dashCooldownTimer = 0.0f;
    private float dashTime = 0f;
    private float dashDirection;
    private bool canDash = true;
    private bool isDashing = false;

    public bool dashUnlocked = true;

    [Header("Camera Follow")]
    [SerializeField] CinemachineVirtualCamera vCam1;



    // Start is called before the first frame update
    void Start()
    {
        canDash = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        CheckGrounded();
        DashHandler();
    }

    private void FixedUpdate()
    {
        MovementHandler();
        WallJumpHandler();
        Dash();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (collision.gameObject.name != previousJumpedWall)
            {
                previousJumpedWall = collision.gameObject.name;
                canWallJump = true;
            }
            else
            {
                canWallJump = false;
            }
        }
    }

    void MovementHandler()
    {
        //gets player input
        mx = Input.GetAxis("Horizontal");
        if (mx < 0)
        {
            isFacingRight = false;
        }
        else if (mx > 0)
        {
            isFacingRight = true;
        }

        //flips player sprite
        if (!Mathf.Approximately(0, mx))
            transform.rotation = mx < 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        //gives player velocity
        rb.velocity = new Vector2(mx * playerSpeed, rb.velocity.y);
    }

    void WallJumpHandler()
    {
        //wall sliding raycast
        if (isFacingRight)
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(wallDistance, 0), wallDistance, groundLayer);
            //Debug.DrawRay(transform.position, new Vector2(wallDistance, 0), Color.blue);
        }
        else
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(-wallDistance, 0), wallDistance, groundLayer);
            //Debug.DrawRay(transform.position, new Vector2(-wallDistance, 0), Color.blue);
        }

        if (wallCheckHit && !isGrounded && mx != 0)
        {
            isWallSliding = true;
            jumpTime = Time.time + wallJumpTime;
        }
        else if (jumpTime < Time.time)
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, wallSlideSpeed, float.MaxValue));
        }
    }

    void Jump()
    {
        if (isGrounded || jumpCount < extraJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, playerJumpPower);
            jumpCount++;
        }
        else if (isWallSliding && canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, playerJumpPower);
            canWallJump = false;
        }
    }

    void CheckGrounded()
    {
        if (Physics2D.OverlapCircle(playerFeet.position, 0.1f, groundLayer))
        {
            isGrounded = true;
            jumpCount = 0;
            jumpCoolDown = Time.time + 0.2f;
        }
        else if (Time.time < jumpCoolDown)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void DashHandler()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            //stops infinite dashing
            canDash = false;
            isDashing = true;
            dashTime = Time.time + dashDuration;
            dashCooldownTimer = Time.time + dashCooldown;

            //gets dash direction
            Vector2 direction = transform.right;
            direction.Normalize();
            dashDirection = direction.x;
        }
    }

    void Dash()
    {
        if (isDashing && dashTime >= Time.time)
        {
            rb.velocity = new Vector2(dashDirection, 0.1f) * dashSpeed;
        }
        if (dashTime < Time.time)
        {
            isDashing = false;
        }
        if (dashCooldownTimer < Time.time)
        {
            canDash = true;
        }
    }
}
