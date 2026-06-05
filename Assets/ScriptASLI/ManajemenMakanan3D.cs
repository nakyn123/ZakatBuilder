using UnityEngine;

public class ManajemenMakanan3D : MonoBehaviour
{
    [Header("Aset Daun 3D (3 Fase)")]
    [SerializeField] private GameObject daunFase1Penuh;
    [SerializeField] private GameObject daunFase2Berkurang;
    [SerializeField] private GameObject daunFase3Sedikit;

    [Header("Tombol Interaksi Dunia (3D/World)")]
    [SerializeField] private GameObject btnMasukinPakan; // Tombol awal (saat pakan kosong)
    [SerializeField] private GameObject btnIsiUlang;     // Tombol saat makanan habis

    [Header("Pengaturan Waktu (Detik)")]
    [SerializeField] private float waktuMakanTotal = 10f; 

    private float waktuBerjalan;
    private bool makananHabis = false;
    private bool pakanPertamaSudahDimasukan = false; 

    void Start()
    {
        SetPakanAwalKosong();
    }

    void Update()
    {
        if (!pakanPertamaSudahDimasukan || makananHabis) return;

        waktuBerjalan += Time.deltaTime;
        float persentaseMakanan = 1f - (waktuBerjalan / waktuMakanTotal);

        if (persentaseMakanan <= 0f)
        {
            SetMakananHabis();
        }
        else if (persentaseMakanan <= 0.33f)
        {
            GantiVisual(daunFase3Sedikit);
        }
        else if (persentaseMakanan <= 0.66f)
        {
            GantiVisual(daunFase2Berkurang);
        }
        else
        {
            GantiVisual(daunFase1Penuh);
        }
    }

    private void GantiVisual(GameObject faseYangAktif)
    {
        if (daunFase1Penuh != null) daunFase1Penuh.SetActive(daunFase1Penuh == faseYangAktif);
        if (daunFase2Berkurang != null) daunFase2Berkurang.SetActive(daunFase2Berkurang == faseYangAktif);
        if (daunFase3Sedikit != null) daunFase3Sedikit.SetActive(daunFase3Sedikit == faseYangAktif);
    }

    private void SetPakanAwalKosong()
    {
        pakanPertamaSudahDimasukan = false;
        makananHabis = false;
        GantiVisual(null);

        if (btnMasukinPakan != null) btnMasukinPakan.SetActive(true);
        if (btnIsiUlang != null) btnIsiUlang.SetActive(false);
    }

    private void SetMakananHabis()
    {
        makananHabis = true;
        GantiVisual(null);

        if (btnMasukinPakan != null) btnMasukinPakan.SetActive(false);
        if (btnIsiUlang != null) btnIsiUlang.SetActive(true);

        Debug.Log("Makanan sapi telah habis! Tombol Isi Ulang Muncul.");
    }

    // =================================================================
// COMMAND KHUSUS MASUKIN PAKAN (Tombol Pertama)
// =================================================================
    public void MasukinPakanPertama()
    {
        // Cek ke inventory, jika pakan ada dan berhasil dikurangi 1x
        if (InventoryManager.instance != null && InventoryManager.instance.GunakanPakanDiWorld())
        {
            MulaiSapiMakan();
            Debug.Log("[PakanSapi] Player memasukkan pakan. Sisa di inventory: " + InventoryManager.instance.pakanRumputCount);
        }
        else
        {
            Debug.LogWarning("[PakanSapi] Gagal memberi makan! Pakan di Inventory HABIS. Silakan beli di toko.");
            // Opsi tambahan: kamu bisa munculin pop-up tulisan "Pakan Habis!" di layar di sini
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyIsiPakanWorld3D();
    }

    // =================================================================
    // COMMAND KHUSUS ISI ULANG (Tombol Kedua)
    // =================================================================
    public void IsiUlangMakananHabis()
    {
        // Logikanya sama, potong 1 kuota pakan lagi dari inventory untuk isi ulang
        if (InventoryManager.instance != null && InventoryManager.instance.GunakanPakanDiWorld())
        {
            MulaiSapiMakan();
            Debug.Log("[PakanSapi] Player mengisi ulang pakan. Sisa di inventory: " + InventoryManager.instance.pakanRumputCount);
        }
        else
        {
            Debug.LogWarning("[PakanSapi] Gagal isi ulang! Pakan di Inventory HABIS.");
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyIsiPakanWorld3D();
    }

    // Fungsi pembantu internal untuk menyatukan logika reset visual & waktu
    private void MulaiSapiMakan()
    {
        waktuBerjalan = 0f;
        makananHabis = false;
        pakanPertamaSudahDimasukan = true; 

        GantiVisual(daunFase1Penuh);

        // Sembunyikan kedua tombol di dunia saat pakan sedang tersedia
        if (btnMasukinPakan != null) btnMasukinPakan.SetActive(false);
        if (btnIsiUlang != null) btnIsiUlang.SetActive(false);
    }
}