using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimSpeed = 5f;
    public float rotationSpeed = 3f;
    public float wanderRadius = 5f;
    public float detectionRadius = 8f;

    private Vector3 targetPosition;
    private Transform waterVolume;
    private bool isHooked = false;
    private Rigidbody rb;

    // Statik bool de�i�ken - t�m bal�klar i�in ortak
    public static bool isAnyFishHooked = false;
    private HookThrow hookThrow;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        waterVolume = GameObject.FindGameObjectWithTag("Water").transform;
        hookThrow = FindObjectOfType<HookThrow>();
        SetNewRandomTarget();
    }

    private void Update()
    {
        if (isHooked) return;

        GameObject hook = GameObject.FindGameObjectWithTag("Hook");
        if (hook != null && hookThrow != null)
        {
            float distanceToHook = Vector3.Distance(transform.position, hook.transform.position);

            // Olta at�lm�� ve bal�k yakalanmam��sa oltaya git
            if (distanceToHook <= detectionRadius && !isAnyFishHooked)
            {
                targetPosition = hook.transform.position;
            }
            else
            {
                CheckAndUpdateTarget();
            }
        }
        else
        {
            CheckAndUpdateTarget();
        }

        MoveTowardsTarget();
    }

    private void CheckAndUpdateTarget()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            SetNewRandomTarget();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * swimSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void SetNewRandomTarget()
    {
        Bounds waterBounds = waterVolume.GetComponent<Collider>().bounds;
        targetPosition = new Vector3(
            Random.Range(waterBounds.min.x, waterBounds.max.x),
            Random.Range(waterBounds.min.y, waterBounds.max.y),
            Random.Range(waterBounds.min.z, waterBounds.max.z)
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hook") && !isHooked && !isAnyFishHooked)
        {
            isHooked = true;
            isAnyFishHooked = true;
            transform.parent = other.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            FindObjectOfType<FishSpawner>().OnFishCaught();

            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    // Oltadan ��kar�ld���nda �a�r�lacak metot
    public void OnUnhooked()
    {
        isHooked = false;
        transform.parent = null;
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        SetNewRandomTarget(); // Serbest b�rak�ld���nda yeni hedef belirle
    }

    // Olta s�f�rland���nda �a�r�lacak
    public static void ResetHook()
    {
        isAnyFishHooked = false;
    }
}