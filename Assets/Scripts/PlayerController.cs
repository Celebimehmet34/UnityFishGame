using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 5f;

    // Animator parametreleri i�in string'ler
    private const string IS_WALKING = "isWalking";
    private const string IS_WALKING_BACKWARD = "isWalkingBackward";

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Animator bile�eninin varl���n� kontrol et
        if (animator == null)
        {
            Debug.LogError("Animator bile�eni bulunamad�!");
            return;
        }

        rb.freezeRotation = true;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Debug i�in input de�erlerini g�ster
        Debug.Log($"Vertical Input: {verticalInput}");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);

        rb.velocity = new Vector3(
            movement.normalized.x * moveSpeed,
            rb.velocity.y,
            movement.normalized.z * moveSpeed
        );

        UpdateAnimations(verticalInput);

        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    void UpdateAnimations(float verticalInput)
    {
        bool isWalking = verticalInput > 0;
        bool isWalkingBackward = verticalInput < 0;

        // Animator parametrelerinin de�i�imini logla
        Debug.Log($"isWalking: {isWalking}, isWalkingBackward: {isWalkingBackward}");

        animator.SetBool(IS_WALKING, isWalking);
        animator.SetBool(IS_WALKING_BACKWARD, isWalkingBackward);

        // Parametre de�erlerinin do�ru ayarland���n� kontrol et
        Debug.Log($"Animator isWalking parameter: {animator.GetBool(IS_WALKING)}");
        Debug.Log($"Animator isWalkingBackward parameter: {animator.GetBool(IS_WALKING_BACKWARD)}");
    }
}