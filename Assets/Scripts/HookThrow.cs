using UnityEngine;
using UnityEngine.Rendering;

public class HookThrow : MonoBehaviour
{
    public GameObject hookPrefab;
    public Transform hookPoint;
    public GameObject character;
    public GameObject startPoint;
    public float throwForce = 20f;
    public float retractSpeed = 10f;
    public float maxDistance = 15f;
    public int ropeSegments = 20;

    private bool isThrown = false;
    private bool isReturning = false;

    private GameObject newHook = null;
    private LineRenderer lineRenderer;
    private Rigidbody hookRb;
    private Vector3 initialPosition;

    private CharacterController3D script;

    void Start()
    {
        script = character.GetComponent<CharacterController3D>();
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // LineRenderer ayarlarý
        lineRenderer.positionCount = ropeSegments; // Baþlangýçta boþ segmentler var
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.useWorldSpace = true;

        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isThrown && script.isFishing)
        {
            Invoke(nameof(ThrowHook), 2.1f);
        }
        else if (Input.GetMouseButtonDown(0) && isThrown && script.isFishing)
        {
            StartReturningHook();
        }

        if (isThrown && newHook != null)
        {
            UpdateRopePositions();

            if (Vector3.Distance(initialPosition, newHook.transform.position) > maxDistance)
            {
                StartReturningHook();
            }
        }

        if (isReturning && newHook != null)
        {
            newHook.transform.position = Vector3.MoveTowards(newHook.transform.position, startPoint.transform.position, retractSpeed * Time.deltaTime);

            if (Vector3.Distance(newHook.transform.position, startPoint.transform.position) < 0.1f)
            {
                ReturnHook();
            }
        }
    }

    void ThrowHook()
    {
        FishMovement.ResetHook();
        isThrown = true;

        newHook = Instantiate(hookPrefab, hookPoint.position, hookPoint.rotation);
        hookRb = newHook.GetComponent<Rigidbody>();

        if (hookRb == null)
        {
            hookRb = newHook.AddComponent<Rigidbody>();
        }
        Hook hookScript = newHook.GetComponent<Hook>();
        if (hookScript != null)
        {
            hookScript.OnCast();
        }
        hookRb.useGravity = true;
        hookRb.velocity = Vector3.zero;

        initialPosition = hookPoint.position;

        Vector3 throwDirection = character.transform.forward + Vector3.up * 0.2f;
        hookRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
    }

    void StartReturningHook()
    {
        if (newHook != null)
        {
            isReturning = true;
            if (hookRb != null)
            {
                hookRb.velocity = Vector3.zero;
                hookRb.useGravity = false;
            }
        }
    }


    void ReturnHook()
    {
        isThrown = false;
        isReturning = false;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        if (newHook != null)
        {
            // Olta geri çekilirken Hook scriptini bilgilendir
            Hook hookScript = newHook.GetComponent<Hook>();
            if (hookScript != null)
            {
                hookScript.OnReelIn();
            }

            Destroy(newHook);
            newHook = null;
        }
    }
    void UpdateRopePositions()
    {
        if (newHook == null || lineRenderer == null) return;

        lineRenderer.positionCount = ropeSegments;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector3 pointOnRope = Vector3.Lerp(startPoint.transform.position, newHook.transform.position, t);

            // Yerçekimi etkisi ekleyerek daha gerçekçi ip görüntüsü
            pointOnRope.y -= Mathf.Sin(t * Mathf.PI) * 0.75f;

            lineRenderer.SetPosition(i, pointOnRope);
        }
    }
}
