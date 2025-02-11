using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public GameObject botPrefab;
    public Transform spawnPoint;
    private float spawnInterval = 10f;

    void Start()
    {
        StartCoroutine(SpawnBots());
    }

    IEnumerator SpawnBots()
    {
        while (true)
        {
            bool flag = AreFishInAnyKasa();
            Debug.Log("flag : " + flag);
            if (!flag)
            {
                yield return new WaitForSeconds(spawnInterval); // E�er bal�k yoksa bekle
                continue; // Kasada bal�k yoksa bot spawn etmeyelim
            }

            GameObject newBot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f); // Bot biraz gecikmeli ba�las�n
            Debug.Log("bot olu�turuldu");
            BotManager botManager = newBot.GetComponent<BotManager>();
            if (botManager != null)
            {
                botManager.InitializeBot();
            }
            else
            {
                Debug.LogError("BotManager bile�eni bulunamad�! Prefab� kontrol edin.");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool AreFishInAnyKasa()
    {
        GameObject[] kasalar = GameObject.FindGameObjectsWithTag("pallet");

        foreach (GameObject kasa in kasalar)
        {
            // Kasaya ba�l� t�m alt objeler i�inde "Fish" tag'ine sahip olanlar� ara
            Transform[] fishObjects = kasa.GetComponentsInChildren<Transform>();

            foreach (Transform fish in fishObjects)
            {
                if (fish.CompareTag("Fish")) // Tag'i "Fish" olan bir obje bulundu�unda
                {
                    return true;
                }
            }
        }

        return false; // Hi� "Fish" tag'li obje yoksa
    }

}
