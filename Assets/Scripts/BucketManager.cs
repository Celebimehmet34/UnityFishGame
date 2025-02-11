using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class BucketFishData
{
    public string fishName;
    public Vector3 localPosition;
    public Vector3 localScale;
}

[System.Serializable]
public class BucketFishSaveData
{
    public List<BucketFishData> fishList = new List<BucketFishData>();
}

public class BucketManager : MonoBehaviour
{
    private List<GameObject> fishList = new List<GameObject>();
    private int maxFishCount = 5;
    public TMP_Text fishCountText;
    private FishStorageTest fishScript;
    private GameObject heldFish = null;
    private Vector3 fishHoldPosition;
    private bool isFishHeld = false;
    private string saveFilePath;

    public GameObject[] fishPrefabs;
    public GameObject fishCaught;
    public GameObject placeFish;
    public GameObject character;
    private void Start()
    {
        fishCaught.SetActive(false);
        placeFish.SetActive(false);
        saveFilePath = Path.Combine(Application.persistentDataPath, "bucketFishData.json");
        GameObject player = GameObject.FindWithTag("Player2");
        if (player != null)
        {
            fishScript = player.GetComponent<FishStorageTest>();
        }
        LoadFishData();
        UpdateFishCountUI();
    }

    private void Update()
    {
        if (isFishHeld && heldFish != null)
        {
            heldFish.transform.position = fishHoldPosition;
            if (Input.GetMouseButtonDown(0))
            {
                if (fishList.Count < maxFishCount)
                {
                    fishList.Add(heldFish);
                    heldFish.SetActive(false);
                    heldFish.transform.SetParent(transform);
                    UpdateFishCountUI();
                    isFishHeld = false;
                    heldFish = null;
                }
                else
                {
                    Debug.Log("Kova dolu! Daha fazla balýk ekleyemezsin.");
                }
            }
        }
        if((character.transform.position.z < -5.5f) && (transform.position.y > 0.2f))
        {
            placeFish.SetActive(true);
            if (fishList.Count > 0 && Input.GetKeyDown(KeyCode.B))
            {
                GameObject fish = fishList[0];
                fish.SetActive(true);
                fishScript.StoreFishInCrate(fish);
                fishList.RemoveAt(0);
                UpdateFishCountUI();
            }
        }
        else
        {
            placeFish.SetActive(false);
        }
        
    }

    public void HoldFish(GameObject fish)
    {
        if (!isFishHeld)
        {
            fishCaught.SetActive(true);
            heldFish = fish;
            fishHoldPosition = fish.transform.position;
            isFishHeld = true;
            fish.SetActive(true);
            fish.transform.position = fishHoldPosition;
        }
        else
        {
            Debug.Log("Zaten bir balýk tutuyorsun! Önce onu kovaya eklemelisin.");
        }
    }

    private void UpdateFishCountUI()
    {
        fishCaught.SetActive(false);
        if (fishCountText != null)
        {
            fishCountText.text = "Balýk: " + fishList.Count + "/" + maxFishCount;
        }
    }

    private void OnApplicationQuit()
    {
        SaveFishData();
    }

    private void SaveFishData()
    {
        BucketFishSaveData saveData = new BucketFishSaveData();
        foreach (GameObject fish in fishList)
        {
            BucketFishData fishData = new BucketFishData()
            {
                fishName = fish.name.Replace("(Clone)", ""),
                localPosition = fish.transform.localPosition,
                localScale = fish.transform.localScale
            };
            saveData.fishList.Add(fishData);
        }
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Saved {saveData.fishList.Count} fish to file.");
    }

    private void LoadFishData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            BucketFishSaveData saveData = JsonUtility.FromJson<BucketFishSaveData>(json);
            Debug.Log($"Loaded {saveData.fishList.Count} fish from file.");

            foreach (BucketFishData fishData in saveData.fishList)
            {
                GameObject fishPrefab = FindFishPrefabByName(fishData.fishName);
                Debug.Log(fishData.fishName);
                if (fishPrefab != null)
                {
                    GameObject newFish = Instantiate(fishPrefab, transform);
                    newFish.transform.localPosition = fishData.localPosition;
                    newFish.transform.localScale = fishData.localScale;
                    newFish.SetActive(false);
                    newFish.tag = "Fish";
                    fishList.Add(newFish);
                }
            }
            UpdateFishCountUI();
        }
    }

    private GameObject FindFishPrefabByName(string name)
    {
        foreach (GameObject prefab in fishPrefabs)
        {
            Debug.Log($"Checking prefab: {prefab.name} against {name}");

            if (name.StartsWith(prefab.name)) // Burayý gerekirse deðiþtirebiliriz
            {
                Debug.Log($"Match found: {prefab.name}");
                return prefab;
            }
        }

        Debug.Log($"No match found for {name}");
        return null;
    }

}
