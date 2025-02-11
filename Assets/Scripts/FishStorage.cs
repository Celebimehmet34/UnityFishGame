using System.Collections.Generic;
using UnityEngine;

public class FishStorage : MonoBehaviour
{
    public Transform crateTransform; // Balýklarýn ekleneceði kasa objesi
    public int maxSlots = 10; // Kasada kaç balýk olabilir?
    public float xSpacing = 0.5f; // X ekseninde mesafe
    public float zSpacing = 0.5f; // Z ekseninde mesafe

    private List<GameObject> fishSlots = new List<GameObject>();
    private int rowSize = 4; // X ekseninde en fazla 4 balýk

    public void AddFish(GameObject fish)
    {
        if (fishSlots.Count < maxSlots)
        {
            fish.transform.SetParent(crateTransform);
            fish.transform.localPosition = GetSlotPosition(fishSlots.Count);
            fish.transform.localRotation = Quaternion.Euler(90, 0, 0); // X ekseninde döndürme
            fishSlots.Add(fish);
            Debug.Log($"Balýk kasaya eklendi! Toplam: {fishSlots.Count}");
        }
        else
        {
            Debug.Log("Kasa dolu!");
        }
    }

    Vector3 GetSlotPosition(int index)
    {
        int xIndex = index % rowSize;
        int zIndex = index / rowSize;
        return new Vector3(xIndex * xSpacing, 0, zIndex * zSpacing);
    }
}
