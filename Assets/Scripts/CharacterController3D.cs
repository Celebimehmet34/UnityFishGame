using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class CharacterController3D : MonoBehaviour
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


    [Header("Fishing Settings")]
    public GameObject fishingRod;
    public Transform handTransform;
    public GameObject hand; 
    public float throwForce = 2f;
    public float throwUpwardForce = 1.5f;
    public GameObject bucket;
    public Transform rightHandTransform;


    [Header("Text Settings")]
    public GameObject PickHook;
    public GameObject fishNumberPanel;
    public GameObject pickBucket;
    public GameObject pressRight;
    public GameObject pressLeft;
    public Image warningImage;
    public TMP_Text warningText;

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

    // Jump state
    private bool isJumping;
    private bool isGrounded;
    private int jumpCount;
    private bool canJump = true;
    private float verticalVelocity;


    // Fishing state
    public bool isFishing;
    private bool isCasting;
    private Rigidbody rodRigidbody;
    private bool isBucket = false;


    private readonly int SpeedHash = Animator.StringToHash("speed");
    private readonly int IsRunningHash = Animator.StringToHash("run");
    private readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private readonly int JumpHash = Animator.StringToHash("jump");
    private readonly int LandHash = Animator.StringToHash("land");
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

    }

    private void InitializeSettings()
    {
        rb.freezeRotation = true;
        rb.useGravity = false;
        currentSpeed = walkSpeed;
        fishNumberPanel.SetActive(false);
        pickBucket.SetActive(false);
        pressLeft.SetActive(false);
        pressRight.SetActive(false);
        warningImage.gameObject.SetActive(false);
        if (rodRigidbody != null)
        {
            rodRigidbody.isKinematic = true;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimationState();
        if (isFishing && fishingRod.transform.parent == handTransform)
        {
            fishingRod.transform.localPosition = new Vector3(0.1f, 0.1f, -0.02f);
            fishingRod.transform.localRotation = Quaternion.Euler(98.6f, -57f, 179f);
        }
        float distanceToBucket = Vector3.Distance(transform.position, bucket.transform.position);
        Vector3 directionToBucket = bucket.transform.position - transform.position;
        float angleToBucket = Vector3.Angle(transform.forward, directionToBucket);
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isBucket)
            {
                DropBucket();
            }
            else if(distanceToBucket <= 1f && angleToBucket < 60f && !isBucket)
            {
                PickBucket();
            }
            
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
    }


    private void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {

            TryJump();
        }
        float distanceToBucket = Vector3.Distance(transform.position, bucket.transform.position);
        Vector3 directionToBucket = bucket.transform.position - transform.position;
        float angleToBucket = Vector3.Angle(transform.forward , directionToBucket);
        if (distanceToBucket <= 1f && angleToBucket < 60f && !isBucket)
        {
            pickBucket.SetActive(true);
        }
        else
        {
            pickBucket.SetActive(false);
        }
            
        if (isFishing)
        {
            PickHook.SetActive(false);
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleFishing();
            }
            HandleFishingInput();
        }
        else
        {
            float distanceToRod = Vector3.Distance(transform.position, fishingRod.transform.position);
            float distanceThreshold = 1f;
            Vector3 directionToRod = fishingRod.transform.position - transform.position;
            float angleToRod = Vector3.Angle(transform.forward, directionToRod);
            if(distanceToRod <= distanceThreshold && angleToRod < 60f)
            {
                if (!isFishing)
                {
                    PickHook.SetActive(true);
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ToggleFishing();
                }
            }
            else
            {
                PickHook.SetActive(false);
            }

            
        }
    }

    private void HandleMovement()
    {

        if (isCasting) return;

        moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        bool isMoving = moveDirection.magnitude > 0.1f;

        if (isMoving)
        {
            if (verticalInput != 0)
            {
                float acceleration = 0.00001f;
                float maxSpeed = 10f;
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.fixedDeltaTime)/2f;

                Vector3 moveVelocity = transform.forward * currentSpeed * Mathf.Sign(verticalInput);
                rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

                float animationSpeed = (rb.velocity.magnitude / runSpeed) * Mathf.Sign(verticalInput);
                animator.SetFloat(SpeedHash, animationSpeed);

                if (horizontalInput != 0)
                {
                    transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                }
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);

                if (horizontalInput != 0)
                {
                    transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                }

                animator.SetFloat(SpeedHash, 0);
            }
        }
        else
        {
            if (horizontalInput != 0)
            {
                transform.Rotate(Vector3.up * horizontalInput * rotationSpeed * Time.fixedDeltaTime);
                animator.SetFloat(SpeedHash, 0);
            }

            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }



    private void TryJump()
    {
        Debug.Log(isGrounded);
        Debug.Log("çaðýrýldý");
        

        if (isGrounded)
        {
            animator.SetTrigger(JumpHash);
            Invoke("ExecuteJump", 0.5f);
            
        }
    }

    private void ExecuteJump()
    {
        
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

    }


    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity = Mathf.Max(rb.velocity.y + gravity * Time.fixedDeltaTime, terminalVelocity);
            rb.velocity = new Vector3(rb.velocity.x, verticalVelocity, rb.velocity.z);
        }
    }


    private void UpdateAnimationState()
    {
        float normalizedSpeed = rb.velocity.magnitude / runSpeed * Mathf.Sign(verticalInput);
        if (verticalInput == 0)
        {
            animator.SetFloat(SpeedHash, 0);
        }
        else
        {
            animator.SetFloat(SpeedHash, normalizedSpeed);
        }

        animator.SetBool(IsRunningHash, isRunning && normalizedSpeed > 0.1f);
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
            if (!isBucket)
            {
                StartCasting();
            }
            else
            {
                warningText.text = "You can't angling with bucket";
                ShowWarning();
            }
            
        }
        else if (Input.GetMouseButtonDown(0) && isCasting && !isBucket)
        {
            if (!isBucket)
            {
                StartReeling();
            }
            else
            {
                warningText.text = "You can't reel rod with bucket";
                ShowWarning();
            }
            
        }
    }

    private void PickBucket()
    {
        bucket.transform.SetParent(rightHandTransform);
        bucket.transform.localPosition = new Vector3(0.019f, 0.455f, -0.004f);
        bucket.transform.localRotation = Quaternion.Euler(177f, -3.5f, 3.8f);
        isBucket = true;
        fishNumberPanel.SetActive(true);
    }

    private void DropBucket()
    {
        bucket.transform.SetParent(null);
        Vector3 dropPosition = transform.position +
                               transform.forward * 0.3f +
                               transform.right * 0.5f +
                               Vector3.up * 0f;

        bucket.transform.position = dropPosition;

        bucket.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        isBucket = false;
        fishNumberPanel.SetActive(false);
    }

    private void StartFishing()
    {
        isFishing = true;
        pressRight.SetActive(true);
        fishingRod.transform.SetParent(handTransform);
        fishingRod.transform.localPosition = new Vector3(0.1f, 0.1f, -0.02f);
        fishingRod.transform.localRotation = Quaternion.Euler(98.6f, -57f, 179f);
        Collider rodCollider = fishingRod.GetComponent<Collider>();
        if (rodCollider != null)
        {
            rodCollider.enabled = false;
        }

        fishingRod.transform.position = handTransform.position + handTransform.forward * 0.5f;


        animator.SetBool(FishingIdleHash, true);
    }

    private void StartCasting()
    {
        isCasting = true;
        animator.SetBool(FishingHash, true);
        animator.SetBool(FishingIdleHash, false);
        animator.SetBool(FishingHoldHash, false);
        pressRight.SetActive(false);
        pressLeft.SetActive(true);
    }

    private void StartReeling()
    {
        isCasting = false;
        animator.SetBool(FishingHash, false);
        animator.SetBool(FishingHoldHash, true);
        pressRight.SetActive(true);
        pressLeft.SetActive(false);
    }

    private void StopFishing()
    {
        Collider rodCollider = fishingRod.GetComponent<Collider>();
        if (rodCollider != null)
        {
            rodCollider.enabled = true;
        }
        fishingRod.transform.SetParent(null);


        Vector3 rodPosition = transform.position + transform.forward * 0.6f + transform.right * -0.2f;
        fishingRod.transform.position = rodPosition;


        fishingRod.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);


        animator.SetBool(FishingHash, false);
        animator.SetBool(FishingIdleHash, false);
        animator.SetBool(FishingHoldHash, false);
        pressRight.SetActive(false);
        pressLeft.SetActive(false);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Ground") )
        {
            isGrounded = true;
            isJumping = false;
            animator.SetTrigger(LandHash);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            isJumping = true;
            animator.SetTrigger(LandHash);
            animator.SetBool("jump" , false);
        }
    }

    public void ShowWarning()
    {
        StartCoroutine(FadeOutWarning());
    }

    private IEnumerator FadeOutWarning()
    {
        warningImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color imageColor = warningImage.color;
        Color textColor = warningText.color;

        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2f); 

            warningImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, alpha);
            warningText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            yield return null;
        }


        warningImage.gameObject.SetActive(false);
    }
}