using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  // Takip edilecek karakter (Player)
    public Vector3 offset = new Vector3(0, 2, -5);  // Kameranýn karaktere olan uzaklýðý
    public float smoothSpeed = 5f;  // Kameranýn hareket yumuþaklýðý

    void LateUpdate()
    {
        if (target == null) return;

        // Hedef pozisyonu hesapla
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Kamera pozisyonunu yumuþak þekilde hedefe yaklaþtýr
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Kameranýn karaktere bakmasýný saðla
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}

