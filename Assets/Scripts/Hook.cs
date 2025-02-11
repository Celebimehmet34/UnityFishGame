using Unity.VisualScripting;
using UnityEngine;

public class Hook : MonoBehaviour
{
    private BucketManager fishScript;
    private bool hasCaughtFish = false;  // Bu kancada balýk var mý kontrolü
    

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
            Debug.Log("Balýk yakalandý!");

            // Balýðý yakala ve statik deðiþkeni güncelle
            hasCaughtFish = true;
            FishMovement.isAnyFishHooked = true;

            // Balýk hareketini durdur
            FishMovement fishMovement = collision.gameObject.GetComponent<FishMovement>();
            if (fishMovement != null)
            {
                fishMovement.enabled = false;
            }

            // Balýðý kovaya ekle
            if (fishScript != null)
            {
                fishScript.HoldFish(collision.gameObject);
            }
        }
    }

    // Olta geri çekildiðinde çaðrýlacak metot
    public void OnReelIn()
    {
        hasCaughtFish = false;
        FishMovement.isAnyFishHooked = false;
    }

    // Olta atýldýðýnda çaðrýlacak metot
    public void OnCast()
    {
        hasCaughtFish = false;
        FishMovement.isAnyFishHooked = false;
    }

    // Hook objesi yok edildiðinde çaðrýlacak
    private void OnDestroy()
    {
        // Eðer bu hook yok ediliyorsa, statik deðiþkeni sýfýrla
        if (hasCaughtFish)
        {
            FishMovement.isAnyFishHooked = false;
        }
    }
}