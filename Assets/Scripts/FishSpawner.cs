using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Prefabs")]
    public GameObject[] fishPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;  // Her ka� saniyede bir spawn olacak
    public int maxFishCount = 40;     // Maksimum bal�k say�s�
    public Transform waterVolume;      // Su k�p�

    private float nextSpawnTime;
    private int currentFishCount;

    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime && currentFishCount < maxFishCount)
        {
            SpawnFish();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnFish()
    {
        // Su k�p�n�n boyutlar�n� al
        Bounds waterBounds = waterVolume.GetComponent<Collider>().bounds;

        // Rastgele bir pozisyon se�
        Vector3 spawnPos = new Vector3(
            Random.Range(waterBounds.min.x, waterBounds.max.x),
            Random.Range(waterBounds.min.y+0.5f, waterBounds.max.y-0.5f),
            Random.Range(waterBounds.min.z, waterBounds.max.z)
        );

        // Rastgele bir bal�k prefab� se�
        int randomFishIndex = Random.Range(0, fishPrefabs.Length);
        GameObject fishPrefab = fishPrefabs[randomFishIndex];

        // Bal��� spawn et
        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        currentFishCount++;
    }

    public void OnFishCaught()
    {
        currentFishCount--;
    }
}
