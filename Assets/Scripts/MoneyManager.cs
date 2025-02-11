using System.IO;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;
    private string saveFilePath;
    private int moneyAmount = 0;
    public TMP_Text moneyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/saveData.json";


        LoadMoney();
    }

    public void AddMoney(int amount)
    {
        moneyAmount += amount;
        UpdateMoneyUI();
        SaveMoney();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = moneyAmount.ToString();
        }
    }

    private void SaveMoney()
    {
        MoneyData data = new MoneyData();
        data.moneyAmount = moneyAmount;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Para kaydedildi: " + moneyAmount);
    }

    private void LoadMoney()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            MoneyData data = JsonUtility.FromJson<MoneyData>(json);
            moneyAmount = data.moneyAmount;
            UpdateMoneyUI();
            Debug.Log("Para yüklendi: " + moneyAmount);
        }
        else
        {
            Debug.Log("Kaydedilmiþ para verisi bulunamadý.");
        }
    }

    void OnApplicationQuit()
    {
        SaveMoney();
    }
}

[System.Serializable]
public class MoneyData
{
    public int moneyAmount;
}
