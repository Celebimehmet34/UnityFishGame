using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BotManager : MonoBehaviour
{
    private GameObject[] kasalar;
    public float speed = 3f;
    private Animator animator;
    private Rigidbody rb;

    private GameObject targetKasa;
    private bool isMoving = false;
    private float stopDistance = 1.5f;
    Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        kasalar = GameObject.FindGameObjectsWithTag("pallet");
        if (kasalar.Length == 0)
        {
            Debug.LogError("Kasalar bulunamadý! 'pallet' tag'li objeleri sahneye ekleyin.");
            return;
        }

        StartCoroutine(DelayedInitialize());
    }

    IEnumerator DelayedInitialize()
    {
        yield return new WaitForSeconds(0.5f);
        InitializeBot();
    }

    public void InitializeBot()
    {
        targetKasa = FindRandomFishKasa();

        if (targetKasa == null)
        {
            Debug.LogWarning("Balýklý kasa bulunamadý! Bot hareket etmeyecek.");
            return;
        }

        isMoving = true;
    }

    void Update()
    {
        if (isMoving && targetKasa != null)
        {
            MoveToKasa();
        }
    }

    void MoveToKasa()
    {
        Vector3 direction = (targetKasa.transform.position - transform.position).normalized;
        Vector3 targetPosition = targetKasa.transform.position - direction * stopDistance;

        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        rb.MovePosition(newPosition);

        transform.LookAt(new Vector3(targetKasa.transform.position.x, transform.position.y, targetKasa.transform.position.z));
        animator.SetFloat("speed", speed);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            isMoving = false;
            animator.SetFloat("speed", 0);
            StartCoroutine(TakeFishFromKasa());
        }
    }

    IEnumerator TakeFishFromKasa()
    {
        yield return new WaitForSeconds(2f);

        List<Transform> fishList = new List<Transform>();
        foreach (Transform child in targetKasa.transform)
        {
            if (child.CompareTag("Fish"))
            {
                fishList.Add(child);
            }
        }

        if (fishList.Count == 0)
        {
            Debug.Log("Bu kasada balýk kalmadý! Bot hemen geri dönüyor.");
            StartCoroutine(ReturnToStart());
            yield break;
        }

        int fishTaken = Random.Range(1, Mathf.Min(3, fishList.Count) + 1);
        Debug.Log($"Bot {fishTaken} balýk aldý!");

        // Balýk türüne göre para miktarlarý
        Dictionary<string, int> fishPrices = new Dictionary<string, int>()
    {
        { "FishV1", 10 },
        { "FishV2", 20 },
        { "FishV3", 30 }
    };

        int totalMoney = 0;

        for (int i = 0; i < fishTaken; i++)
        {
            string fishName = fishList[i].name.Replace("(Clone)", "").Trim(); // Prefab adýndan "(Clone)" kaldýr
            if (fishPrices.TryGetValue(fishName, out int price))
            {
                totalMoney += price;
            }
            else
            {
                Debug.LogWarning($"Fiyat bulunamadý: {fishName}");
            }

            Destroy(fishList[i].gameObject);
        }

        MoneyManager.Instance.AddMoney(totalMoney);
        Debug.Log($"Toplam {totalMoney} para eklendi!");

        yield return new WaitForSeconds(1f);

        StartCoroutine(ReturnToStart());
    }


    IEnumerator ReturnToStart()
    {
        targetKasa = null;
        isMoving = true;

        while (Vector3.Distance(transform.position, startPosition) > 0.5f)
        {
            animator.SetFloat("speed", speed);
            Vector3 direction = (startPosition - transform.position).normalized;
            Vector3 newPosition = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            rb.MovePosition(newPosition);
            transform.LookAt(new Vector3(startPosition.x, transform.position.y, startPosition.z));
            yield return null;
        }

        isMoving = false;
        animator.SetFloat("speed", 0);

        Destroy(gameObject);
    }

    private GameObject FindRandomFishKasa()
    {
        List<GameObject> validKasalar = new List<GameObject>();

        foreach (var kasa in kasalar)
        {
            foreach (Transform child in kasa.transform)
            {
                if (child.CompareTag("Fish"))
                {
                    validKasalar.Add(kasa);
                    break;
                }
            }
        }

        if (validKasalar.Count == 0)
            return null;

        return validKasalar[Random.Range(0, validKasalar.Count)];
    }
}
