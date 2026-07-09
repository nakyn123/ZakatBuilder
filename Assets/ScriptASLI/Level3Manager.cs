using UnityEngine;
using UnityEngine.UI; // 🔥 TAMBAHKAN BARIS INI AGAR 'Button' DIKENALI
using TMPro;

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

    [Header("Panel Pembatas Level 3")]
    public GameObject panelBabLvl3; // Tarik panel bab-lvl3 kamu ke sini di Inspector
    public Button btnCloseBabLvl3;
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

   // ... (Kode bagian atas tetap sama) ...

   public void TutupRewardDanMasukLevel3()
    {
        // Tutup langsung Carousel Zakat Umum agar background gameplay murni
        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.CloseZakatPanel();
        }

        if (panelRewardEmasPerak != null) panelRewardEmasPerak.SetActive(false);

        // 🔥 TAMBAHKAN PENGECEKAN INI: Jika peternakan sudah selesai, ini adalah ending!
        // Jangan munculkan panel bab level 3 atau konversi lagi, langsung kosongkan.
        if (ZakatPanelManager.instance != null && ZakatPanelManager.instance.isPeternakanCompleted)
        {
            if (panelBabLvl3 != null) panelBabLvl3.SetActive(false);
            if (conversionPanel != null) conversionPanel.SetActive(false);
            Debug.Log("<color=cyan>[Level 3 Manager]</color> Zakat Ternak Selesai. Panel dikosongkan, bersiap untuk Ending...");
            return; 
        }

        // LOGIKA DENGAN BUTTON CLOSE MANUAL (X) UNTUK TRANSISI AWAL MASUK LEVEL 3
        if (panelBabLvl3 != null)
        {
            panelBabLvl3.SetActive(true);

            if (btnCloseBabLvl3 != null)
            {
                btnCloseBabLvl3.onClick.RemoveAllListeners();
                btnCloseBabLvl3.onClick.AddListener(() => {
                    panelBabLvl3.SetActive(false);
                    
                    if (conversionPanel != null)
                    {
                        conversionPanel.SetActive(true);
                    }

                    MulaiLogikaMasukLevel3Akhir();
                });
            }
            else
            {
                Debug.LogWarning("[Level 3 Manager] Kamu belum memasukkan 'Btn Close Bab Lvl 3' di Inspector!");
                if (conversionPanel != null) conversionPanel.SetActive(true);
                MulaiLogikaMasukLevel3Akhir();
            }
        }
        else
        {
            if (conversionPanel != null) conversionPanel.SetActive(true);
            MulaiLogikaMasukLevel3Akhir();
        }
    }

    // Fungsi pembantu baru untuk merapikan urutan sisa eksekusi Level 3
    private void MulaiLogikaMasukLevel3Akhir()
    {
        if (TaskManager.instance != null)
        {
            TaskManager.instance.MulaiMisiBabak3(); //[cite: 10]
        }
        ZCapitalManagerClosePanel(); //[cite: 10]
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

        // =====================================================================
        // 🔥 FIX UTAMA: PAKSA KEBANGKITAN HUD JIRAN & KOIN RUPIAH UTAMA
        // =====================================================================
        if (UIManager.instance != null)
        {
            // Reset hitungan panel UIManager menjadi 0 agar HUD Gameplay mau menyala secara normal
            System.Type.GetType("UIManager").GetField("openedPanelsCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(UIManager.instance, 0);
            
            // Ambil objek UI gameplayHUD dan paksa set aktif ke true
            GameObject gameplayHUDObj = (GameObject)System.Type.GetType("UIManager").GetField("gameplayHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(UIManager.instance);
            if (gameplayHUDObj != null) 
            {
                gameplayHUDObj.SetActive(true);
            }
        }

        // Temukan nav-coin target utama (Rupiah) melalui InventoryManager jika terikat, lalu paksa aktifkan
        if (InventoryManager.instance != null)
        {
            GameObject coinObj = (GameObject)System.Type.GetType("InventoryManager").GetField("navCoinObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(InventoryManager.instance);
            if (coinObj != null)
            {
                coinObj.SetActive(true);
            }
        }
        // =====================================================================

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