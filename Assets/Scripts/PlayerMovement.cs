using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float move = Input.GetAxis("Vertical");
        rb.velocity = new Vector3(0, rb.velocity.y, move * speed);

        animator.SetFloat("Speed", Mathf.Abs(move));
    }
}
