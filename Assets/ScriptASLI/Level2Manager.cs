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
        InitLevel1Configuration();
    }

    public void InitLevel1Configuration()
    {
        if (navCoinLeftEmasPerak != null) navCoinLeftEmasPerak.SetActive(false);
        if (environmentLevel2 != null) environmentLevel2.SetActive(false); 
        // Pastikan kontainer koin mati di awal game
        if (koinLevel2Container != null) koinLevel2Container.SetActive(false); 
    }

    public void SwitchToLevel2()
    {
        Debug.Log("<color=cyan>[Level 2 Manager]</color> Mengaktifkan seluruh environment Babak 2...");

        if (MoneyManager.instance != null)
        {
            MoneyManager.instance.totalEmas = 0;   
            MoneyManager.instance.totalPerak = 100; 
            MoneyManager.instance.UpdateEmasPerakUI();
        }

        if (txtPerakUtama != null) txtPerakUtama.text = "100 gr";
        if (txtEmasUtama != null) txtEmasUtama.text = "0 gr";

        if (environmentLevel2 != null) environmentLevel2.SetActive(true);
        if (navCoinLeftEmasPerak != null) navCoinLeftEmasPerak.SetActive(true);

        // 🔥 CRITICAL: Pastikan koin TETAP MATI saat masuk babak 2 sebelum surat dibaca
        if (koinLevel2Container != null) koinLevel2Container.SetActive(false);

        if (JurnalManager.instance != null)
        {
            if (JurnalManager.instance.visualHalamanLock != null) JurnalManager.instance.visualHalamanLock.SetActive(false);
            if (JurnalManager.instance.visualHalamanUnlock != null) JurnalManager.instance.visualHalamanUnlock.SetActive(true);
            if (JurnalManager.instance.navCoinLeftPanel != null) JurnalManager.instance.navCoinLeftPanel.SetActive(true);
            JurnalManager.instance.CheckEmasPerakNisab();
        }

        if (TaskManager.instance != null)
        {
            TaskManager.instance.AktifkanMisiEdaranKades();
        }
    }
}