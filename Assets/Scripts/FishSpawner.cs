using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Prefabs")]
    public GameObject[] fishPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;  // Her kaç saniyede bir spawn olacak
    public int maxFishCount = 40;     // Maksimum balýk sayýsý
    public Transform waterVolume;      // Su küpü

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
        // Su küpünün boyutlarýný al
        Bounds waterBounds = waterVolume.GetComponent<Collider>().bounds;

        // Rastgele bir pozisyon seç
        Vector3 spawnPos = new Vector3(
            Random.Range(waterBounds.min.x, waterBounds.max.x),
            Random.Range(waterBounds.min.y+0.5f, waterBounds.max.y-0.5f),
            Random.Range(waterBounds.min.z, waterBounds.max.z)
        );

        // Rastgele bir balýk prefabý seç
        int randomFishIndex = Random.Range(0, fishPrefabs.Length);
        GameObject fishPrefab = fishPrefabs[randomFishIndex];

        // Balýðý spawn et
        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        currentFishCount++;
    }

    public void OnFishCaught()
    {
        currentFishCount--;
    }
}
