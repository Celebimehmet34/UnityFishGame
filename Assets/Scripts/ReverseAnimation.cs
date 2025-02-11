using UnityEngine;

public class ReverseAnimation : MonoBehaviour
{
    public Animator animator;
    public string animationName = "YourAnimation";  // Animasyon adý
    private bool isReversed = false; // Animasyon ters mi oynatýlýyor kontrolü
    private bool isPlaying = false; // Animasyon oynuyor mu?

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Eðer animasyon oynuyorsa
        if (isPlaying)
        {
            // Animasyon tamamlandýysa
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
            {
                isPlaying = false;
                StartNextAnimation(); // Animasyon bittiðinde bir sonraki animasyonu baþlat
            }
        }
        else
        {
            // Eðer animasyon oynuyorsa, tekrar baþlatma
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                isPlaying = true;
                StartNextAnimation(); // Bir sonraki animasyonu baþlat
            }
        }
    }

    // Animasyonu baþlat
    void StartNextAnimation()
    {
        if (isReversed)
        {
            // Ters animasyon baþlat
            animator.Play(animationName, 0, 1);  // Animasyonu bitmiþten baþlat
        }
        else
        {
            // Düz animasyon baþlat
            animator.Play(animationName, 0, 0);  // Animasyonu baþtan baþlat
        }

        // Yönü deðiþtir
        isReversed = !isReversed;
    }
}
