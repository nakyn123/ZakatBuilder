using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TaskManager : MonoBehaviour {
    public static TaskManager instance;

    // =================================================================
    // 🖥️ UI GLOBAL PANELS, NOTIFICATIONS & AUDIO SETTINGS
    // =================================================================
    [Header("--- UI Global Panels & Notifications ---")]
    public GameObject misiPanel;
    public GameObject asetBlur; 
    public GameObject ikonNotifikasi; 
    public GameObject rawImageHUDArrow; // Objek Raw Image di Gameplay-HUB

    [Header("--- Global UI Visual & Audio Asset ---")]
    public Sprite btnAbuAbu; 
    public Sprite btnHijauAmbil; 
    public AudioClip suaraBukaMisi;    
    public AudioClip suaraTutupMisi;

    // =================================================================
    // 🗺️ NAVIGATION SYSTEM & 3D TARGETS
    // =================================================================
    [Header("--- Navigation System Scripts ---")]
    public UI3DArrowNavigation ui3DArrowScript; //

    [Header("--- Navigation UI Bars (Helper) ---")]
    public GameObject barKeKantorZakat; //
    public GameObject barKeTambang;         // Tarik ke-tambang-bar dari Hierarchy
    public GameObject barKeToko; //
    public GameObject barKeIsiPakan;        // Tarik ke-isi-pakan-bar dari Hierarchy

    [Header("--- 3D Hologram World Targets ---")]
    public Transform lokasiZakatCube; //
    public Transform lokasiTambangCube;      // Tarik 3D Cube Transparan Tambang
    public Transform lokasiTokoCube;         // Tarik 3D Cube Transparan Toko Hewan
    public Transform lokasiIsiPakanCube;     // Tarik 3D Cube Transparan Tempat Pakan

    // =================================================================
    // 🌲 BABAK 1: MISI UTAMA LEVEL 1
    // =================================================================
    [Header("--- Babak 1: Misi 1 (Tebang + Jual) ---")]
    public GameObject barTebangJual;
    public Button btnAmbilTebangJual;
    public Image imgBtnTebangJual;
    public TextMeshProUGUI txtTebangJual;
    public int rewardMisi1 = 5000;

    [Header("--- Babak 1: Misi 2 (Tebang Pohon) ---")]
    public GameObject barTebangPohon; 
    public Button btnAmbilTebangPohon; 
    public Image imgBtnTebangPohon;
    public Slider sliderTebang;
    public TextMeshProUGUI txtTebang;
    public int targetTebang = 5;
    public int rewardMisi2 = 10000;

    [Header("--- Babak 1: Misi 3 (Jual Aset / Nisab Uang) ---")]
    public GameObject barJualNisab; // Nama di hierarchy: jual-nisab-bar
    public Button btnAmbilJualNisab;
    public Image imgBtnJualNisab;
    public Slider sliderJualNisab;
    public TextMeshProUGUI txtJualNisab;
    public int targetNisabUang = 94000000; // Target 94 Juta
    public int rewardMisiNisab = 25000;

    [Header("--- Babak 1: UI Text Zakat Perdagangan ---")]
    public TextMeshProUGUI txtKeKantorZakat; //

    // =================================================================
    // ⛏️ BABAK 2: MISI TAMBANG LEVEL 2
    // =================================================================
    [Header("--- Babak 2: Misi 1 (Surat Edaran Kades) ---")]
    public GameObject barEdaranKades;       
    public Button btnBukaEdaranKades;       
    public GameObject panelEdaranKades;
    public GameObject asetBlurEdaran;
    public TextMeshProUGUI txtIsiEdaranKades; 
    public Button btnCloseEdaranKades;       
    public AudioClip suaraBukaSurat;         
    public AudioClip suaraEmasDapat;        
    public GameObject prefabTeksPlusKades; 
    public RectTransform posisiTargetEmasHUD; 
    [TextArea(3, 10)] public string teksLengkapEdaran; //
    public float kecepatanKetik = 0.05f;    

    // --- TAMBAHAN AUDIO UNTUK TYPEWRITER SURAT EDARAN KADES ---
    [Header("--- Audio Edaran Kades Settings ---")]
    [Tooltip("Masukkan komponen AudioSource yang digunakan untuk membunyikan teks edaran")]
    public AudioSource audioSourceEdaran;
    [Tooltip("Masukkan file sound effect pendek dialog kamu")]
    public AudioClip soundClipEdaran;
    [Tooltip("Suara berbunyi setiap berapa karakter? (Rekomendasi: 3 atau 4 karena kecepatan ketikmu cepat)")]
    public int karakterPerBunyiEdaran = 3;

    [Header("--- Babak 2: Misi 2 (Tambang Logam) ---")]
    public GameObject barTambangLogam;       // Bar UI Baru untuk Misi Tambang
    public Button btnAmbilTambangLogam;       // Tombol Ambil Hadiah
    public Image imgBtnTambangLogam;         // Gambar Tombol Ambil
    public Slider sliderTambangLogam;         // Slider Progress
    public TextMeshProUGUI txtTambangLogam;   // Teks UI Misi (0/15)
    public int targetTambangLogam = 15;       // Target 15 kali
    public int rewardTambangLogam = 5000000;  // Hadiah 5 Juta Rupiah

    // =================================================================
    // 🐓 BABAK 3: MISI PETERNAKAN LEVEL 3
    // =================================================================
    [Header("--- Babak 3: Misi 1 (Beli Hewan Ternak) ---")]
    public GameObject barBeliTernak; //
    public Button btnAmbilKeToko;
    public Image imgBtnKeToko; 
    public TextMeshProUGUI txtKeToko;
    public Slider sliderBeliTernak;
    public int rewardKeToko = 15000;
    private int targetBeliHewanMisi1 = 3;  

    [Header("--- Babak 3: Misi 2 (Beli Pakan Ternak) ---")]
    public GameObject barBeliPakan; 
    public Button btnAmbilBeliPakan;
    public Image imgBtnBeliPakan; 
    public TextMeshProUGUI txtBeliPakan;
    public int rewardBeliPakan = 5000;

    [Header("--- Babak 3: Misi 3 (Isi Pakan Ternak) ---")]
    public GameObject barIsiPakan; 
    public Button btnAmbilIsiPakan; 
    public Image imgBtnIsiPakan; 
    public TextMeshProUGUI txtIsiPakan;
    public int rewardIsiPakan = 10000; 
    private int targetIsiPakan = 6;

    // =================================================================
    // 🔒 SYSTEM PRIVATE TRACKERS & HIDE IN INSPECTOR VARIABLES
    // =================================================================
    [HideInInspector] public int totalLogamMinedCount = 0; // Hitungan progress saat ini
    [HideInInspector] public bool isIsiPakanDone = false; //

    // Babak 1 State Trackers
    private int woodOffset = 0; //
    private bool isMisi2Started = false; //
    private bool isJualDone = false; //
    private bool isMisi1Claimed = false; //
    private bool isTebangDone = false; //
    private bool isMisi2Claimed = false; //
    private bool isNisabMisiDone = false; //
    private bool isNisabMisiClaimed = false; //
    private bool isZakatMisiDone = false; //
    private bool isZakatMisiClaimed = false; //

    // Babak 2 State Trackers
    private bool isTambangLogamDone = false; //
    private bool isTambangLogamClaimed = false; //
    private bool edaranSedangMengetik = false; //
    private Coroutine typewriterCoroutine; //

    // Babak 3 State Trackers
    private int beliHewanMisi1Count = 0; //
    private bool isKeTokoDone = false; //
    private bool isKeTokoClaimed = false; //
    private bool isBeliPakanDone = false; //
    private bool isBeliPakanClaimed = false; //
    private int isiPakanCount = 0; //
    private bool isIsiPakanClaimed = false; //

    [HideInInspector] public bool isGameEnding = false;


    // =================================================================
    // ⚙️ ENGINE CORE FUNCTIONS (START, UPDATE, INITIALIZATION)
    // =================================================================
    void Awake() { instance = this; } //

    void Start() {
        if (barTambangLogam != null) barTambangLogam.SetActive(false); //
        if (misiPanel != null) misiPanel.SetActive(false); //
        if (asetBlur != null) asetBlur.SetActive(false); //
        if (barTebangPohon != null) barTebangPohon.SetActive(false);  //
        if (panelEdaranKades != null) panelEdaranKades.SetActive(false); //
        if (barEdaranKades != null) barEdaranKades.SetActive(false); //
        if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false); //
        if (barJualNisab != null) barJualNisab.SetActive(false); //
        if (barKeKantorZakat != null) barKeKantorZakat.SetActive(false); //

        isMisi2Started = false; //

        StartCoroutine(JalankanPengecekanAwalGame()); //
    }

    void Update() {
        if (isGameEnding) return;
        if (isMisi2Claimed && !isNisabMisiClaimed) {
            UpdateMisiNisabProgress();
        }
    }

    private IEnumerator JalankanPengecekanAwalGame()
    {
        yield return new WaitForEndOfFrame();

        // 🛑 PROTEKSI: Jika state mendeteksi ending, matikan paksa alur notifikasi start awal game
        if (isGameEnding)
        {
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
            yield break;
        }

        if (PlayerPrefs.GetInt("IsRestarted", 0) == 1)
        {
            ResetSeluruhProgressMisi();
            PlayerPrefs.SetInt("IsRestarted", 0);
            PlayerPrefs.Save();
        }
        else
        {
            LoadProgressMisiTerakhir();
        }

        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);
        UpdateMisi1UI();
    }

    private void ResetSeluruhProgressMisi()
    {
        Debug.Log("<color=yellow>[TaskManager]</color> Scene Gameplay Mendeteksi Restart. Mengembalikan posisi objek ke awal...");
        PlayerPrefs.SetInt("Saved_EdaranSelesai", 0);
        if (barTambangLogam != null) barTambangLogam.SetActive(false);

        woodOffset = 0; 
        isMisi2Started = false; 
        isJualDone = false; 
        isMisi1Claimed = false; 
        isTebangDone = false; 
        isMisi2Claimed = false; 
        isNisabMisiDone = false;
        isNisabMisiClaimed = false;
        isZakatMisiDone = false;
        isZakatMisiClaimed = false;
        beliHewanMisi1Count = 0; 
        isKeTokoDone = false; 
        isKeTokoClaimed = false; 
        isBeliPakanDone = false; 
        isBeliPakanClaimed = false; 
        isiPakanCount = 0; 
        isIsiPakanDone = false; 
        isIsiPakanClaimed = false; 

        if (MoneyManager.instance != null) 
        {
            MoneyManager.instance.totalMoney = 0; 
            MoneyManager.instance.totalEmas = 0; 
            MoneyManager.instance.totalPerak = 0; 
            MoneyManager.instance.UpdateEmasPerakUI();  
        }

        if (InventoryManager.instance != null) 
        {
            InventoryManager.instance.woodKecilCount = 0; 
            InventoryManager.instance.woodSedangCount = 0; 
            InventoryManager.instance.woodBesarCount = 0; 
            InventoryManager.instance.asetEmasCount = 0; 
            InventoryManager.instance.asetPerakCount = 0; 
            InventoryManager.instance.pakanRumputCount = 0; 
            InventoryManager.instance.totalWoodCollected = 0; 
            InventoryManager.instance.UpdateUI();  
        }

        if (ZakatPanelManager.instance != null) 
        {
            ZakatPanelManager.instance.isPerdaganganUnlocked = false; 
            ZakatPanelManager.instance.isEmasPerakUnlocked = false; 
            ZakatPanelManager.instance.isPeternakanUnlocked = false; 
            ZakatPanelManager.instance.isPerdaganganCompleted = false; 
            ZakatPanelManager.instance.isEmasPerakCompleted = false; 
            ZakatPanelManager.instance.isPeternakanCompleted = false; 
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();  
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player"); 
        if (player != null) 
        {
            CharacterController cc = player.GetComponent<CharacterController>(); 
            if (cc != null) cc.enabled = false;  

            player.transform.position = new Vector3(8.751f, 0.44f, -64.016f);  
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f); 

            if (cc != null) cc.enabled = true;
            
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null && pm.cameraTransform != null)
            {
                pm.cameraTransform.position = new Vector3(0, 2f, -5f); 
                pm.cameraTransform.rotation = Quaternion.identity;
            }
        }

        if (JurnalManager.instance != null) 
        {
            JurnalManager.instance.isNisabReached = false; 
            JurnalManager.instance.isHaulComplete = false; 
            JurnalManager.instance.isZakatPaid = false; 
            JurnalManager.instance.isDagangLockedInJurnal = false; 
            if (JurnalManager.instance.haulSlider != null) JurnalManager.instance.haulSlider.value = 0f; 

            JurnalManager.instance.isEmasPerakNisabReached = false; 
            JurnalManager.instance.isEmasPerakHaulComplete = false; 
            JurnalManager.instance.isEmasPerakZakatPaid = false; 
            JurnalManager.instance.isEmasLockedInJurnal = false; 
            if (JurnalManager.instance.visualHalamanLock != null) JurnalManager.instance.visualHalamanLock.SetActive(true); 
            if (JurnalManager.instance.visualHalamanUnlock != null) JurnalManager.instance.visualHalamanUnlock.SetActive(false); 
            if (JurnalManager.instance.navCoinLeftPanel != null) JurnalManager.instance.navCoinLeftPanel.SetActive(false); 
            if (JurnalManager.instance.haulSliderEmasPerak != null) JurnalManager.instance.haulSliderEmasPerak.value = 0f; 

            JurnalManager.instance.isTernakNisabReached = false; 
            JurnalManager.instance.isTernakHaulComplete = false; 
            JurnalManager.instance.isTernakZakatPaid = false; 
            JurnalManager.instance.isTernakLockedInJurnal = false; 
            if (JurnalManager.instance.panelLockTernak != null) JurnalManager.instance.panelLockTernak.SetActive(true); 
            if (JurnalManager.instance.panelUnlockTernak != null) JurnalManager.instance.panelUnlockTernak.SetActive(false); 
            if (JurnalManager.instance.haulSliderTernak != null) JurnalManager.instance.haulSliderTernak.value = 0f; 

            JurnalManager.instance.MatikanSistemBeranak();  
            JurnalManager.instance.StopAllCoroutines(); 
            JurnalManager.instance.ShowPage(1);  
        }
    }

    private void LoadProgressMisiTerakhir()
    {
        Debug.Log("<color=green>[TaskManager]</color> Melanjutkan game dari data terakhir.");

        isMisi1Claimed = PlayerPrefs.GetInt("Saved_IsMisi1Claimed", 0) == 1;
        isJualDone = PlayerPrefs.GetInt("Saved_IsJualDone", 0) == 1;
        isMisi2Started = PlayerPrefs.GetInt("Saved_IsMisi2Started", 0) == 1;
        isTebangDone = PlayerPrefs.GetInt("Saved_IsTebangDone", 0) == 1;
        isMisi2Claimed = PlayerPrefs.GetInt("Saved_IsMisi2Claimed", 0) == 1;
        isNisabMisiDone = PlayerPrefs.GetInt("Saved_IsNisabMisiDone", 0) == 1;
        isNisabMisiClaimed = PlayerPrefs.GetInt("Saved_IsNisabMisiClaimed", 0) == 1;
        isZakatMisiDone = PlayerPrefs.GetInt("Saved_IsZakatMisiDone", 0) == 1;
        isZakatMisiClaimed = PlayerPrefs.GetInt("Saved_IsZakatMisiClaimed", 0) == 1;
        isKeTokoDone = PlayerPrefs.GetInt("Saved_IsKeTokoDone", 0) == 1;
        isKeTokoClaimed = PlayerPrefs.GetInt("Saved_IsKeTokoClaimed", 0) == 1;
        beliHewanMisi1Count = PlayerPrefs.GetInt("Saved_BeliHewanCount", 0);
        isBeliPakanDone = PlayerPrefs.GetInt("Saved_IsBeliPakanDone", 0) == 1;
        isBeliPakanClaimed = PlayerPrefs.GetInt("Saved_IsBeliPakanClaimed", 0) == 1;
        isiPakanCount = PlayerPrefs.GetInt("Saved_IsiPakanCount", 0);
        isIsiPakanDone = PlayerPrefs.GetInt("Saved_IsIsiPakanDone", 0) == 1;
        isIsiPakanClaimed = PlayerPrefs.GetInt("Saved_IsiPakanClaimed", 0) == 1;
        
        isTambangLogamDone = PlayerPrefs.GetInt("Saved_IsTambangLogamDone", 0) == 1;
        isTambangLogamClaimed = PlayerPrefs.GetInt("Saved_IsTambangLogamClaimed", 0) == 1;
        totalLogamMinedCount = PlayerPrefs.GetInt("Saved_TotalLogamMinedCount", 0);

        woodOffset = PlayerPrefs.GetInt("Saved_WoodOffset", 0);

        // 🎯 DETEKSI STATUS LEVEL DARI PLAYERPREFS KESAYANGANMU
        bool sudahMasukLevel2 = PlayerPrefs.GetInt("Saved_SudahLevel2", 0) == 1;

        if (sudahMasukLevel2)
        {
            // 🔒 JIKA DI LEVEL 2: Paksa matikan semua bar misi kayu Level 1 agar tidak ketimpa!
            if (barTebangJual != null) barTebangJual.SetActive(false);
            if (barTebangPohon != null) barTebangPohon.SetActive(false);
            if (barJualNisab != null) barJualNisab.SetActive(false);
            if (barKeKantorZakat != null) barKeKantorZakat.SetActive(false);

            // ⛏️ AKTIFKAN MISI TAMBANG EMAS/PERAK LEVEL 2
            if (barTambangLogam != null) {
                // Selama hadiah tambang belum diklaim, pastikan bar tambang logam selalu menyala di panel
                barTambangLogam.SetActive(!isTambangLogamClaimed);
                if (barTambangLogam.activeSelf) UpdateTambangLogamProgress(totalLogamMinedCount);
            }
        }
        else
        {
            // 🌲 JIKA MASIH DI LEVEL 1: Jalankan logika load bawaan kamu secara normal
            if (barTebangJual != null) barTebangJual.SetActive(!isMisi1Claimed);
            if (barTebangPohon != null) barTebangPohon.SetActive(isJualDone && !isMisi2Claimed);
            
            if (barJualNisab != null) {
                barJualNisab.SetActive(isMisi2Claimed && !isNisabMisiClaimed);
            }
            if (barKeKantorZakat != null) {
                barKeKantorZakat.SetActive(isNisabMisiDone && !isZakatMisiClaimed);
            }
        }

        // Jalankan sisa load manager di bawahnya
        if (MoneyManager.instance != null) {
            MoneyManager.instance.totalMoney = PlayerPrefs.GetInt("JumlahUangPemain", 0);
            MoneyManager.instance.totalEmas = PlayerPrefs.GetInt("EmasPemain", 0);
            MoneyManager.instance.totalPerak = PlayerPrefs.GetInt("Saved_PerakPemain", 0);
            MoneyManager.instance.UpdateEmasPerakUI();
        }

        if (InventoryManager.instance != null) {
            InventoryManager.instance.woodKecilCount = PlayerPrefs.GetInt("Saved_WoodKecil", 0);
            InventoryManager.instance.woodSedangCount = PlayerPrefs.GetInt("Saved_WoodSedang", 0);
            InventoryManager.instance.woodBesarCount = PlayerPrefs.GetInt("Saved_WoodBesar", 0);
            InventoryManager.instance.asetEmasCount = PlayerPrefs.GetInt("Saved_AsetEmas", 0);
            InventoryManager.instance.asetPerakCount = PlayerPrefs.GetInt("Saved_AsetPerak", 0);
            InventoryManager.instance.pakanRumputCount = PlayerPrefs.GetInt("Saved_PakanRumput", 0);
            InventoryManager.instance.totalWoodCollected = PlayerPrefs.GetInt("TotalKayuDitebang", 0);
            InventoryManager.instance.UpdateUI();
        }

       // 🎯 KUNCI LOAD GAME LEVEL 3 DI TASKMANAGER.CS
        if (isMisi2Claimed && (Level3Manager.instance != null && Level3Manager.instance.isBabak3Aktif)) {
            // Jika hadiah belanja belum diklaim, biarkan bar navigasi toko menyala
            if (barKeToko != null) barKeToko.SetActive(!isKeTokoClaimed);
            
            // Pastikan barBeliTernak ikut di-load status hidup/matinya beserta slidernya!
            if (barBeliTernak != null) {
                barBeliTernak.SetActive(!isKeTokoClaimed);
                if (sliderBeliTernak != null) {
                    sliderBeliTernak.maxValue = targetBeliHewanMisi1;
                    sliderBeliTernak.value = beliHewanMisi1Count;
                }
            }

            if (barBeliPakan != null) barBeliPakan.SetActive(!isBeliPakanClaimed);
            
            if (isKeTokoDone && isBeliPakanDone) {
                if (barIsiPakan != null) barIsiPakan.SetActive(!isIsiPakanClaimed);
            } else {
                if (barIsiPakan != null) barIsiPakan.SetActive(false);
            }
        }
    }

    public void SimpanProgressGameKeKomputer()
    {
        PlayerPrefs.SetInt("Saved_IsMisi1Claimed", isMisi1Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsJualDone", isJualDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsMisi2Started", isMisi2Started ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsTebangDone", isTebangDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsMisi2Claimed", isMisi2Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsNisabMisiDone", isNisabMisiDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsNisabMisiClaimed", isNisabMisiClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsZakatMisiDone", isZakatMisiDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsZakatMisiClaimed", isZakatMisiClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsKeTokoDone", isKeTokoDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsKeTokoClaimed", isKeTokoClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_BeliHewanCount", beliHewanMisi1Count);
        PlayerPrefs.SetInt("Saved_IsBeliPakanDone", isBeliPakanDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsBeliPakanClaimed", isBeliPakanClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsiPakanCount", isiPakanCount);
        PlayerPrefs.SetInt("Saved_IsIsiPakanDone", isIsiPakanDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsiPakanClaimed", isIsiPakanClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsTambangLogamDone", isTambangLogamDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsTambangLogamClaimed", isTambangLogamClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_TotalLogamMinedCount", totalLogamMinedCount);
        PlayerPrefs.SetInt("Saved_WoodOffset", woodOffset);

        if (MoneyManager.instance != null)
        {
            PlayerPrefs.SetInt("JumlahUangPemain", MoneyManager.instance.totalMoney);
            PlayerPrefs.SetInt("EmasPemain", MoneyManager.instance.totalEmas);
            PlayerPrefs.SetInt("Saved_PerakPemain", MoneyManager.instance.totalPerak);
        }

        if (InventoryManager.instance != null)
        {
            PlayerPrefs.SetInt("Saved_WoodKecil", InventoryManager.instance.woodKecilCount);
            PlayerPrefs.SetInt("Saved_WoodSedang", InventoryManager.instance.woodSedangCount);
            PlayerPrefs.SetInt("Saved_WoodBesar", InventoryManager.instance.woodBesarCount);
            PlayerPrefs.SetInt("Saved_AsetEmas", InventoryManager.instance.asetEmasCount);
            PlayerPrefs.SetInt("Saved_AsetPerak", InventoryManager.instance.asetPerakCount);
            PlayerPrefs.SetInt("Saved_PakanRumput", InventoryManager.instance.pakanRumputCount);
            PlayerPrefs.SetInt("TotalKayuDitebang", InventoryManager.instance.totalWoodCollected);
        }

        PlayerPrefs.Save();
    }

    public void OpenMisi() {
        // 🛑 KUNCI GERBANG UTAMA: Jika game sudah ending, BLOKIR total akses membuka panel misi!
        if (isGameEnding) return;

        if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraBukaMisi != null) {
            InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraBukaMisi);
        }

        if (misiPanel != null) {
            if (UIManager.instance != null) {
                UIManager.instance.OpenPanelMenu(misiPanel);
            } else {
                misiPanel.SetActive(true);
            }

            if (asetBlur != null) asetBlur.SetActive(true);
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
            
            bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;
            
            if (!sudahLevel3 && !isMisi2Claimed && InventoryManager.instance != null) {
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected);
            }
            if (barTambangLogam != null && barTambangLogam.activeSelf) {
                UpdateTambangLogamProgress(totalLogamMinedCount);
            }
        }
    }

    public void CloseMisi() {
        if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraTutupMisi != null) {
            InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraTutupMisi);
        }

        if (UIManager.instance != null) {
            UIManager.instance.ClosePanelMenu(misiPanel);
        } else {
            if (misiPanel != null) misiPanel.SetActive(false);
        }
        if (asetBlur != null) asetBlur.SetActive(false);
    }

    public void NotifyWoodSold() {
        if (isJualDone) return; 
        isJualDone = true;

        if (InventoryManager.instance != null) {
            woodOffset = InventoryManager.instance.totalWoodCollected;
        }
        isMisi2Started = true;

        if (!misiPanel.activeSelf && ikonNotifikasi != null) {
            ikonNotifikasi.SetActive(true);
        }
        UpdateMisi1UI();
    }

    void UpdateMisi1UI() {
        if (Level3Manager.instance != null && Level3Manager.instance.isBabak3Aktif) return;

        if (isJualDone) {
            txtTebangJual.text = "Selesai!";
            imgBtnTebangJual.sprite = btnHijauAmbil; 
            
            if (barTebangPohon != null && !barTebangPohon.activeSelf) {
                barTebangPohon.SetActive(true);
                barTebangPohon.transform.SetAsFirstSibling(); 
                if (!misiPanel.activeSelf && ikonNotifikasi != null) ikonNotifikasi.SetActive(true);
            }
        } else {
            txtTebangJual.text = "Tebang & Jual Kayu";
            imgBtnTebangJual.sprite = btnAbuAbu; 
        }
    }

    public void AmbilHadiahTebangJual() {
        if (isJualDone && !isMisi1Claimed) {
            isMisi1Claimed = true;
            PlayRewardEffects(rewardMisi1, btnAmbilTebangJual.transform);
            if (barTebangPohon != null) {
                barTebangPohon.SetActive(true);
                barTebangPohon.transform.SetAsFirstSibling(); 
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected); 
            }
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardMisi1);
            btnAmbilTebangJual.gameObject.SetActive(false);
            if (barTebangJual != null) barTebangJual.SetActive(false);
            SimpanProgressGameKeKomputer();
        }
    }

    public void UpdateTebangProgress(int totalCount) {
        if (totalCount >= 1 && PlayerPrefs.GetInt("Panel17Selesai", 0) == 0) {
            PlayerPrefs.SetInt("Panel17Selesai", 1);
            PlayerPrefs.Save();
            if (IntroStoryManager.instance != null) {
                IntroStoryManager.instance.TriggerPanel17SelesaiTebang();
            }
        }

        if (isMisi2Claimed) return; 

        int progressMisiSekarang = totalCount - woodOffset; 
        if (progressMisiSekarang < 0) progressMisiSekarang = 0;

        if (barTebangPohon != null && barTebangPohon.activeSelf) {
            if (sliderTebang != null) {
                sliderTebang.maxValue = targetTebang;
                sliderTebang.value = progressMisiSekarang;
            }
            if (txtTebang != null) {
                txtTebang.text = "Tebang Pohon (" + progressMisiSekarang.ToString() + "/" + targetTebang.ToString() + ")";
            }
            if (progressMisiSekarang >= targetTebang) {
                isTebangDone = true;
                if (imgBtnTebangPohon != null) imgBtnTebangPohon.sprite = btnHijauAmbil;
                if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                    ikonNotifikasi.SetActive(true);
                }
            }
        }
    }

    public void AmbilHadiahTebangPohon() {
        if (isTebangDone && !isMisi2Claimed) {
            isMisi2Claimed = true;
            PlayRewardEffects(rewardMisi2, btnAmbilTebangPohon.transform);
            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardMisi2);
            }
            btnAmbilTebangPohon.gameObject.SetActive(false);
            txtTebang.text = "Misi Selesai!";
            if (barTebangPohon != null) barTebangPohon.SetActive(false);

            if (barJualNisab != null) {
                barJualNisab.SetActive(true);
                barJualNisab.transform.SetAsFirstSibling();
                if (imgBtnJualNisab != null) imgBtnJualNisab.sprite = btnAbuAbu;
            }
            SimpanProgressGameKeKomputer();
        }
    }

    private void UpdateMisiNisabProgress() {
        if (isGameEnding) return;
        if (MoneyManager.instance == null || barJualNisab == null || !barJualNisab.activeSelf) return;

        int uangSekarang = MoneyManager.instance.totalMoney;
        if (uangSekarang > targetNisabUang) uangSekarang = targetNisabUang; 

        if (sliderJualNisab != null) {
            sliderJualNisab.maxValue = targetNisabUang;
            sliderJualNisab.value = uangSekarang;
        }

        if (uangSekarang >= targetNisabUang && !isNisabMisiDone) {
            isNisabMisiDone = true;
            if (imgBtnJualNisab != null) imgBtnJualNisab.sprite = btnHijauAmbil;
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
            AktifkanMisiKeKantorZakat();
        }
    }

    public void AmbilHadiahJualNisab() {
        if (isNisabMisiDone && !isNisabMisiClaimed) {
            isNisabMisiClaimed = true;
            PlayRewardEffects(rewardMisiNisab, btnAmbilJualNisab.transform);
            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardMisiNisab);
            }
            btnAmbilJualNisab.gameObject.SetActive(false);
            if (barJualNisab != null) barJualNisab.SetActive(false);
            SimpanProgressGameKeKomputer();
        }
    }

    private void AktifkanMisiKeKantorZakat() {
        if (barKeKantorZakat != null && !barKeKantorZakat.activeSelf && !isZakatMisiClaimed) {
            barKeKantorZakat.SetActive(true);
            barKeKantorZakat.transform.SetAsFirstSibling();
            if (txtKeKantorZakat != null) txtKeKantorZakat.text = "Pergi ke Kantor Zakat & Bayar Zakat Perdagangan";
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);
        }
    }

    public void NotifyZakatPaid() {
        if (isZakatMisiDone) return;
        isZakatMisiDone = true;
        isZakatMisiClaimed = true;

        if (barKeKantorZakat != null) {
            barKeKantorZakat.SetActive(false);
        }
        
        AktifkanMisiEdaranKades();
        SimpanProgressGameKeKomputer();
        Debug.Log("<color=green>[TaskManager]</color> Misi Zakat selesai, bar langsung di-hide & Misi Edaran Kades aktif!");
    }

    public void AktifkanMisiEdaranKades() {
        if (barEdaranKades != null) {
            barEdaranKades.SetActive(true);
            barEdaranKades.transform.SetAsFirstSibling(); 
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void BukaSuratEdaranKades() {
        if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraBukaSurat != null) {
            InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraBukaSurat);
        }

        if (panelEdaranKades != null) {
            if (misiPanel != null) misiPanel.SetActive(false);
            if (asetBlurEdaran != null) {
                asetBlurEdaran.SetActive(true);
                asetBlurEdaran.transform.SetAsFirstSibling(); 
            }
            panelEdaranKades.SetActive(true);
            panelEdaranKades.transform.SetAsLastSibling(); 
           
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(false);
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }
    }

    IEnumerator TypewriterRoutine() {
        if (txtIsiEdaranKades != null) {
            edaranSedangMengetik = true; 
            txtIsiEdaranKades.text = ""; 

            // Pengaturan Nada Bapak-bapak Tua (Berat & Rendah)
            float pitchMinBapak = 0.55f;
            float pitchMaxBapak = 0.75f;

            char[] hurufArray = teksLengkapEdaran.ToCharArray();

            for (int i = 0; i < hurufArray.Length; i++) {
                txtIsiEdaranKades.text += hurufArray[i];

                // Hitung sisa huruf yang belum diketik ke titik akhir kalimat
                int sisaKarakter = hurufArray.Length - (i + 1);

                // Logika Suara: Bunyi di kelipatan karakterPerBunyiEdaran, BUKAN spasi, dan sisa karakter > 3
                if (i > 0 && i % karakterPerBunyiEdaran == 0 && sisaKarakter > 3) {
                    char karakterSekarang = hurufArray[i];

                    // Benar-benar abaikan spasi / white space
                    if (karakterSekarang != ' ' && audioSourceEdaran != null && soundClipEdaran != null) {
                        audioSourceEdaran.clip = soundClipEdaran;
                        // Terapkan pitch bapak-bapak tua
                        audioSourceEdaran.pitch = Random.Range(pitchMinBapak, pitchMaxBapak);
                        audioSourceEdaran.Play();
                    }
                }
                // Hentikan paksa audio lebih awal jika sudah sangat mendekati akhir kalimat
                else if (sisaKarakter <= 3 && audioSourceEdaran != null && audioSourceEdaran.isPlaying) {
                    audioSourceEdaran.Stop();
                }

                yield return new WaitForSeconds(kecepatanKetik); 
            }

            // Pastikan audio mati total saat ketikan selesai murni
            if (audioSourceEdaran != null) audioSourceEdaran.Stop();

            edaranSedangMengetik = false; 
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(true);
            }
        }
    }

    public void SkipKetikEdaran() {
        if (edaranSedangMengetik) {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

            // FIX AUDIO: Matikan suara edaran secara paksa saat di-skip
            if (audioSourceEdaran != null) audioSourceEdaran.Stop();

            if (txtIsiEdaranKades != null) {
                txtIsiEdaranKades.text = teksLengkapEdaran;
            }
            edaranSedangMengetik = false;
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(true);
            }
        }
    }

    public void TutupSuratEdaranKades() {
        if (panelEdaranKades != null) {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            if (audioSourceEdaran != null) audioSourceEdaran.Stop();
            if (UIManager.instance != null) {
                UIManager.instance.ClosePanelMenu(panelEdaranKades);
            } else {
                panelEdaranKades.SetActive(false);
            }
            if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false);
            if (btnBukaEdaranKades != null) btnBukaEdaranKades.gameObject.SetActive(false); 

            if (Level2Manager.instance != null && Level2Manager.instance.koinLevel2Container != null) {
                Level2Manager.instance.koinLevel2Container.SetActive(true);
            }

            // =====================================================================
            // 🌟 LOGIKA DATA BACKEND UTAMA
            // =====================================================================
            if (MoneyManager.instance != null) {
                MoneyManager.instance.totalEmas += 5; 
                MoneyManager.instance.UpdateEmasPerakUI(); 
            }

            if (Level2Manager.instance != null && Level2Manager.instance.txtEmasUtama != null) {
                Level2Manager.instance.txtEmasUtama.text = MoneyManager.instance.totalEmas + " gr";
            }

            /// =====================================================================
            // 🌟 LAHIRKAN ANIMASI BARU KHUSUS KADES DI LUAR LAYOUT GROUP 🌟
            // =====================================================================
            if (prefabTeksPlusKades != null && posisiTargetEmasHUD != null) {
                if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraEmasDapat != null) {
                    InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraEmasDapat);
                }

                // 🌟 KUNCI 1: Spawn menempel langsung pada Canvas terluar (parent dari parent-nya HUD)
                // Ini dilakukan agar prefab bebas dari kekuasaan Horizontal Layout Group!
                Transform rootCanvas = posisiTargetEmasHUD.parent.parent;
                GameObject teksPlusObj = Instantiate(prefabTeksPlusKades, rootCanvas, false);
                
                TeksPlusKadesAnim komponenAnim = teksPlusObj.GetComponent<TeksPlusKadesAnim>();
                if (komponenAnim != null) {
                    komponenAnim.SetupTeksPlus("+5 gr"); 
                }

                // 🌟 KUNCI 2: Ambil RectTransform dan samakan posisinya secara mutlak
                RectTransform rectAnim = teksPlusObj.GetComponent<RectTransform>();
                if (rectAnim != null) {
                    // Gunakan .position (World Space UI) untuk menyamakan letak secara presisi dengan ikon emas HUD
                    rectAnim.position = posisiTargetEmasHUD.position;
                    
                    // Karena sudah bebas dari layout group, jika kamu ingin menggesernya sedikit ke kiri 
                    // atau ke bawah agar tidak menumpuk pas di atas teks angka, tinggal mainkan offset kecil ini:
                    rectAnim.anchoredPosition = new Vector2(rectAnim.anchoredPosition.x - 40f, rectAnim.anchoredPosition.y - 10f);
                }
                
                teksPlusObj.transform.SetAsLastSibling(); 
            }
            
            PlayerPrefs.SetInt("Saved_EdaranSelesai", 1); 
            PlayerPrefs.Save();

            if (barEdaranKades != null) barEdaranKades.SetActive(false); 
            
            if (barKeTambang != null) barKeTambang.SetActive(true);
            if (barTambangLogam != null) {
                barTambangLogam.SetActive(true);
                UpdateTambangLogamProgress(totalLogamMinedCount); 
            }

            if (ikonNotifikasi != null && !misiPanel.activeSelf) {
                ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void MulaiMisiBabak3() {
        // 🔒 PAKSA MATIKAN: Semua bar dari babak 1 dan babak 2 termasuk Kantor Zakat lama!
        if (barTebangJual != null) barTebangJual.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);
        if (barTambangLogam != null) barTambangLogam.SetActive(false);
        if (barKeKantorZakat != null) barKeKantorZakat.SetActive(false);

        // 1. Aktifkan Bar Navigasi "Pergi Ke Toko"
        if (barKeToko != null) {
            barKeToko.SetActive(true);
            barKeToko.transform.SetAsFirstSibling();
        }

        // 2. Aktifkan Bar Misi Progress "Beli Hewan Ternak (0/3)"
        if (barBeliTernak != null) {
            barBeliTernak.SetActive(true);
        }

        // 3. Aktifkan Bar Misi "Beli Pakan"
        if (barBeliPakan != null) barBeliPakan.SetActive(true);

        beliHewanMisi1Count = 0; 
        if (sliderBeliTernak != null) {
            sliderBeliTernak.maxValue = targetBeliHewanMisi1;
            sliderBeliTernak.value = beliHewanMisi1Count;
        }

        // Sesuaikan teks dan tombol ambil masing-masing bar
        if (btnAmbilKeToko != null) btnAmbilKeToko.gameObject.SetActive(true); 
        if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnAbuAbu; 
        if (txtKeToko != null) txtKeToko.text = $"Beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";

        if (btnAmbilBeliPakan != null) btnAmbilBeliPakan.gameObject.SetActive(true);
        if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnAbuAbu;
        if (txtBeliPakan != null) txtBeliPakan.text = "Beli pakan di toko";

        if (barIsiPakan != null) barIsiPakan.SetActive(false);
        if (barKeIsiPakan != null) barKeIsiPakan.SetActive(false);
        isiPakanCount = 0;

        if (ikonNotifikasi != null && !misiPanel.activeSelf) {
            ikonNotifikasi.SetActive(true);
        }

        // =================================================================
        // 🔥 REVISI LEVEL 3: HANYA NYALAKAN ZONA MERAH TOKO SECARA OTOMATIS
        // =================================================================
        // Skrip ini langsung mengaktifkan tabung/cube area merah di map tanpa memicu panah HUD
        if (lokasiTokoCube != null) {
            lokasiTokoCube.gameObject.SetActive(true);
        }
    }

    // 🌟 SEKARANG NAMANYA SUDAH SINKRON SESUAI LAPORANMU KESAYANGAN 🌟
    public void KlaimRewardBeliTernak() {
        if (!isKeTokoDone) return; 
        if (isKeTokoDone && !isKeTokoClaimed) {
            isKeTokoClaimed = true; 
            PlayRewardEffects(rewardKeToko, btnAmbilKeToko.transform);
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardKeToko); 
            
            // Matikan bar navigasi "Pergi ke Toko" dan bar progress "Beli Hewan Ternak" sekaligus!
            if (barKeToko != null) barKeToko.SetActive(false); 
            if (barBeliTernak != null) barBeliTernak.SetActive(false); 
            
            CekDanAktifkanMisiIsiPakanAkhir();
        }
    }

    public void NotifyHewanDibeli() {
        if (!isKeTokoDone && barBeliTernak != null && barBeliTernak.activeSelf) {
            
            // =============== TAMBAHAN PENGAMAN ===============
            // Pemain mulai mencicil beli hewan, matikan bar bantuan pergi ke toko
            if (barKeToko != null && barKeToko.activeSelf) barKeToko.SetActive(false);
            // =================================================

            beliHewanMisi1Count++;
            if (beliHewanMisi1Count > targetBeliHewanMisi1) beliHewanMisi1Count = targetBeliHewanMisi1;
            
            if (sliderBeliTernak != null) {
                sliderBeliTernak.value = beliHewanMisi1Count;
            }

            if (txtKeToko != null) txtKeToko.text = $"Beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";
            
            if (beliHewanMisi1Count >= targetBeliHewanMisi1) {
                isKeTokoDone = true;
                if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnHijauAmbil; 
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void NotifyBeliPakan() {
        if (!isBeliPakanDone) {
            
            // =============== TAMBAHAN PENGAMAN ===============
            // Pemain sukses beli pakan, otomatis matikan bar bantuan pergi ke toko
            if (barKeToko != null && barKeToko.activeSelf) barKeToko.SetActive(false);
            // =================================================

            isBeliPakanDone = true;
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnHijauAmbil; 
            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }
    }
    public void NotifyIsiPakanWorld3D() {
        if (!isIsiPakanDone) {

            // =============== TAMBAHAN PENGAMAN ===============
            // Pemain sudah mulai mengisi pakan di world, sembunyikan bar bantuan navigasinya
            if (barKeIsiPakan != null && barKeIsiPakan.activeSelf) barKeIsiPakan.SetActive(false);
            // =================================================

            isiPakanCount++;
            if (isiPakanCount > targetIsiPakan) isiPakanCount = targetIsiPakan;
            if (txtIsiPakan != null) txtIsiPakan.text = $"Mengisi pakan hewan ({isiPakanCount}/{targetIsiPakan})";

            if (isiPakanCount >= targetIsiPakan) {
                isIsiPakanDone = true;
                if (txtIsiPakan != null) txtIsiPakan.text = "Selesai mengisi pakan hewan!";
                if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnHijauAmbil; 
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void KlaimRewardBeliPakan() {
        if (!isBeliPakanDone) return;
        if (isBeliPakanDone && !isBeliPakanClaimed) {
            isBeliPakanClaimed = true;
            PlayRewardEffects(rewardBeliPakan, btnAmbilBeliPakan.transform);
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardBeliPakan);
            if (barBeliPakan != null) barBeliPakan.SetActive(false); 
            
            CekDanAktifkanMisiIsiPakanAkhir();
        }
    }

    // Fungsi baru untuk menyalakan misi isi pakan dan navigasinya secara serentak
    private void CekDanAktifkanMisiIsiPakanAkhir() {
        // Logika ini memastikan KEDUA reward (Beli Ternak & Beli Pakan) wajib di-klaim dulu!
        if (isKeTokoClaimed && isBeliPakanClaimed) {
            // 1. Munculkan bar navigasi "Pergi ke Tempat Isi Pakan" terlebih dahulu
            if (barKeIsiPakan != null) {
                barKeIsiPakan.SetActive(true);
                barKeIsiPakan.transform.SetAsFirstSibling(); // 🌟 DORONG KE POSISI PALING ATAS
            }

            // 2. Munculkan bar progress "Isi Pakan Ternak (0/6)" tepat di bawahnya sebagai satu kesatuan
            if (barIsiPakan != null && !isIsiPakanClaimed) {
                barIsiPakan.SetActive(true);
                // Biarkan barIsiPakan mengikuti urutan Layout Group otomatis di bawah navigasinya
            }

            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }
    }

    public void KlaimRewardIsiPakan() {
        if (!isIsiPakanDone) return;
        if (isIsiPakanDone && !isIsiPakanClaimed) {
            isIsiPakanClaimed = true;
            PlayRewardEffects(rewardIsiPakan, btnAmbilIsiPakan.transform);
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardIsiPakan);
            if (barIsiPakan != null) barIsiPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    private void PlayRewardEffects(int rewardAmount, Transform buttonTransform) {
        if (InventoryManager.instance != null) {
            InventoryManager.instance.Invoke("SpawnUICoin", 0f); 
            if (InventoryManager.instance.uiCoinPrefab != null && InventoryManager.instance.navCoinTarget != null) {
                int jumlahKoin = 5;
                for (int i = 0; i < jumlahKoin; i++) {
                    GameObject coin = Instantiate(InventoryManager.instance.uiCoinPrefab, misiPanel.transform.parent);
                    coin.transform.SetAsLastSibling();
                    coin.transform.position = buttonTransform.position; 

                    UICoinEffect effect = coin.GetComponent<UICoinEffect>();
                    if (effect == null) effect = coin.AddComponent<UICoinEffect>();
                    
                    int nilaiPerKoin = (i == 0) ? rewardAmount : 0;
                    effect.Init(InventoryManager.instance.navCoinTarget, nilaiPerKoin);
                }
            }
            if (InventoryManager.instance.audioSourceInventory != null && InventoryManager.instance.suaraJualKoin != null) {
                InventoryManager.instance.audioSourceInventory.PlayOneShot(InventoryManager.instance.suaraJualKoin);
            }
        }
    }

    private void CekSemuaMisiBabak3Selesai() {
        if (isBeliPakanClaimed && isIsiPakanClaimed) {
            Debug.Log("<color=cyan>[Task Manager]</color> Babak 3 SELESAI MUTLAK!");
        }
    }

    public void UpdateTambangLogamProgress(int totalCount) {
        if (isTambangLogamClaimed) return;
        totalLogamMinedCount = totalCount;
        PlayerPrefs.SetInt("Saved_TotalLogamMinedCount", totalLogamMinedCount);
        PlayerPrefs.Save();

        // =============== TAMBAHAN PENGAMAN ===============
        // Jika pemain sudah menambang minimal 1 kali, otomatis hilangkan bantuan "Pergi ke Tambang"
        if (totalLogamMinedCount > 0 && barKeTambang != null && barKeTambang.activeSelf)
        {
            barKeTambang.SetActive(false);
        }
        // =================================================

        if (totalLogamMinedCount >= targetTambangLogam) {
            isTambangLogamDone = true;
            if (imgBtnTambangLogam != null) imgBtnTambangLogam.sprite = btnHijauAmbil;
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
            CekPemicuZakatEmasPerak();
        }

        if (barTambangLogam != null && barTambangLogam.activeSelf) {
            if (sliderTambangLogam != null) {
                sliderTambangLogam.maxValue = targetTambangLogam;
                sliderTambangLogam.value = totalLogamMinedCount;
            }
            if (txtTambangLogam != null) {
                txtTambangLogam.text = "Tambang Emas/Perak (" + totalLogamMinedCount.ToString() + "/" + targetTambangLogam.ToString() + ")";
            }
        }
    }

    public void CekPemicuZakatEmasPerak() {
        if (isTambangLogamDone && JurnalManager.instance != null && JurnalManager.instance.IsEmasPerakUnlocked()) {
            if (barKeKantorZakat != null && !barKeKantorZakat.activeSelf) {
                barKeKantorZakat.SetActive(true);
                barKeKantorZakat.transform.SetAsFirstSibling();
                if (txtKeKantorZakat != null) {
                    txtKeKantorZakat.text = "Pergi ke Kantor Zakat & Bayar Zakat Emas/Perak";
                }
                if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void CekPemicuZakatTernak() {
        // 🛑 KUNCI RE-TRIGGER: Jangan biarkan bar kantor zakat / notif nyala lagi kalau game sudah ending!
        if (isGameEnding) return;

        if (isIsiPakanDone && JurnalManager.instance != null && JurnalManager.instance.IsPeternakanUnlocked()) {
            if (barKeKantorZakat != null && !barKeKantorZakat.activeSelf) {
                barKeKantorZakat.SetActive(true);
                barKeKantorZakat.transform.SetAsFirstSibling();
                if (txtKeKantorZakat != null) {
                    txtKeKantorZakat.text = "Pergi ke Kantor Zakat & Bayar Zakat Hewan Ternak";
                }
                if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void AmbilHadiahTambangLogam() {
        if (isTambangLogamDone && !isTambangLogamClaimed) {
            isTambangLogamClaimed = true;
            PlayRewardEffects(rewardTambangLogam, btnAmbilTambangLogam.transform);
            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardTambangLogam);
            }
            btnAmbilTambangLogam.gameObject.SetActive(false);
            if (barTambangLogam != null) barTambangLogam.SetActive(false);
            SimpanProgressGameKeKomputer();
        }
    }

    // Fungsi Master Tunggal untuk Semua Jenis Tombol "Pergi" di Game Kamu
    public void FungsiMasterTombolPergi(string jenisLokasi)
    {
        // 1. Tutup panel misi lewat UIManager agar Gameplay-HUB bangun kembali
        if (misiPanel != null) {
            if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(misiPanel); 
            else misiPanel.SetActive(false);
        }
        if (asetBlur != null) asetBlur.SetActive(false);

        if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraTutupMisi != null) {
            InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraTutupMisi);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform targetTerpilih = null;

        // 2. Filter target 3D Cube dan nyalakan areanya berdasarkan parameter tombol
        if (jenisLokasi == "Zakat") {
            targetTerpilih = lokasiZakatCube;
        } 
        else if (jenisLokasi == "Tambang") {
            targetTerpilih = lokasiTambangCube;
        } 
        else if (jenisLokasi == "Toko") {
            targetTerpilih = lokasiTokoCube;
        } 
        else if (jenisLokasi == "IsiPakan") {
            targetTerpilih = lokasiIsiPakanCube;
        }

        // 3. Nyalakan objek tabung/cube tujuan di map biar kelihatan gradasinya
        if (targetTerpilih != null) {
            targetTerpilih.gameObject.SetActive(true);
        }

        // 4. Perintahkan panah 3D HUD untuk mengunci dan mengejar target tersebut secara interaktif
        if (ui3DArrowScript != null && targetTerpilih != null && player != null) {
            if (rawImageHUDArrow != null) rawImageHUDArrow.SetActive(true);
            ui3DArrowScript.SetTarget(targetTerpilih, player.transform);
        }
    }

    public void SetGameEndingBersih()
    {
        isGameEnding = true;

        // Padamkan seluruh bar misi tanpa kecuali
        if (barTebangJual != null) barTebangJual.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false);
        if (barJualNisab != null) barJualNisab.SetActive(false);
        if (barKeKantorZakat != null) barKeKantorZakat.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);
        if (barTambangLogam != null) barTambangLogam.SetActive(false);
        if (barBeliTernak != null) barBeliTernak.SetActive(false);
        if (barBeliPakan != null) barBeliPakan.SetActive(false);
        if (barIsiPakan != null) barIsiPakan.SetActive(false);
        if (barKeToko != null) barKeToko.SetActive(false);
        if (barKeIsiPakan != null) barKeIsiPakan.SetActive(false);

        // Kunci mati ikon notifikasi dan panah HUD agar tidak bisa dinyalakan oleh script lain
        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
        if (rawImageHUDArrow != null) rawImageHUDArrow.SetActive(false);

        if (ui3DArrowScript != null)
        {
            ui3DArrowScript.SetTarget(null, null);
        }

        Debug.Log("<color=red>[TaskManager]</color> Game Selesai! Arus UI Misi dikunci mati secara permanen.");
    }
}