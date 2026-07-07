using UnityEngine;
using TMPro;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager instance;

    [Header("UI Level 2 References")]
    public GameObject navCoinLeftEmasPerak; 

    [Header("🔥 TMPro Text Detection Slots")]
    public TextMeshProUGUI txtPerakUtama;   
    public TextMeshProUGUI txtEmasUtama;    

    [Header("UI Money Manager Hide Settings")]
    public GameObject panelUangUtamaRupiah;

    [Header("Environment Level 2 Only")]
    public GameObject environmentLevel2;   

    // 🔥 TAMBAHAN BARU: Referensi khusus folder wadah koin di Level 2
    [Header("Coins Level 2 Activation")]
    public GameObject koinLevel2Container; 

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Cek apakah komputer mencatat kita sudah pernah masuk level 2 sebelumnya
        if (PlayerPrefs.GetInt("Saved_SudahLevel2", 0) == 1)
        {
            SwitchToLevel2(); // Jika iya, langsung lompat aktifkan Level 2!
        }
        else
        {
            InitLevel1Configuration(); // Jika belum, mulai dari Level 1 normal
        }
    }

    public void InitLevel1Configuration()
    {
        if (navCoinLeftEmasPerak != null) navCoinLeftEmasPerak.SetActive(false);
        if (environmentLevel2 != null) environmentLevel2.SetActive(false); 
        // Pastikan kontainer koin mati di awal game
        if (koinLevel2Container != null) koinLevel2Container.SetActive(false); 
        if (panelUangUtamaRupiah != null) panelUangUtamaRupiah.SetActive(true);
    }

    public void SwitchToLevel2()
    {
        Debug.Log("<color=cyan>[Level 2 Manager]</color> Mengaktifkan seluruh environment Babak 2...");
        
        // Simpan status ke komputer agar saat di-play lagi tidak hilang
        PlayerPrefs.SetInt("Saved_SudahLevel2", 1);
        PlayerPrefs.Save();

        if (panelUangUtamaRupiah != null) panelUangUtamaRupiah.SetActive(false);
        
        // Agar uang emas/perak tidak terus-terusan kembali ke 0/100 saat load game:
        if (MoneyManager.instance != null)
        {
            // Load nilai yang tersimpan, jika tidak ada baru gunakan default (0 dan 100)
            MoneyManager.instance.totalEmas = PlayerPrefs.GetInt("EmasPemain", 0);
            MoneyManager.instance.totalPerak = PlayerPrefs.GetInt("Saved_PerakPemain", 100);
            MoneyManager.instance.UpdateEmasPerakUI();
        }

        if (txtPerakUtama != null) txtPerakUtama.text = MoneyManager.instance.totalPerak + " gr";
        if (txtEmasUtama != null) txtEmasUtama.text = MoneyManager.instance.totalEmas + " gr";

        if (environmentLevel2 != null) environmentLevel2.SetActive(true);
        if (navCoinLeftEmasPerak != null) navCoinLeftEmasPerak.SetActive(true);
        if (koinLevel2Container != null) koinLevel2Container.SetActive(false);

        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isPerdaganganCompleted = true; 
            ZakatPanelManager.instance.isPerdaganganUnlocked = true;   
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();       
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();    
        }

        if (JurnalManager.instance != null)
        {
            if (JurnalManager.instance.visualHalamanLock != null) JurnalManager.instance.visualHalamanLock.SetActive(false);
            if (JurnalManager.instance.visualHalamanUnlock != null) JurnalManager.instance.visualHalamanUnlock.SetActive(true);
            if (JurnalManager.instance.navCoinLeftPanel != null) JurnalManager.instance.navCoinLeftPanel.SetActive(true);
            JurnalManager.instance.CheckEmasPerakNisab();
        }

        if (TaskManager.instance != null)
        {
            if (TaskManager.instance.barTebangJual != null) TaskManager.instance.barTebangJual.SetActive(false);
            if (TaskManager.instance.barTebangPohon != null) TaskManager.instance.barTebangPohon.SetActive(false);
            if (TaskManager.instance.barJualNisab != null) TaskManager.instance.barJualNisab.SetActive(false);
            if (TaskManager.instance.barKeKantorZakat != null) TaskManager.instance.barKeKantorZakat.SetActive(false); // 🌟 TAMBAHKAN BARIS INI
            
            TaskManager.instance.AktifkanMisiEdaranKades();
        }
    }
}