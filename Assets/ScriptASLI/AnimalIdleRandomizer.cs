using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimalIdleRandomizer : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Nama parameter float untuk Cycle Offset (jika pakai).")]
    public string offsetParameterName = "OffsetLompat";

    void Start()
    {
        Animator anim = GetComponent<Animator>();

        // 🔥 SINKRONISASI AMAN: Pastikan Animator ada DAN memiliki Animator Controller aktif
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            // 1. Mengacak parameter float bawaan (Cek juga apakah parameternya valid di Animator)
            if (!string.IsNullOrEmpty(offsetParameterName) && HasParameter(anim, offsetParameterName))
            {
                anim.SetFloat(offsetParameterName, Random.Range(0f, 1f));
            }

            // 2. Paksa Animator melompat ke frame acak di State mana pun yang sedang aktif
            // Sekarang aman dieksekusi karena runtimeAnimatorController sudah dipastikan TIDAK NULL
            anim.Play(0, -1, Random.Range(0f, 1f));
        }
        else
        {
            // Opsional: Memberi tahu developer di log objek mana yang kosongan
            Debug.LogWarning($"[IdleRandomizer] Objek '{gameObject.name}' dilewati karena tidak memiliki Animator Controller di Inspector.", gameObject);
        }

        // Setelah mengacak, script ini otomatis menghancurkan dirinya sendiri (Self Destruct)
        Destroy(this);
    }

    // Fungsi pembantu untuk mengecek apakah sebuah parameter float benar-benar eksis di Animator Controller
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}