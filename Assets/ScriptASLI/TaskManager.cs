using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TaskManager : MonoBehaviour {
    public static TaskManager instance;

    [Header("UI Panels & Notification")]
    public GameObject misiPanel;
    public GameObject asetBlur; 
    public GameObject ikonNotifikasi; 

    [Header("Misi Progress Logic")]
    private int woodOffset = 0;
    private bool isMisi2Started = false;
    
    [Header("Misi 1: Tebang + Jual")]
    public GameObject barTebangJual;
    public Button btnAmbilTebangJual;
    public Image imgBtnTebangJual;
    public TextMeshProUGUI txtTebangJual;
    public int rewardMisi1 = 5000;
    private bool isJualDone = false;
    private bool isMisi1Claimed = false;

    [Header("Misi 2: Tebang Pohon")]
    public GameObject barTebangPohon; 
    public Button btnAmbilTebangPohon; 
    public Image imgBtnTebangPohon;
    public Slider sliderTebang;
    public TextMeshProUGUI txtTebang;
    public int targetTebang = 5;
    public int rewardMisi2 = 10000;
    private bool isTebangDone = false;
    private bool isMisi2Claimed = false;

    [Header("Misi 3: Surat Edaran Kades (Babak 2)")]
    public GameObject barEdaranKades;       
    public Button btnBukaEdaranKades;       
    public GameObject panelEdaranKades;
    public GameObject asetBlurEdaran;
    
    public TextMeshProUGUI txtIsiEdaranKades; 
    public Button btnCloseEdaranKades;       
    [TextArea(3, 10)]
    public string teksLengkapEdaran;         
    public float kecepatanKetik = 0.05f;    
    public AudioClip suaraBukaSurat;         // Tarik SFX suara kertas/buka surat di Inspector
    public AudioClip suaraEmasDapat;         // Tarik SFX suara koin/emas didapat di Inspector
    public RectTransform posisiTargetEmasHUD; // Tarik objek UI "Emas" yang ada di pojok kiri atas HUD ke sini 

    private Coroutine typewriterCoroutine;
    private bool edaranSedangMengetik = false;

    [Header("Babak 3: Misi Peternakan")]
    public GameObject barKeToko; 
    public Button btnAmbilKeToko;
    public Image imgBtnKeToko; 
    public TextMeshProUGUI txtKeToko;
    public int rewardKeToko = 15000;
    private int beliHewanMisi1Count = 0; // 🔥 Tambahan baru untuk hitungan (0/3)
    private int targetBeliHewanMisi1 = 3;  // 🔥 Target 3 kali beli
    private bool isKeTokoDone = false;
    private bool isKeTokoClaimed = false;

    [Header("Babak 3: 3 Misi Serentak (Muncul setelah Misi 1 Claimed)")]
    [Header("Misi Beli Pakan")]
    public GameObject barBeliPakan; 
    public Button btnAmbilBeliPakan;
    public Image imgBtnBeliPakan; 
    public TextMeshProUGUI txtBeliPakan;
    public int rewardBeliPakan = 5000;
    private bool isBeliPakanDone = false;
    private bool isBeliPakanClaimed = false;

    [Header("Misi Isi Pakan")]
    public GameObject barIsiPakan; 
    public Button btnAmbilIsiPakan; 
    public Image imgBtnIsiPakan; 
    public TextMeshProUGUI txtIsiPakan;
    public int rewardIsiPakan = 10000; 
    private int isiPakanCount = 0;
    private int targetIsiPakan = 6;
    public bool isIsiPakanDone = false;
    private bool isIsiPakanClaimed = false; 

    [Header("Global UI Settings")]
    public Sprite btnAbuAbu; 
    public Sprite btnHijauAmbil; 
    public AudioClip suaraBukaMisi;    // 🔥 TAMBAHAN BARU
    public AudioClip suaraTutupMisi;

    void Awake() { instance = this; }

    void Start() {
        if (misiPanel != null) misiPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false); 
        if (panelEdaranKades != null) panelEdaranKades.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);
        if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false);
        
        isMisi2Started = false; 

        // Jalankan pengecekan awal via coroutine biar aman dari NullReferenceException
        StartCoroutine(JalankanPengecekanAwalGame());
    }

    private IEnumerator JalankanPengecekanAwalGame()
    {
        // Tunggu 1 frame agar seluruh file Manager (Awake) selesai loading di memori
        yield return new WaitForEndOfFrame();

        // Ambil status penanda restart dari Menu Home
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
        
        // 1. Reset Variabel Internal TaskManager
        woodOffset = 0; //
        isMisi2Started = false; //
        isJualDone = false; //
        isMisi1Claimed = false; //
        isTebangDone = false; //
        isMisi2Claimed = false; //
        beliHewanMisi1Count = 0; //
        isKeTokoDone = false; //
        isKeTokoClaimed = false; //
        isBeliPakanDone = false; //
        isBeliPakanClaimed = false; //
        isiPakanCount = 0; //
        isIsiPakanDone = false; //
        isIsiPakanClaimed = false; //

        // 2. Reset Data Keuangan Pemain (MoneyManager)
        if (MoneyManager.instance != null) //
        {
            MoneyManager.instance.totalMoney = 0; //
            MoneyManager.instance.totalEmas = 0; //
            MoneyManager.instance.totalPerak = 0; //
            MoneyManager.instance.UpdateEmasPerakUI();  //
        }

        // 3. Reset Isi Kantong Tas (InventoryManager)
        if (InventoryManager.instance != null) //
        {
            InventoryManager.instance.woodKecilCount = 0; //
            InventoryManager.instance.woodSedangCount = 0; //
            InventoryManager.instance.woodBesarCount = 0; //
            InventoryManager.instance.asetEmasCount = 0; //
            InventoryManager.instance.asetPerakCount = 0; //
            InventoryManager.instance.pakanRumputCount = 0; //
            InventoryManager.instance.totalWoodCollected = 0; //
            InventoryManager.instance.UpdateUI();  //
        }

        // 4. Reset Sistem & Keadaan Panel Zakat (ZakatPanelManager)
        if (ZakatPanelManager.instance != null) //
        {
            ZakatPanelManager.instance.isPerdaganganUnlocked = false; //
            ZakatPanelManager.instance.isEmasPerakUnlocked = false; //
            ZakatPanelManager.instance.isPeternakanUnlocked = false; //
            ZakatPanelManager.instance.isPerdaganganCompleted = false; //
            ZakatPanelManager.instance.isEmasPerakCompleted = false; //
            ZakatPanelManager.instance.isPeternakanCompleted = false; //
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();  //
        }

        // 5. 🏃‍♂️ TELEPORT PLAYER KE KOORDINAT SPAWN AWAL (HALAMAN FARM)
        GameObject player = GameObject.FindGameObjectWithTag("Player"); 
        if (player != null) 
        {
            CharacterController cc = player.GetComponent<CharacterController>(); 
            if (cc != null) cc.enabled = false;  

            // 🔥 MASUKKAN KOORDINAT FARM KAMU DI SINI (Ganti angka di bawah sesuai Inspector Unity-mu)
            player.transform.position = new Vector3(8.751f, 0.44f, -64.016f);  
            
            // Atur rotasi hadapan player saat bangun (Quaternion.Euler(Y, X, Z))
            // Angka 180f berarti player otomatis menghadap membelakangi rumah/menghadap jalan
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f); 

            if (cc != null) cc.enabled = true;
            
            // 6. 🎥 RESET POSISI KAMERA CINEMACHINE BIAR SINKRON DI BELAKANG PLAYER
            // Mencari objek dengan komponen PlayerMovement untuk mengakses transform kamera pendukung
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null && pm.cameraTransform != null)
            {
                // Mengatur rotasi target orbit kamera Cinemachine kembali menghadap ke depan default
                // (Mencegah kamera melintir ke sudut aneh sisa game sebelumnya)
                pm.cameraTransform.position = new Vector3(0, 2f, -5f); 
                pm.cameraTransform.rotation = Quaternion.identity;
            }
        }

        // 7. Reset Pembukuan Jurnal Balik Terkunci Semula (JurnalManager)
        if (JurnalManager.instance != null) //
        {
            JurnalManager.instance.isNisabReached = false; //
            JurnalManager.instance.isHaulComplete = false; //
            JurnalManager.instance.isZakatPaid = false; //
            JurnalManager.instance.isDagangLockedInJurnal = false; //
            if (JurnalManager.instance.haulSlider != null) JurnalManager.instance.haulSlider.value = 0f; //

            JurnalManager.instance.isEmasPerakNisabReached = false; //
            JurnalManager.instance.isEmasPerakHaulComplete = false; //
            JurnalManager.instance.isEmasPerakZakatPaid = false; //
            JurnalManager.instance.isEmasLockedInJurnal = false; //
            if (JurnalManager.instance.visualHalamanLock != null) JurnalManager.instance.visualHalamanLock.SetActive(true); //
            if (JurnalManager.instance.visualHalamanUnlock != null) JurnalManager.instance.visualHalamanUnlock.SetActive(false); //
            if (JurnalManager.instance.navCoinLeftPanel != null) JurnalManager.instance.navCoinLeftPanel.SetActive(false); //
            if (JurnalManager.instance.haulSliderEmasPerak != null) JurnalManager.instance.haulSliderEmasPerak.value = 0f; //

            JurnalManager.instance.isTernakNisabReached = false; //
            JurnalManager.instance.isTernakHaulComplete = false; //
            JurnalManager.instance.isTernakZakatPaid = false; //
            JurnalManager.instance.isTernakLockedInJurnal = false; //
            if (JurnalManager.instance.panelLockTernak != null) JurnalManager.instance.panelLockTernak.SetActive(true); //
            if (JurnalManager.instance.panelUnlockTernak != null) JurnalManager.instance.panelUnlockTernak.SetActive(false); //
            if (JurnalManager.instance.haulSliderTernak != null) JurnalManager.instance.haulSliderTernak.value = 0f; //

            JurnalManager.instance.MatikanSistemBeranak();  //
            JurnalManager.instance.StopAllCoroutines(); //
            JurnalManager.instance.ShowPage(1);  //
        }
    }

    // --- FUNGSI LOAD DATA JIKA MELANJUTKAN GAME ---
    private void LoadProgressMisiTerakhir()
    {
        Debug.Log("<color=green>[TaskManager]</color> Melanjutkan game dari data terakhir.");

        // 1. Muat Progress Misi dari PlayerPrefs (Default kembali ke Misi 1 jika baru pertama main)
        isMisi1Claimed = PlayerPrefs.GetInt("Saved_IsMisi1Claimed", 0) == 1;
        isJualDone = PlayerPrefs.GetInt("Saved_IsJualDone", 0) == 1;
        isMisi2Started = PlayerPrefs.GetInt("Saved_IsMisi2Started", 0) == 1;
        isTebangDone = PlayerPrefs.GetInt("Saved_IsTebangDone", 0) == 1;
        isMisi2Claimed = PlayerPrefs.GetInt("Saved_IsMisi2Claimed", 0) == 1;
        
        // Babak 3
        isKeTokoDone = PlayerPrefs.GetInt("Saved_IsKeTokoDone", 0) == 1;
        isKeTokoClaimed = PlayerPrefs.GetInt("Saved_IsKeTokoClaimed", 0) == 1;
        beliHewanMisi1Count = PlayerPrefs.GetInt("Saved_BeliHewanCount", 0);
        isBeliPakanDone = PlayerPrefs.GetInt("Saved_IsBeliPakanDone", 0) == 1;
        isBeliPakanClaimed = PlayerPrefs.GetInt("Saved_IsBeliPakanClaimed", 0) == 1;
        isiPakanCount = PlayerPrefs.GetInt("Saved_IsiPakanCount", 0);
        isIsiPakanDone = PlayerPrefs.GetInt("Saved_IsIsiPakanDone", 0) == 1;
        isIsiPakanClaimed = PlayerPrefs.GetInt("Saved_IsIsiPakanClaimed", 0) == 1;

        // 2. Muat Nilai Offset Kayu & Uang Terakhir
        woodOffset = PlayerPrefs.GetInt("Saved_WoodOffset", 0);
        if (MoneyManager.instance != null)
        {
            MoneyManager.instance.totalMoney = PlayerPrefs.GetInt("JumlahUangPemain", 0);
            MoneyManager.instance.totalEmas = PlayerPrefs.GetInt("EmasPemain", 0);
            MoneyManager.instance.totalPerak = PlayerPrefs.GetInt("Saved_PerakPemain", 0);
            MoneyManager.instance.UpdateEmasPerakUI();
        }

        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.woodKecilCount = PlayerPrefs.GetInt("Saved_WoodKecil", 0);
            InventoryManager.instance.woodSedangCount = PlayerPrefs.GetInt("Saved_WoodSedang", 0);
            InventoryManager.instance.woodBesarCount = PlayerPrefs.GetInt("Saved_WoodBesar", 0);
            InventoryManager.instance.asetEmasCount = PlayerPrefs.GetInt("Saved_AsetEmas", 0);
            InventoryManager.instance.asetPerakCount = PlayerPrefs.GetInt("Saved_AsetPerak", 0);
            InventoryManager.instance.pakanRumputCount = PlayerPrefs.GetInt("Saved_PakanRumput", 0);
            InventoryManager.instance.totalWoodCollected = PlayerPrefs.GetInt("TotalKayuDitebang", 0);
            InventoryManager.instance.UpdateUI();
        }

        // 3. Setel Keaktifan Bar UI Sesuai Progress yang Dimuat
        if (barTebangJual != null) barTebangJual.SetActive(!isMisi1Claimed);
        if (barTebangPohon != null) barTebangPohon.SetActive(isMisi2Started && !isMisi2Claimed);
        
        // Pemicu Babak 3 jika sudah masuk jalurnya
        if (isMisi2Claimed && !isIsiPakanClaimed)
        {
            if (barKeToko != null) barKeToko.SetActive(!isKeTokoClaimed);
            if (barBeliPakan != null) barBeliPakan.SetActive(!isBeliPakanClaimed);
            if (barIsiPakan != null) barIsiPakan.SetActive(!isIsiPakanClaimed);
        }
    }

    // --- FUNGSI AUTO SAVE (PANGGIL SETIAP KALI PROGRESS BERUBAH) ---
    public void SimpanProgressGameKeKomputer()
    {
        // Simpan Status Booleans Alur Misi
        PlayerPrefs.SetInt("Saved_IsMisi1Claimed", isMisi1Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsJualDone", isJualDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsMisi2Started", isMisi2Started ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsTebangDone", isTebangDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsMisi2Claimed", isMisi2Claimed ? 1 : 0);
        
        PlayerPrefs.SetInt("Saved_IsKeTokoDone", isKeTokoDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsKeTokoClaimed", isKeTokoClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_BeliHewanCount", beliHewanMisi1Count);
        PlayerPrefs.SetInt("Saved_IsBeliPakanDone", isBeliPakanDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsBeliPakanClaimed", isBeliPakanClaimed ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsiPakanCount", isiPakanCount);
        PlayerPrefs.SetInt("Saved_IsIsiPakanDone", isIsiPakanDone ? 1 : 0);
        PlayerPrefs.SetInt("Saved_IsIsiPakanClaimed", isIsiPakanClaimed ? 1 : 0);

        PlayerPrefs.SetInt("Saved_WoodOffset", woodOffset);

        // Simpan Aset Keuangan Permanen
        if (MoneyManager.instance != null)
        {
            PlayerPrefs.SetInt("JumlahUangPemain", MoneyManager.instance.totalMoney);
            PlayerPrefs.SetInt("EmasPemain", MoneyManager.instance.totalEmas);
            PlayerPrefs.SetInt("Saved_PerakPemain", MoneyManager.instance.totalPerak);
        }

        // Simpan Aset Tas Inventory Permanen
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
        // 🔥 TAMBAHAN: Suara saat panel Misi DIBUKA
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
        }
    }

    public void CloseMisi() {
        // 🔥 TAMBAHAN: Suara saat panel Misi DITUTUP
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
        if (isMisi1Claimed) return;
        isJualDone = true;

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

    // 🔥 MODIFIKASI: Ambil Hadiah Misi 1 + Efek Koin & Suara
    public void AmbilHadiahTebangJual() {
        if (isJualDone && !isMisi1Claimed) {
            isMisi1Claimed = true;

            // Efek Suara dan Koin Terbang dari Posisi Tombol
            PlayRewardEffects(rewardMisi1, btnAmbilTebangJual.transform);

            if (InventoryManager.instance != null) {
                woodOffset = InventoryManager.instance.totalWoodCollected;
            }

            isMisi2Started = true; 

            if (barTebangPohon != null) {
                barTebangPohon.SetActive(true);
                barTebangPohon.transform.SetAsFirstSibling(); 
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected); 
            }

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardMisi1);
            btnAmbilTebangJual.gameObject.SetActive(false);
            txtTebangJual.text = "Misi Selesai!";
        }
    }

    public void UpdateTebangProgress(int totalCount) {
        // 🔥 PERBAIKAN UTAMA: Pindahkan ke baris paling atas agar tebangan pertama di Misi 1 langsung memicu cerita
        if (totalCount >= 1 && PlayerPrefs.GetInt("Panel17Selesai", 0) == 0) {
            PlayerPrefs.SetInt("Panel17Selesai", 1);
            PlayerPrefs.Save();

            if (IntroStoryManager.instance != null) {
                IntroStoryManager.instance.TriggerPanel17SelesaiTebang();
            }
        }

        if (isMisi2Claimed) return; 

        if (barTebangPohon != null && barTebangPohon.activeSelf && isMisi2Started) {
            int progressMisiSekarang = totalCount - woodOffset; 
            if (progressMisiSekarang < 0) progressMisiSekarang = 0;

            sliderTebang.maxValue = targetTebang;
            sliderTebang.value = progressMisiSekarang;
            txtTebang.text = "Tebang Pohon (" + progressMisiSekarang.ToString() + "/" + targetTebang.ToString() + ")";

            // Baris deteksi yang lama di sini bisa dihapus karena sudah dipindahkan ke atas!

            if (progressMisiSekarang >= 15) {
                if (ReminderManager.instance != null) {
                    ReminderManager.instance.TriggerJualKayuReminder();
                }
            }

            if (progressMisiSekarang >= targetTebang) {
                isTebangDone = true;
                imgBtnTebangPohon.sprite = btnHijauAmbil;
                if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                    ikonNotifikasi.SetActive(true);
                }
            }
        }
    }

    // 🔥 MODIFIKASI: Ambil Hadiah Misi 2 + Efek Koin & Suara
    public void AmbilHadiahTebangPohon() {
        if (isTebangDone && !isMisi2Claimed) {
            isMisi2Claimed = true;

            // Efek Suara dan Koin Terbang dari Posisi Tombol
            PlayRewardEffects(rewardMisi2, btnAmbilTebangPohon.transform);

            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardMisi2);
            }
            btnAmbilTebangPohon.gameObject.SetActive(false);
            txtTebang.text = "Misi Selesai!";
        }
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
        // 🔥 TAMBAHAN: Mainkan suara buka surat saat tombol diklik
        if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraBukaSurat != null) {
            InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraBukaSurat);
        }

        if (panelEdaranKades != null) {
            if (misiPanel != null) misiPanel.SetActive(false);
            // 🔥 Aktifkan blur mandiri milik edaran kades dan taruh di paling belakang
        if (asetBlurEdaran != null) {
            asetBlurEdaran.SetActive(true);
            asetBlurEdaran.transform.SetAsFirstSibling(); // visual paling belakang
        }

        panelEdaranKades.SetActive(true);
        panelEdaranKades.transform.SetAsLastSibling(); // Surat didorong ke paling depan
           
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
            foreach (char huruf in teksLengkapEdaran.ToCharArray()) {
                txtIsiEdaranKades.text += huruf;
                yield return new WaitForSeconds(kecepatanKetik); 
            }
            edaranSedangMengetik = false; 
            
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(true);
            }
        }
    }

    public void SkipKetikEdaran()
    {
        if (edaranSedangMengetik)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

            if (txtIsiEdaranKades != null)
            {
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

            if (UIManager.instance != null) {
                UIManager.instance.ClosePanelMenu(panelEdaranKades);
            } else {
                panelEdaranKades.SetActive(false);
            }
            if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false);

            if (btnBukaEdaranKades != null) {
                btnBukaEdaranKades.gameObject.SetActive(false); 
            }

            if (Level2Manager.instance != null && Level2Manager.instance.koinLevel2Container != null) {
                Level2Manager.instance.koinLevel2Container.SetActive(true);
            }

            if (MoneyManager.instance != null) {
                MoneyManager.instance.totalEmas += 5; 
                MoneyManager.instance.UpdateEmasPerakUI(); 
            }

            if (Level2Manager.instance != null && Level2Manager.instance.txtEmasUtama != null) {
                Level2Manager.instance.txtEmasUtama.text = MoneyManager.instance.totalEmas + " gr";
            }

            // 🔥 PERBAIKAN UTAMA: EFEK AUDIO DAN ANIMASI TEKS PLUS EMAS PRESISI
            if (TokoManager.instance != null && TokoManager.instance.prefabTeksMinusAnim != null && posisiTargetEmasHUD != null)
            {
                // 1. Mainkan suara dencing koin/emas didapat
                if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraEmasDapat != null)
                {
                    InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraEmasDapat);
                }

                // 2. Munculkan prefab animasi teks, spawn langsung di bawah Nav_emasperak
                // Kita gunakan parameter 'false' agar koordinat local prefab tidak rusak saat menempel ke parent baru
                GameObject teksPlusObj = Instantiate(TokoManager.instance.prefabTeksMinusAnim, posisiTargetEmasHUD, false);

                // 3. Ambil Sprite dari EmasIcon secara otomatis untuk mengganti visual koin bawaan prefab
                Image targetEmasImage = posisiTargetEmasHUD.GetComponentInChildren<Image>(); // Mengambil gambar EmasIcon
                Image prefabImageComponent = teksPlusObj.GetComponentInChildren<Image>(); // Mengambil gambar koin di prefab

                if (targetEmasImage != null && prefabImageComponent != null)
                {
                    prefabImageComponent.sprite = targetEmasImage.sprite; // Ganti koin jadi emas!
                }

                // 4. Masukkan teks "+5 Gram" ke dalam komponen animasinya
                TeksMinusAnim komponenAnim = teksPlusObj.GetComponent<TeksMinusAnim>();
                if (komponenAnim != null)
                {
                    komponenAnim.SetupTeksMinus("+5 gr"); 
                    
                    // Ubah warna teks menjadi hijau secara dinamis
                    TMP_Text komponenTeksTMP = teksPlusObj.GetComponentInChildren<TMP_Text>();
                    if (komponenTeksTMP != null) komponenTeksTMP.color = Color.green;
                }

                // 5. Paksa posisi transform-nya berada tepat di tengah-tengah area objek emas agar tidak lari ke ujung
                RectTransform rectAnim = teksPlusObj.GetComponent<RectTransform>();
                if (rectAnim != null)
                {
                    rectAnim.anchoredPosition = Vector2.zero; // Reset posisi relatif terhadap EmasIcon / Nav_emasperak
                }
                teksPlusObj.transform.SetParent(posisiTargetEmasHUD.parent, true); // Pindah ke hierarki Nav_emasperak
                teksPlusObj.transform.SetAsLastSibling(); // Paksa duduk di urutan paling bawah hierarki (Layer Terdepan!)
            }
        }
    }

    public void MulaiMisiBabak3()
    {
        if (barTebangJual != null) barTebangJual.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);

        // --- 1. AKTIFKAN 3 MISI SEKALIGUS ---
        if (barKeToko != null) barKeToko.SetActive(true);
        if (barBeliPakan != null) barBeliPakan.SetActive(true);
        if (barIsiPakan != null) barIsiPakan.SetActive(true);

        // --- 2. URUTKAN HIRARKI UI (Atas ke Bawah) ---
        if (barKeToko != null) barKeToko.transform.SetAsLastSibling();
        if (barBeliPakan != null) barBeliPakan.transform.SetAsLastSibling();
        if (barIsiPakan != null) barIsiPakan.transform.SetAsLastSibling();

        // --- 3. INITIALIZATION VISUAL MISI ---
        // Misi 1: Pergi ke Toko & Beli Hewan Ternak (0/3)
        beliHewanMisi1Count = 0; // Reset ke 0 saat masuk babak baru
        if (btnAmbilKeToko != null) btnAmbilKeToko.gameObject.SetActive(true); 
        if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnAbuAbu; 
        if (txtKeToko != null) txtKeToko.text = $"Pergi ke toko & beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";

        // Misi 2: Beli Pakan
        if (btnAmbilBeliPakan != null) btnAmbilBeliPakan.gameObject.SetActive(true);
        if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnAbuAbu;
        if (txtBeliPakan != null) txtBeliPakan.text = "Beli pakan di toko";

        // Misi 3: Isi Pakan
        if (btnAmbilIsiPakan != null) btnAmbilIsiPakan.gameObject.SetActive(true);
        if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnAbuAbu;
        if (txtIsiPakan != null) txtIsiPakan.text = "Isi Pakan Hewan di peternakan";
        isiPakanCount = 0;

        if (ikonNotifikasi != null && !misiPanel.activeSelf) {
            ikonNotifikasi.SetActive(true);
        }
    }

    // 🔥 MODIFIKASI: Ambil Hadiah Misi Ke Toko tanpa memicu aktivasi bar pakan lagi
    public void KlaimRewardKeToko()
    {
        if (!isKeTokoDone) return; 

        if (isKeTokoDone && !isKeTokoClaimed) 
        {
            isKeTokoClaimed = true; 

            // Efek Suara dan Koin Terbang dari Posisi Tombol
            PlayRewardEffects(rewardKeToko, btnAmbilKeToko.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardKeToko); 
            if (barKeToko != null) barKeToko.SetActive(false); // Sembunyikan bar ini setelah diklaim

            // Catatan: Pemanggilan barBeliPakan dan barIsiPakan .SetActive(true) 
            // sudah dihapus dari sini karena dipindahkan langsung ke MulaiMisiBabak3()
            
            CekSemuaMisiBabak3Selesai();
        }
    }

    public void NotifyBeliPakan()
    {
        if (isKeTokoClaimed && !isBeliPakanDone)
        {
            isBeliPakanDone = true;
            if (txtBeliPakan != null) txtBeliPakan.text = "Selesai membeli paket pakan!";
            
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnHijauAmbil; 
            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }
    }

    public void NotifyHewanDibeli()
    {
        // Hanya berjalan jika misi belum selesai dilakukan
        if (!isKeTokoDone && barKeToko != null && barKeToko.activeSelf)
        {
            beliHewanMisi1Count++;
            if (beliHewanMisi1Count > targetBeliHewanMisi1) beliHewanMisi1Count = targetBeliHewanMisi1;

            // Update teks hitungan secara realtime
            if (txtKeToko != null) txtKeToko.text = $"Pergi ke toko & beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";
            
            // Jika sudah mencapai target 3 kali beli
            if (beliHewanMisi1Count >= targetBeliHewanMisi1)
            {
                isKeTokoDone = true;
                if (txtKeToko != null) txtKeToko.text = "Selesai pergi ke toko & beli hewan ternak!";
                if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnHijauAmbil; 
                
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void NotifyIsiPakanWorld3D()
    {
        if (isKeTokoClaimed && !isIsiPakanDone)
        {
            isiPakanCount++;
            if (isiPakanCount > targetIsiPakan) isiPakanCount = targetIsiPakan;

            if (txtIsiPakan != null) txtIsiPakan.text = $"Sedang mengisi ulang tempat makanan 3D...";

            if (isiPakanCount >= targetIsiPakan)
            {
                isIsiPakanDone = true;
                if (txtIsiPakan != null) txtIsiPakan.text = "Selesai mengisi pakan hewan!";
                
                if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnHijauAmbil; 
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    // 🔥 MODIFIKASI: Ambil Hadiah Beli Pakan + Efek Koin & Suara
    public void KlaimRewardBeliPakan()
    {
        if (!isBeliPakanDone) return;

        if (isBeliPakanDone && !isBeliPakanClaimed)
        {
            isBeliPakanClaimed = true;

            // Efek Suara dan Koin Terbang dari Posisi Tombol
            PlayRewardEffects(rewardBeliPakan, btnAmbilBeliPakan.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardBeliPakan);
            if (barBeliPakan != null) barBeliPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    // 🔥 MODIFIKASI: Ambil Hadiah Isi Pakan + Efek Koin & Suara
    public void KlaimRewardIsiPakan()
    {
        if (!isIsiPakanDone) return;

        if (isIsiPakanDone && !isIsiPakanClaimed)
        {
            isIsiPakanClaimed = true;

            // Efek Suara dan Koin Terbang dari Posisi Tombol
            PlayRewardEffects(rewardIsiPakan, btnAmbilIsiPakan.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardIsiPakan);
            if (barIsiPakan != null) barIsiPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    // 🔥 FUNGSI BARU: Jembatan Instan Menggunakan Efek Koin Terbang & Suara dari InventoryManager
    private void PlayRewardEffects(int rewardAmount, Transform buttonTransform)
    {
        if (InventoryManager.instance != null)
        {
            // 1. Ambil fungsi bawaan SpawnUICoin milik InventoryManager (Harta, PosisiTombolMisi)
            // Menggunakan parent dari panel kuis/misi agar koin muncul di layer UI terdepan
            InventoryManager.instance.Invoke("SpawnUICoin", 0f); 
            
            // Replikasi logika internal koin terbang milik InventoryManager agar bekerja sinkron di TaskPanel
            if (InventoryManager.instance.uiCoinPrefab != null && InventoryManager.instance.navCoinTarget != null)
            {
                int jumlahKoin = 5;
                for (int i = 0; i < jumlahKoin; i++)
                {
                    GameObject coin = Instantiate(InventoryManager.instance.uiCoinPrefab, misiPanel.transform.parent);
                    coin.transform.SetAsLastSibling();
                    coin.transform.position = buttonTransform.position; // Keluar tepat dari tombol klaim yang diklik

                    UICoinEffect effect = coin.GetComponent<UICoinEffect>();
                    if (effect == null) effect = coin.AddComponent<UICoinEffect>();
                    
                    int nilaiPerKoin = (i == 0) ? rewardAmount : 0;
                    effect.Init(InventoryManager.instance.navCoinTarget, nilaiPerKoin);
                }
            }

            // 2. Mainkan sound effect jual koin yang nempel di InventoryManager
            if (InventoryManager.instance.audioSourceInventory != null && InventoryManager.instance.suaraJualKoin != null)
            {
                InventoryManager.instance.audioSourceInventory.PlayOneShot(InventoryManager.instance.suaraJualKoin);
            }
        }
    }

    private void CekSemuaMisiBabak3Selesai()
    {
        if (isBeliPakanClaimed && isIsiPakanClaimed)
        {
            Debug.Log("<color=cyan>[Task Manager]</color> Babak 3 SELESAI MUTLAK!");
        }
    }
    
}