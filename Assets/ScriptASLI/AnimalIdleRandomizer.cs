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

        if (anim != null)
        {
            // 1. Mengacak parameter float bawaan (seperti OffsetLompat kamu)
            if (!string.IsNullOrEmpty(offsetParameterName))
            {
                anim.SetFloat(offsetParameterName, Random.Range(0f, 1f));
            }

            // 2. SOLUSI UTAMA: Paksa Animator melompat ke frame acak di State mana pun yang sedang aktif
            // Angka 0f sampai 1f artinya melompat antara frame awal sampai frame akhir animasi secara acak
            // Angka 0 di tengah adalah index layer (Base Layer = 0)
            anim.Play(0, -1, Random.Range(0f, 1f));
        }

        // Setelah mengacak, script ini otomatis menghancurkan dirinya sendiri (Self Destruct)
        // Agar tidak memakan memori/CPU saat game berjalan, karena tugasnya sudah selesai di awal.
        Destroy(this);
    }
}