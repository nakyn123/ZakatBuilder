using UnityEngine;

public class Level3Manager : MonoBehaviour
{
    public static Level3Manager instance;

    [Header("Environment Level 3 Only")]
    public GameObject environmentLevel3; 

    [Header("Lock System Visuals")]
    [SerializeField] private GameObject barrierLevel3; 

    [Header("UI Level 3 References")]
    public GameObject panelRewardEmasPerak; 
    public GameObject conversionPanel;
    public GameObject txtPeringatanWilayahObj;
    [HideInInspector] public bool isBabak3Aktif = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (barrierLevel3 != null) barrierLevel3.SetActive(true); 
        if (txtPeringatanWilayahObj != null) txtPeringatanWilayahObj.SetActive(false);
    }

    public void SwitchToLevel3()
    {
        Debug.Log("<color=green>[Level 3 Manager]</color> Membuka wilayah Babak 3! Menghilangkan pembatas gaib...");

        isBabak3Aktif = true;

        if (barrierLevel3 != null) barrierLevel3.SetActive(false);
        if (environmentLevel3 != null) environmentLevel3.SetActive(true);

        // =================================================================
        // 🔥 UTAMA: KUNCI & CENTANG ZAKAT EMAS PERAK SAAT MASUK LEVEL 3
        // =================================================================
        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isEmasPerakCompleted = true; // Set status selesai
            ZakatPanelManager.instance.isEmasPerakUnlocked = true;   // Pastikan status unlock tetap aman
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();     // Nyalakan gambar centang & refresh warna panel
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();  // Gelapkan tombol bayar 3D jika fokus di emas perak
        }
        // =================================================================

        if (TokoManager.instance != null && TokoManager.instance.isPlayerInside)
        {
            TokoManager.instance.PerbaruiTampilanToko();
        }
    }

    public void TutupRewardDanMasukLevel3()
    {
        if (panelRewardEmasPerak != null) panelRewardEmasPerak.SetActive(false);

        // ➕ LANGSUNG MUNCULKAN PANEL KONVERSI DI SINI
        if (conversionPanel != null)
        {
            conversionPanel.SetActive(true);
        }

        // 1. Panggil TaskManager untuk mengaktifkan misi pertama Babak 3
        if (TaskManager.instance != null)
        {
            TaskManager.instance.MulaiMisiBabak3();
        }

        // 2. Jalankan penutupan panel, perpindahan lingkungan level, dan pembersihan asset
        ZCapitalManagerClosePanel();
    }

    private void ZCapitalManagerClosePanel()
    {
        // 1. Amankan penutupan panel carousel zakat terlebih dahulu
        if (ZakatPanelManager.instance != null) 
        {
            ZakatPanelManager.instance.CloseZakatPanel();
        }

        // 2. Matikan environment babak 2
        if (Level2Manager.instance != null)
        {
            if (Level2Manager.instance.navCoinLeftEmasPerak != null) Level2Manager.instance.navCoinLeftEmasPerak.SetActive(false);
            if (Level2Manager.instance.environmentLevel2 != null) Level2Manager.instance.environmentLevel2.SetActive(false);
        }

        // 3. Buka barier wilayah babak 3 (Update visual status & tombol UI dijalankan di sini)
        SwitchToLevel3();

        // 4. LAKUKAN KONVERSI DI PALING AKHIR (Setelah semua urutan UI & logika selesai)
        if (MoneyManager.instance != null && InventoryManager.instance != null)
        {
            // Ambil nilai murni sisa uang setelah dipotong zakat
            int sisaEmasMentah = Mathf.RoundToInt(MoneyManager.instance.totalEmas);
            int sisaPerakMentah = Mathf.RoundToInt(MoneyManager.instance.totalPerak);
            
            // Pindahkan ke inventory sebagai ASET
            InventoryManager.instance.KonversiSisaLogamKeAset(sisaEmasMentah, sisaPerakMentah);
            
            // Setelah sukses masuk tas, baru aman di-reset ke 0 agar UI atas bersih
            MoneyManager.instance.totalEmas = 0;
            MoneyManager.instance.totalPerak = 0;
            MoneyManager.instance.UpdateEmasPerakUI();
        }
    }
    public void HandleSensorWilayahMasuk()
    {
        bool sudahLevel3 = (ZakatPanelManager.instance != null) && ZakatPanelManager.instance.isEmasPerakUnlocked;
        if (!sudahLevel3 && txtPeringatanWilayahObj != null)
        {
            txtPeringatanWilayahObj.SetActive(true);
        }
    }

    public void HandleSensorWilayahKeluar()
    {
        if (txtPeringatanWilayahObj != null) txtPeringatanWilayahObj.SetActive(false);
    }
}