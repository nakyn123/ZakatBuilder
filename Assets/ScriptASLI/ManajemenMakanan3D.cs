using UnityEngine;

public class ManajemenMakanan3D : MonoBehaviour
{
    [Header("Aset Daun 3D (3 Fase)")]
    [SerializeField] private GameObject daunFase1Penuh;
    [SerializeField] private GameObject daunFase2Berkurang;
    [SerializeField] private GameObject daunFase3Sedikit;

    [Header("Objek Peringatan 2D (Sprite)")]
    [SerializeField] private GameObject spriteWarningCaution;

    [Header("Pengaturan Waktu (Detik)")]
    [SerializeField] private float waktuMakanTotal = 10f; 

    private float waktuBerjalan;
    private bool makananHabis = false;

    void Start()
    {
        // Kondisi awal: Hanya daun fase 1 (penuh) yang aktif
        ResetFaseVisual();
        waktuBerjalan = 0f;
    }

    void Update()
    {
        // Jika makanan sudah habis, stop logika update berkurang
        if (makananHabis) return;

        // Waktu terus berjalan seiring sapi makan
        waktuBerjalan += Time.deltaTime;

        // Hitung persentase sisa makanan (1.0 turun ke 0.0)
        float persentaseMakanan = 1f - (waktuBerjalan / waktuMakanTotal);

        if (persentaseMakanan <= 0f)
        {
            SetMakananHabis();
        }
        else if (persentaseMakanan <= 0.33f)
        {
            // FASE 3: Sisa Sedikit (33% kebawah menuju 0%)
            GantiVisual(daunFase3Sedikit);
        }
        else if (persentaseMakanan <= 0.66f)
        {
            // FASE 2: Berkurang Setengah (66% kebawah menuju 33%)
            GantiVisual(daunFase2Berkurang);
        }
        else
        {
            // FASE 1: Penuh (Diatas 66% hingga 100%)
            GantiVisual(daunFase1Penuh);
        }
    }

    // Fungsi pembantu agar perpindahan visual antar objek 3D bersih
    private void GantiVisual(GameObject faseYangAktif)
    {
        if (daunFase1Penuh != null) daunFase1Penuh.SetActive(daunFase1Penuh == faseYangAktif);
        if (daunFase2Berkurang != null) daunFase2Berkurang.SetActive(daunFase2Berkurang == faseYangAktif);
        if (daunFase3Sedikit != null) daunFase3Sedikit.SetActive(daunFase3Sedikit == faseYangAktif);
    }

    private void SetMakananHabis()
    {
        makananHabis = true;

        // Matikan semua model daun 3D
        GantiVisual(null);

        // Munculkan tanda Caution di tempat makan
        if (spriteWarningCaution != null)
        {
            spriteWarningCaution.SetActive(true);
        }

        Debug.Log("Makanan sapi telah habis!");
    }

    private void ResetFaseVisual()
    {
        if (daunFase1Penuh != null) daunFase1Penuh.SetActive(true);
        if (daunFase2Berkurang != null) daunFase2Berkurang.SetActive(false);
        if (daunFase3Sedikit != null) daunFase3Sedikit.SetActive(false);
        if (spriteWarningCaution != null) spriteWarningCaution.SetActive(false);
    }

    // Fungsi otomatis dipicu saat pemain melakukan klik pada objek 2D ini di dunia game
    public void IsiUlangMakanan()
    {
        waktuBerjalan = 0f;
        makananHabis = false;
        ResetFaseVisual();
        Debug.Log("Makanan berhasil diisi ulang!");
    }
}