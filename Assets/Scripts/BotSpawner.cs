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
                yield return new WaitForSeconds(spawnInterval); // Eðer balýk yoksa bekle
                continue; // Kasada balýk yoksa bot spawn etmeyelim
            }

            GameObject newBot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f); // Bot biraz gecikmeli baþlasýn
            Debug.Log("bot oluþturuldu");
            BotManager botManager = newBot.GetComponent<BotManager>();
            if (botManager != null)
            {
                botManager.InitializeBot();
            }
            else
            {
                Debug.LogError("BotManager bileþeni bulunamadý! Prefabý kontrol edin.");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool AreFishInAnyKasa()
    {
        GameObject[] kasalar = GameObject.FindGameObjectsWithTag("pallet");

        foreach (GameObject kasa in kasalar)
        {
            // Kasaya baðlý tüm alt objeler içinde "Fish" tag'ine sahip olanlarý ara
            Transform[] fishObjects = kasa.GetComponentsInChildren<Transform>();

            foreach (Transform fish in fishObjects)
            {
                if (fish.CompareTag("Fish")) // Tag'i "Fish" olan bir obje bulunduðunda
                {
                    return true;
                }
            }
        }

        return false; // Hiç "Fish" tag'li obje yoksa
    }

}
