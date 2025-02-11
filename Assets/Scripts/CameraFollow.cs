using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  // Takip edilecek karakter (Player)
    public Vector3 offset = new Vector3(0, 2, -5);  // Kameran�n karaktere olan uzakl���
    public float smoothSpeed = 5f;  // Kameran�n hareket yumu�akl���

    void LateUpdate()
    {
        if (target == null) return;

        // Hedef pozisyonu hesapla
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Kamera pozisyonunu yumu�ak �ekilde hedefe yakla�t�r
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Kameran�n karaktere bakmas�n� sa�la
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}

