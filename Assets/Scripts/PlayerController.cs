using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 5f;

    // Animator parametreleri için string'ler
    private const string IS_WALKING = "isWalking";
    private const string IS_WALKING_BACKWARD = "isWalkingBackward";

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Animator bileþeninin varlýðýný kontrol et
        if (animator == null)
        {
            Debug.LogError("Animator bileþeni bulunamadý!");
            return;
        }

        rb.freezeRotation = true;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Debug için input deðerlerini göster
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

        // Animator parametrelerinin deðiþimini logla
        Debug.Log($"isWalking: {isWalking}, isWalkingBackward: {isWalkingBackward}");

        animator.SetBool(IS_WALKING, isWalking);
        animator.SetBool(IS_WALKING_BACKWARD, isWalkingBackward);

        // Parametre deðerlerinin doðru ayarlandýðýný kontrol et
        Debug.Log($"Animator isWalking parameter: {animator.GetBool(IS_WALKING)}");
        Debug.Log($"Animator isWalkingBackward parameter: {animator.GetBool(IS_WALKING_BACKWARD)}");
    }
}