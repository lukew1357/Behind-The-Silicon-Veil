using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovementScript : MonoBehaviour, IDataPersistence
{
    [Header("Player Movement")]
    //player statistics
    [SerializeField] float playerSpeed = 10f;
    [SerializeField] float playerJumpPower = 15f;
    [SerializeField] float gravityScale = 3f;
    [SerializeField] private Transform respawnPoint;
    public float jumpLeniency = 0.7f;
    public int extraJumps = 1;

    //objects and components
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform playerFeet;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] BoxCollider2D coll;


    //variables that change in real time
    private int jumpCount = 0;
    private float mx;
    private float newSpeed;
    private float jumpCoolDown;
    private bool disableMovement = false;
    private bool isGrounded = false;
    private bool isFacingRight = true;

    [Header("Wall Jump")]
    //player statistics
    public float wallSlideSpeed = 0.3f;
    public float wallDistance = 0.52f;

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

    [Header("Camera and UI")]
    [SerializeField] CinemachineVirtualCamera vCam1;
    public GameObject deathScreen;
    public GameObject blackScreen;
    private SpriteRenderer deathScreenSr;
    private SpriteRenderer blackScreenSr;
    private bool deathScreenEnabled = false;
    private float deathScreenCounter = 0f;

    [Header("Script References")]
    public GameData gameData;

    

    [Header("Abilities")]
    public bool doubleJumpUnlocked = true;
    public bool dashUnlocked = true;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        deathScreenSr = deathScreen.GetComponent<SpriteRenderer>();
        blackScreenSr = blackScreen.GetComponent<SpriteRenderer>();
    }

    public void LoadData(GameData data)
    {
        this.transform.position = data.playerPosition;
    }

    public void SaveData(GameData data)
    {
        data.playerPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        DashHandler();
    }

    private void FixedUpdate()
    {
        if (disableMovement)
        {
            return;
        }
        MovementHandler();
        WallJumpHandler();
        Dash();

        if (gameData.health <= 0)
        {
            StartCoroutine(HandleDeath());
        }
        if (deathScreenEnabled)
        {
            DeathScreen();
        }
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

        //increases agility of player
        if (mx > 0)
        {
            newSpeed = Mathf.Sqrt(Mathf.Sqrt(mx));
        }
        else if (mx < 0)
        {
            newSpeed = Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(mx))) * -1;
        }
        else
        {
            newSpeed = 0f;
        }

        //gives player velocity
        rb.velocity = new Vector2(newSpeed * playerSpeed, rb.velocity.y);
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
        if (Physics2D.OverlapCircle(playerFeet.position, 0.2f, groundLayer))
        {
            isGrounded = true;
            jumpCount = 0;
            jumpCoolDown = Time.time + jumpLeniency;
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
            rb.velocity = new Vector2(dashDirection, 0.15f) * dashSpeed;
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

    private void UpdateAnimator()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("movementX", rb.velocity.x);
        animator.SetFloat("movementY", rb.velocity.y);
    }

    private IEnumerator HandleDeath()
    {
        // freeze player movemet
        rb.gravityScale = 0;
        disableMovement = true;
        rb.velocity = Vector3.zero;
        // prevent other collisions
        coll.enabled = false;
        // hide the player visual
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);

        // send off event that we died for other components in our system to pick up
        GameEventsManager.instance.PlayerDeath();
        yield return new WaitForSeconds(0.4f);
        deathScreenEnabled = true;
    }

    private void DeathScreen()
    {
        deathScreenCounter += Time.deltaTime;
        if (deathScreenCounter <= 2)
        {
            blackScreenSr.color = new Color(0, 0, 0, deathScreenCounter / 2);
        }
        if(deathScreenCounter > 1 && deathScreenCounter <= 3)
        {
            deathScreenSr.color = new Color(255, 255, 255, deathScreenCounter / 2);
        }
    }

    private void Respawn()
    {
        // enable movement
        rb.gravityScale = gravityScale;
        coll.enabled = true;
        disableMovement = false;
        // show player visual
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
        // move the player to the respawn point
        this.transform.position = respawnPoint.position;
        deathScreenEnabled = false;
        deathScreenCounter = 0f;
        blackScreenSr.color = new Color(0, 0, 0, 0);
        deathScreenSr.color = new Color(255, 255, 255, 0);
    }
}
