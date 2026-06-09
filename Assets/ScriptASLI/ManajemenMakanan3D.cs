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

    // ====== TAMBAHAN BARU UNTUK POP-UP ======
    [Header("UI Pop-up Peringatan (2D Canvas)")]
    [SerializeField] private GameObject panelPakanHabisPopUp; 
    // ========================================

    [Header("Pengaturan Waktu (Detik)")]
    [SerializeField] private float waktuMakanTotal = 10f; 

    private float waktuBerjalan;
    private bool makananHabis = false;
    private bool pakanPertamaSudahDimasukan = false; 

    void Start()
    {
        SetPakanAwalKosong();
        // Pastikan pop-up tertutup saat game mulai
        if (panelPakanHabisPopUp != null) panelPakanHabisPopUp.SetActive(false);
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

    public void MasukinPakanPertama()
    {
        if (InventoryManager.instance != null && InventoryManager.instance.GunakanPakanDiWorld())
        {
            MulaiSapiMakan();
            Debug.Log("[PakanSapi] Player memasukkan pakan. Sisa di inventory: " + InventoryManager.instance.pakanRumputCount);
        }
        else
        {
            Debug.LogWarning("[PakanSapi] Gagal memberi makan! Pakan di Inventory HABIS.");
            
            // 🔥 MODIFIKASI: Munculkan Panel Pop-up 2D
            TampilkanPopUpPakanHabis();
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyIsiPakanWorld3D();
    }

    public void IsiUlangMakananHabis()
    {
        if (InventoryManager.instance != null && InventoryManager.instance.GunakanPakanDiWorld())
        {
            MulaiSapiMakan();
            Debug.Log("[PakanSapi] Player mengisi ulang pakan. Sisa di inventory: " + InventoryManager.instance.pakanRumputCount);
        }
        else
        {
            Debug.LogWarning("[PakanSapi] Gagal isi ulang! Pakan di Inventory HABIS.");
            
            // 🔥 MODIFIKASI: Munculkan Panel Pop-up 2D
            TampilkanPopUpPakanHabis();
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyIsiPakanWorld3D();
    }

    private void SetMakananHabis()
    {
        makananHabis = true;
        GantiVisual(null);

        if (btnMasukinPakan != null) btnMasukinPakan.SetActive(false);
        if (btnIsiUlang != null) btnIsiUlang.SetActive(true);

        Debug.Log("Makanan sapi telah habis! Tombol Isi Ulang Muncul.");

        // 🔥 HUBUNGKAN KE REMINDER: Pemicu kakek mulai muncul berulang
        if (ReminderManager.instance != null)
        {
            ReminderManager.instance.TriggerPakanHabisReminder(true);
        }
    }

    private void MulaiSapiMakan()
    {
        waktuBerjalan = 0f;
        makananHabis = false;
        pakanPertamaSudahDimasukan = true; 

        GantiVisual(daunFase1Penuh);

        if (btnMasukinPakan != null) btnMasukinPakan.SetActive(false);
        if (btnIsiUlang != null) btnIsiUlang.SetActive(false);

        // 🔥 HUBUNGKAN KE REMINDER: Hentikan kakek karena pakan sudah diisi ulang
        if (ReminderManager.instance != null)
        {
            ReminderManager.instance.TriggerPakanHabisReminder(false);
        }
    }

    // ====== FUNGSI BARU UNTUK KONTROL POP-UP (BISA DIPANGGIL DARI TOMBOL SILANG) ======
    public void TampilkanPopUpPakanHabis()
    {
        if (panelPakanHabisPopUp != null)
        {
            panelPakanHabisPopUp.SetActive(true);
        }
    }

    public void TutupPopUpPakanHabis()
    {
        if (panelPakanHabisPopUp != null)
        {
            panelPakanHabisPopUp.SetActive(false);
        }
    }
}