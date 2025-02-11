using UnityEngine;
using System.Collections;

public class Yedek : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 200f;
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.1f;
    public float gravity = -9.81f;
    public float terminalVelocity = -20f;
    public int maxJumps = 1;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public bool canDash = true;
    public ParticleSystem dashParticles;

    [Header("Fishing Settings")]
    public GameObject fishingRod;
    public Transform handTransform;
    public float throwForce = 2f;
    public float throwUpwardForce = 1.5f;

    // Component references
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsuleCollider;

    // Movement state
    private Vector3 moveDirection;
    private float currentSpeed;
    private float verticalInput;
    private float horizontalInput;
    private bool isRunning;
    private float lastGroundedTime;
    private float lastJumpTime;

    // Jump state
    private bool isJumping;
    private bool isGrounded;
    private int jumpCount;
    private bool canJump = true;
    private float verticalVelocity;

    // Dash state
    private bool isDashing;
    private Vector3 dashDirection;
    private float dashTimer;
    private float lastDashTime;

    // Fishing state
    private bool isFishing;
    private bool isCasting;
    private bool isReeling;
    private Rigidbody rodRigidbody;
    private Vector3 initialRodPosition;
    private Quaternion initialRodRotation;


    private readonly int SpeedHash = Animator.StringToHash("speed");
    private readonly int IsRunningHash = Animator.StringToHash("run");
    private readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private readonly int JumpHash = Animator.StringToHash("jump");
    private readonly int LandHash = Animator.StringToHash("land");
    private readonly int DashHash = Animator.StringToHash("dash");
    private readonly int TurnRightHash = Animator.StringToHash("turnRight");
    private readonly int TurnLeftHash = Animator.StringToHash("turnLeft");
    private readonly int FishingHash = Animator.StringToHash("fishing");
    private readonly int FishingIdleHash = Animator.StringToHash("fishingidle");
    private readonly int FishingHoldHash = Animator.StringToHash("fishinghold");

    private void Awake()
    {
        InitializeComponents();
        InitializeSettings();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        rodRigidbody = fishingRod.GetComponent<Rigidbody>();
    }

    private void InitializeSettings()
    {
        rb.freezeRotation = true;
        rb.useGravity = false;
        currentSpeed = walkSpeed;

        if (rodRigidbody != null)
        {
            rodRigidbody.isKinematic = true;
            initialRodPosition = fishingRod.transform.localPosition;
            initialRodRotation = fishingRod.transform.localRotation;
        }
    }

    private void Update()
    {
        HandleTimers();
        HandleInput();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        ApplyGravity();
    }

    private void HandleTimers()
    {
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    private void HandleInput()
    {
        // Movement input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Running input
        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpTime = Time.time;
            TryJump();
        }

        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftControl) && canDash && !isDashing)
        {
            StartDash();
        }

        // Fishing input
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleFishing();
        }

        if (isFishing)
        {
            HandleFishingInput();
        }
    }

    private void CheckGrounded()
    {
        Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
        isGrounded = Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);

        // Reset jump count when grounded
        if (isGrounded && !isJumping)
        {
            jumpCount = 0;
            canJump = true;
        }
    }

    private void HandleMovement()
    {
        if (isDashing || isCasting) return;

        moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        bool isMoving = moveDirection.magnitude > 0.1f;

        if (isMoving)
        {
            // Karakter ileri veya geri hareket ederken dönüþ yapmak için
            if (verticalInput != 0)
            {
                // Ýleri veya geri hareket ederken de saða/sola dönüþ ekleyebiliriz.
                Vector3 moveVelocity = transform.forward * currentSpeed;
                rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

                // Animasyon için yatay hareket
                animator.SetFloat(SpeedHash, rb.velocity.magnitude / runSpeed);

                // Yalnýzca saða/sola dönüþ, yatay input'a göre
                if (horizontalInput != 0)
                {
                    // Yalnýzca dönüþ için döndürme iþlemi
                    transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                }
            }
            else
            {
                // Sadece saða/sola dönüþ
                rb.velocity = new Vector3(0, rb.velocity.y, 0); // Yalnýzca dikey hýz korunuyor.

                if (horizontalInput != 0)
                {
                    // Dönüþ iþlemi
                    transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                }

                // Animasyon hýzýný sýfýrlýyoruz, çünkü karakter duruyor.
                animator.SetFloat(SpeedHash, 0);
            }
        }
        else
        {
            // Eðer hareket etmiyorsa, sadece dönüþ yapýlacak
            if (horizontalInput != 0)
            {
                transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                animator.SetFloat(SpeedHash, 0);  // Dönüþ animasyonu sýrasýnda hýz sýfýrlanýyor.
            }

            // Yalnýzca y eksenindeki hýz korunuyor.
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }



    private void TryJump()
    {
        bool hasJumpBuffer = Time.time - lastJumpTime <= jumpBufferTime;
        bool hasCoyoteTime = Time.time - lastGroundedTime <= coyoteTime;

        if ((isGrounded || hasCoyoteTime) && canJump && hasJumpBuffer && jumpCount < maxJumps)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        isJumping = true;
        jumpCount++;
        canJump = false;

        // Apply jump force
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        animator.SetTrigger(JumpHash);

        // Start jump cooldown
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity = Mathf.Max(rb.velocity.y + gravity * Time.fixedDeltaTime, terminalVelocity);
            rb.velocity = new Vector3(rb.velocity.x, verticalVelocity, rb.velocity.z);
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;
        canDash = false;

        // Set dash direction and velocity
        dashDirection = transform.forward;
        rb.velocity = dashDirection * dashSpeed;

        // Visual feedback
        animator.SetBool(DashHash, true);
        if (dashParticles != null)
        {
            dashParticles.Play();
        }

        StartCoroutine(DashCooldown());
    }

    private void EndDash()
    {
        isDashing = false;
        animator.SetBool(DashHash, false);
        if (dashParticles != null)
        {
            dashParticles.Stop();
        }
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void UpdateAnimationState()
    {
        float normalizedSpeed = rb.velocity.magnitude / runSpeed;
        if (verticalInput == 0)
        {
            animator.SetFloat(SpeedHash, 0);
        }
        else
        {
            animator.SetFloat(SpeedHash, normalizedSpeed);
        }

        animator.SetBool(IsRunningHash, isRunning && normalizedSpeed > 0.1f);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(TurnRightHash, horizontalInput > 0 && verticalInput == 0);
        animator.SetBool(TurnLeftHash, horizontalInput < 0 && verticalInput == 0);
    }

    private void ToggleFishing()
    {
        isFishing = !isFishing;
        if (isFishing)
        {
            StartFishing();
        }
        else
        {
            StopFishing();
        }
    }

    private void HandleFishingInput()
    {
        if (Input.GetMouseButtonDown(1) && !isCasting)
        {
            StartCasting();
        }
        else if (Input.GetMouseButtonDown(0) && isCasting)
        {
            StartReeling();
        }
    }

    private void StartFishing()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                        RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        fishingRod.transform.SetParent(handTransform);
        fishingRod.transform.localPosition = new Vector3(0.04f, 0.091f, 0.015f);
        fishingRod.transform.localRotation = Quaternion.Euler(66f, 160f, 42f);
        rodRigidbody.isKinematic = true;

        animator.SetBool(FishingIdleHash, true);
    }

    private void StartCasting()
    {
        isCasting = true;
        animator.SetBool(FishingHash, true);
        animator.SetBool(FishingIdleHash, false);
    }

    private void StartReeling()
    {
        isCasting = false;
        isReeling = true;
        animator.SetBool(FishingHash, false);
        animator.SetBool(FishingHoldHash, true);
    }

    private void StopFishing()
    {
        // Reset constraints and states
        rb.constraints = RigidbodyConstraints.None;
        rb.freezeRotation = true;
        isFishing = false;
        isCasting = false;
        isReeling = false;

        // Reset animation states
        animator.SetBool(FishingHash, false);
        animator.SetBool(FishingIdleHash, false);
        animator.SetBool(FishingHoldHash, false);

        // Handle fishing rod
        ThrowFishingRod();
    }

    private void ThrowFishingRod()
    {
        fishingRod.transform.SetParent(null);
        fishingRod.transform.position = handTransform.position;
        fishingRod.transform.rotation = handTransform.rotation;

        rodRigidbody.isKinematic = false;
        rodRigidbody.useGravity = true;
        rodRigidbody.velocity = Vector3.zero;
        rodRigidbody.angularVelocity = Vector3.zero;

        Vector3 throwDirection = transform.forward * throwForce + Vector3.up * throwUpwardForce;
        rodRigidbody.AddForce(throwDirection, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        // Draw ground check sphere
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && isJumping)
        {
            isJumping = false;
            animator.SetTrigger(LandHash);
        }
    }
}