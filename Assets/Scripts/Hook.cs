using Unity.VisualScripting;
using UnityEngine;

public class Hook : MonoBehaviour
{
    private BucketManager fishScript;
    private bool hasCaughtFish = false;  // Bu kancada bal�k var m� kontrol�
    

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Bucket");
        if (player != null)
        {
            fishScript = player.GetComponent<BucketManager>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fish") && !hasCaughtFish)
        {
            Debug.Log("Bal�k yakaland�!");

            // Bal��� yakala ve statik de�i�keni g�ncelle
            hasCaughtFish = true;
            FishMovement.isAnyFishHooked = true;

            // Bal�k hareketini durdur
            FishMovement fishMovement = collision.gameObject.GetComponent<FishMovement>();
            if (fishMovement != null)
            {
                fishMovement.enabled = false;
            }

            // Bal��� kovaya ekle
            if (fishScript != null)
            {
                fishScript.HoldFish(collision.gameObject);
            }
        }
    }

    // Olta geri �ekildi�inde �a�r�lacak metot
    public void OnReelIn()
    {
        hasCaughtFish = false;
        FishMovement.isAnyFishHooked = false;
    }

    // Olta at�ld���nda �a�r�lacak metot
    public void OnCast()
    {
        hasCaughtFish = false;
        FishMovement.isAnyFishHooked = false;
    }

    // Hook objesi yok edildi�inde �a�r�lacak
    private void OnDestroy()
    {
        // E�er bu hook yok ediliyorsa, statik de�i�keni s�f�rla
        if (hasCaughtFish)
        {
            FishMovement.isAnyFishHooked = false;
        }
    }
}