using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class FishData
{
    public string fishName;
    public int crateIndex;
}

[System.Serializable]
public class FishSaveData
{
    public List<FishData> fishes = new List<FishData>();
}


public class FishStorageTest : MonoBehaviour
{
    public GameObject[] fishPrefabs; // 3 �e�it bal�k prefab�
    public Transform[] crateTransforms; // 3 farkl� kasan�n transformlar�
    private int maxSlots = 8; // Kasada ka� bal�k olabilir?

    private Dictionary<int, List<GameObject>> crateFishSlots = new Dictionary<int, List<GameObject>>(); // Kasa i�indeki bal�klar� takip et
    private int rowSize = 4; // X ekseninde en fazla 4 bal�k

    private GameObject hookedFish; // Oltaya tak�lan bal�k
    public bool isHooked = false; // Bal�k oltaya tak�ld� m�?

    private string saveFilePath;

    void Start()
    {
        // Kasalar i�in bal�k listelerini ba�lat
        for (int i = 0; i < crateTransforms.Length; i++)
        {
            crateFishSlots[i] = new List<GameObject>();
        }
        saveFilePath = Path.Combine(Application.persistentDataPath, "fishData.json");

        LoadFishData();
    }



    public void HookFish(GameObject fish)
    {
        hookedFish = fish;
        isHooked = true;
        Debug.Log("Bal�k oltaya tak�ld�!");
    }

    public void StoreFishInCrate(GameObject fish)
    {
        Debug.Log("bu fonksiyon �al��t�r�ld�");
        int crateIndex = GetCrateIndex(fish);
        Transform selectedCrate = crateTransforms[crateIndex];

        if (crateFishSlots[crateIndex].Count < maxSlots) // O kasan�n i�indeki bal�klar� takip eden listeyi kontrol et
        {
            fish.transform.SetParent(selectedCrate);
            fish.transform.localPosition = GetSlotPosition(crateFishSlots[crateIndex].Count); // Listeyi baz alarak pozisyon hesapla
            fish.transform.localRotation = Quaternion.Euler(90, -90, 0);

            // Rigidbody bile�eni varsa sabitle
            Rigidbody rb = fish.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            FishMovement fishMovement = fish.GetComponent<FishMovement>();
            if (fishMovement != null)
            {
                fishMovement.enabled = false;
            }

            // Bal��� listeye ekle
            crateFishSlots[crateIndex].Add(fish);
            fish.SetActive(true);
            Debug.Log($"Bal�k kasaya yerle�tirildi! Kasa: {selectedCrate.name}, Pozisyon: {crateFishSlots[crateIndex].Count}");
            isHooked = false;
            hookedFish = null;
        }
        else
        {
            Debug.Log("Bu kasa dolu, bal��� ba�ka bir kasaya yerle�tir.");
        }
    }

    Vector3 GetSlotPosition(int index)
    {
        float xSpacing = 0.24f;
        float zSpacing = 0.5f;

        float xIndex = index % rowSize;
        float zIndex = index / rowSize;

        return new Vector3(xIndex * xSpacing - 0.36f, 0.1f, -(zIndex * zSpacing - 0.23f));
    }

    int GetCrateIndex(GameObject fish)
    {
        Debug.Log(fish.name);
        if (fish.name.StartsWith("FishV1"))
            return 0;
        else if (fish.name.StartsWith("FishV2"))
            return 1;
        else
            return 2;
    }
    void SaveFishData()
    {
        FishSaveData saveData = new FishSaveData();
        foreach (var crate in crateFishSlots)
        {
            foreach (var fish in crate.Value)
            {
                if (fish != null)
                {
                    saveData.fishes.Add(new FishData { fishName = fish.name, crateIndex = crate.Key });
                }
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Bal�k verileri kaydedildi: " + saveFilePath);
    }

    void LoadFishData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            FishSaveData saveData = JsonUtility.FromJson<FishSaveData>(json);

            foreach (var fishData in saveData.fishes)
            {
                GameObject fishPrefab = FindFishPrefabByName(fishData.fishName);
                if (fishPrefab != null)
                {
                    GameObject fish = Instantiate(fishPrefab);
                    fish.name = fishData.fishName; // Do�ru e�le�meyi sa�lamak i�in ismini ayarla

                    // StoreFishInCrate fonksiyonuna g�nder
                    StoreFishInCrate(fish);
                }
            }
            Debug.Log("Bal�k verileri y�klendi.");
        }
        else
        {
            Debug.Log("Kaydedilmi� bal�k verisi bulunamad�.");
        }
    }


    GameObject FindFishPrefabByName(string name)
    {
        foreach (GameObject prefab in fishPrefabs)
        {
            if (name.StartsWith(prefab.name))
            {
                return prefab;
            }
        }
        return null;
    }
    void OnApplicationQuit()
    {
        SaveFishData();
    }
}
