using UnityEngine;

public class ReverseAnimation : MonoBehaviour
{
    public Animator animator;
    public string animationName = "YourAnimation";  // Animasyon ad�
    private bool isReversed = false; // Animasyon ters mi oynat�l�yor kontrol�
    private bool isPlaying = false; // Animasyon oynuyor mu?

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // E�er animasyon oynuyorsa
        if (isPlaying)
        {
            // Animasyon tamamland�ysa
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
            {
                isPlaying = false;
                StartNextAnimation(); // Animasyon bitti�inde bir sonraki animasyonu ba�lat
            }
        }
        else
        {
            // E�er animasyon oynuyorsa, tekrar ba�latma
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                isPlaying = true;
                StartNextAnimation(); // Bir sonraki animasyonu ba�lat
            }
        }
    }

    // Animasyonu ba�lat
    void StartNextAnimation()
    {
        if (isReversed)
        {
            // Ters animasyon ba�lat
            animator.Play(animationName, 0, 1);  // Animasyonu bitmi�ten ba�lat
        }
        else
        {
            // D�z animasyon ba�lat
            animator.Play(animationName, 0, 0);  // Animasyonu ba�tan ba�lat
        }

        // Y�n� de�i�tir
        isReversed = !isReversed;
    }
}
