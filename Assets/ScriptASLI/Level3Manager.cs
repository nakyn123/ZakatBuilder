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

        // --- TAMBAHKAN BARIS INI ---
        isBabak3Aktif = true; 

        if (barrierLevel3 != null) barrierLevel3.SetActive(false);
        if (environmentLevel3 != null) environmentLevel3.SetActive(true);

        if (TokoManager.instance != null && TokoManager.instance.isPlayerInside)
        {
            TokoManager.instance.PerbaruiTampilanToko();
        }
    }

    public void TutupRewardDanMasukLevel3()
    {
        if (panelRewardEmasPerak != null) panelRewardEmasPerak.SetActive(false);
        
        // 🔥 TAMBAHAN: Panggil TaskManager untuk mengaktifkan misi pertama Babak 3 saat masuk Level 3
        if (TaskManager.instance != null)
        {
            TaskManager.instance.MulaiMisiBabak3();
        }

        ZCapitalManagerClosePanel();
    }

    private void ZCapitalManagerClosePanel()
    {
        if (ZakatPanelManager.instance != null) ZakatPanelManager.instance.CloseZakatPanel();

        if (MoneyManager.instance != null && InventoryManager.instance != null)
        {
            int sisaEmasMentah = Mathf.RoundToInt(MoneyManager.instance.totalEmas);
            int sisaPerakMentah = Mathf.RoundToInt(MoneyManager.instance.totalPerak);
            InventoryManager.instance.KonversiSisaLogamKeAset(sisaEmasMentah, sisaPerakMentah);
            MoneyManager.instance.totalEmas = 0;
            MoneyManager.instance.totalPerak = 0;
            MoneyManager.instance.UpdateEmasPerakUI();
        }

        if (Level2Manager.instance != null)
        {
            if (Level2Manager.instance.navCoinLeftEmasPerak != null) Level2Manager.instance.navCoinLeftEmasPerak.SetActive(false);
            if (Level2Manager.instance.environmentLevel2 != null) Level2Manager.instance.environmentLevel2.SetActive(false);
        }

        SwitchToLevel3();
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