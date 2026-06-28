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
    public AudioClip suaraBukaSurat;         
    public AudioClip suaraEmasDapat;         
    public RectTransform posisiTargetEmasHUD; 

    [Header("Misi 4: Tambang Emas/Perak (Babak 2)")]
    public GameObject barTambangLogam;       // Bar UI Baru untuk Misi Tambang
    public Button btnAmbilTambangLogam;       // Tombol Ambil Hadiah
    public Image imgBtnTambangLogam;         // Gambar Tombol Ambil
    public Slider sliderTambangLogam;         // Slider Progress
    public TextMeshProUGUI txtTambangLogam;   // Teks UI Misi (0/15)
    public int targetTambangLogam = 15;       // Target 15 kali
    public int rewardTambangLogam = 5000000;  // Hadiah 5 Juta Rupiah
    [HideInInspector] public int totalLogamMinedCount = 0; // Hitungan progress saat ini
    private bool isTambangLogamDone = false;
    private bool isTambangLogamClaimed = false;

    private Coroutine typewriterCoroutine;
    private bool edaranSedangMengetik = false;

    [Header("Babak 3: Misi Peternakan")]
    public GameObject barKeToko; 
    public Button btnAmbilKeToko;
    public Image imgBtnKeToko; 
    public TextMeshProUGUI txtKeToko;
    public int rewardKeToko = 15000;
    private int beliHewanMisi1Count = 0; 
    private int targetBeliHewanMisi1 = 3;  
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
    public AudioClip suaraBukaMisi;    
    public AudioClip suaraTutupMisi;

    void Awake() { instance = this; }

    void Start() {
        if (barTambangLogam != null) barTambangLogam.SetActive(false);
        if (misiPanel != null) misiPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false); 
        if (panelEdaranKades != null) panelEdaranKades.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);
        if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false);
        
        isMisi2Started = false; 

        StartCoroutine(JalankanPengecekanAwalGame());
    }

    private IEnumerator JalankanPengecekanAwalGame()
    {
        yield return new WaitForEndOfFrame();

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
        
        isKeTokoDone = PlayerPrefs.GetInt("Saved_IsKeTokoDone", 0) == 1;
        isKeTokoClaimed = PlayerPrefs.GetInt("Saved_IsKeTokoClaimed", 0) == 1;
        beliHewanMisi1Count = PlayerPrefs.GetInt("Saved_BeliHewanCount", 0);
        isBeliPakanDone = PlayerPrefs.GetInt("Saved_IsBeliPakanDone", 0) == 1;
        isBeliPakanClaimed = PlayerPrefs.GetInt("Saved_IsBeliPakanClaimed", 0) == 1;
        isiPakanCount = PlayerPrefs.GetInt("Saved_IsiPakanCount", 0);
        isIsiPakanDone = PlayerPrefs.GetInt("Saved_IsIsiPakanDone", 0) == 1;
        isIsiPakanClaimed = PlayerPrefs.GetInt("Saved_IsIsiPakanClaimed", 0) == 1;
        // 🔥 LOAD PROGRESS TAMBANG BABAK 2
        isTambangLogamDone = PlayerPrefs.GetInt("Saved_IsTambangLogamDone", 0) == 1;
        isTambangLogamClaimed = PlayerPrefs.GetInt("Saved_IsTambangLogamClaimed", 0) == 1;
        totalLogamMinedCount = PlayerPrefs.GetInt("Saved_TotalLogamMinedCount", 0);

        if (barTambangLogam != null) {
            // Muncul jika Surat Edaran sudah ditutup (btnBukaEdaranKades sudah nonaktif) dan belum diklaim
            bool edaranSelesai = PlayerPrefs.GetInt("Saved_EdaranSelesai", 0) == 1;
            barTambangLogam.SetActive(edaranSelesai && !isTambangLogamClaimed);
            if (barTambangLogam.activeSelf) UpdateTambangLogamProgress(totalLogamMinedCount);
        }

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

        if (barTebangJual != null) barTebangJual.SetActive(!isMisi1Claimed);
        
        // 🔥 PERBAIKAN LOAD: Bar Tebang Pohon langsung muncul jika Misi 1 SUDAH SELESAI DITEBANG/DIJUAL, tidak perlu nunggu diklaim
        if (barTebangPohon != null) barTebangPohon.SetActive(isJualDone && !isMisi2Claimed);
        
        if (isMisi2Claimed && !isIsiPakanClaimed)
        {
            if (barKeToko != null) barKeToko.SetActive(!isKeTokoClaimed);
            if (barBeliPakan != null) barBeliPakan.SetActive(!isBeliPakanClaimed);
            if (barIsiPakan != null) barIsiPakan.SetActive(!isIsiPakanClaimed);
        }
    }

    public void SimpanProgressGameKeKomputer()
    {
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
        PlayerPrefs.SetInt("Saved_IsiPakanClaimed", isIsiPakanClaimed ? 1 : 0);
        // 🔥 SIMPAN PROGRESS TAMBANG BABAK 2
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
            // 🔥 TAMBAHAN PENGAMAN: Segarkan tampilan visual bar tambang logam saat panel dibuka
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

    // 🔥 PERBAIKAN LOGIKA: Begitu kayu terjual, langsung amankan isi woodOffset & aktifkan Misi 2
    public void NotifyWoodSold() {
        if (isJualDone) return; 
        isJualDone = true;

        // Kunci offset kayu di sini saat ini juga agar Misi 2 langsung menghitung sisa tebangan dengan benar
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

            // Penguncian woodOffset & isMisi2Started di sini dihapus karena sudah di-handle realtime di NotifyWoodSold()

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

        if (progressMisiSekarang >= 15) {
            if (ReminderManager.instance != null) {
                ReminderManager.instance.TriggerJualKayuReminder(); 
            }
        }

        // 🔥 PASTIKAN DI SINI: Teksnya murni "Tebang Pohon", jangan sampai ketulis Tambang!
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

            if (TokoManager.instance != null && TokoManager.instance.prefabTeksMinusAnim != null && posisiTargetEmasHUD != null)
            {
                if (InventoryManager.instance != null && InventoryManager.instance.audioSourceInventory != null && suaraEmasDapat != null)
                {
                    InventoryManager.instance.audioSourceInventory.PlayOneShot(suaraEmasDapat);
                }

                GameObject teksPlusObj = Instantiate(TokoManager.instance.prefabTeksMinusAnim, posisiTargetEmasHUD, false);

                Image targetEmasImage = posisiTargetEmasHUD.GetComponentInChildren<Image>(); 
                Image prefabImageComponent = teksPlusObj.GetComponentInChildren<Image>(); 

                if (targetEmasImage != null && prefabImageComponent != null)
                {
                    prefabImageComponent.sprite = targetEmasImage.sprite; 
                }

                TeksMinusAnim komponenAnim = teksPlusObj.GetComponent<TeksMinusAnim>();
                if (komponenAnim != null)
                {
                    komponenAnim.SetupTeksMinus("+5 gr"); 
                    
                    TMP_Text komponenTeksTMP = teksPlusObj.GetComponentInChildren<TMP_Text>();
                    if (komponenTeksTMP != null) komponenTeksTMP.color = Color.green;
                }

                RectTransform rectAnim = teksPlusObj.GetComponent<RectTransform>();
                if (rectAnim != null)
                {
                    rectAnim.anchoredPosition = Vector2.zero; 
                }
                teksPlusObj.transform.SetParent(posisiTargetEmasHUD.parent, true); 
                teksPlusObj.transform.SetAsLastSibling(); 
            }
            // 🔥 AKTIFKAN MISI TAMBANG EMAS/PERAK
            PlayerPrefs.SetInt("Saved_EdaranSelesai", 1); // Tandai edaran kades clear
            PlayerPrefs.Save();

            if (barEdaranKades != null) barEdaranKades.SetActive(false); // Hilangkan bar edaran

            if (barTambangLogam != null) {
                barTambangLogam.SetActive(true);
                barTambangLogam.transform.SetAsFirstSibling();
                UpdateTambangLogamProgress(totalLogamMinedCount); // Set tulisan (0/15) awal
            }

            if (ikonNotifikasi != null && !misiPanel.activeSelf) {
                ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void MulaiMisiBabak3()
    {
        if (barTebangJual != null) barTebangJual.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);

        if (barKeToko != null) barKeToko.SetActive(true);
        if (barBeliPakan != null) barBeliPakan.SetActive(true);
        if (barIsiPakan != null) barIsiPakan.SetActive(true);

        if (barKeToko != null) barKeToko.transform.SetAsLastSibling();
        if (barBeliPakan != null) barBeliPakan.transform.SetAsLastSibling();
        if (barIsiPakan != null) barIsiPakan.transform.SetAsLastSibling();

        beliHewanMisi1Count = 0; 
        if (btnAmbilKeToko != null) btnAmbilKeToko.gameObject.SetActive(true); 
        if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnAbuAbu; 
        if (txtKeToko != null) txtKeToko.text = $"Pergi ke toko & beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";

        if (btnAmbilBeliPakan != null) btnAmbilBeliPakan.gameObject.SetActive(true);
        if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnAbuAbu;
        if (txtBeliPakan != null) txtBeliPakan.text = "Beli pakan di toko";

        if (btnAmbilIsiPakan != null) btnAmbilIsiPakan.gameObject.SetActive(true);
        if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnAbuAbu;
        if (txtIsiPakan != null) txtIsiPakan.text = "Isi Pakan Hewan di peternakan";
        isiPakanCount = 0;

        if (ikonNotifikasi != null && !misiPanel.activeSelf) {
            ikonNotifikasi.SetActive(true);
        }
    }

    public void KlaimRewardKeToko()
    {
        if (!isKeTokoDone) return; 

        if (isKeTokoDone && !isKeTokoClaimed) 
        {
            isKeTokoClaimed = true; 

            PlayRewardEffects(rewardKeToko, btnAmbilKeToko.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardKeToko); 
            if (barKeToko != null) barKeToko.SetActive(false); 
            
            CekSemuaMisiBabak3Selesai();
        }
    }

    // 🔥 PERBAIKAN LOGIKA: Hapus syarat 'isKeTokoClaimed' agar misi beli pakan bisa dicicil langsung
    public void NotifyBeliPakan()
    {
        if (!isBeliPakanDone)
        {
            isBeliPakanDone = true;
            if (txtBeliPakan != null) txtBeliPakan.text = "Selesai membeli paket pakan!";
            
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnHijauAmbil; 
            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }
    }

    public void NotifyHewanDibeli()
    {
        if (!isKeTokoDone && barKeToko != null && barKeToko.activeSelf)
        {
            beliHewanMisi1Count++;
            if (beliHewanMisi1Count > targetBeliHewanMisi1) beliHewanMisi1Count = targetBeliHewanMisi1;

            if (txtKeToko != null) txtKeToko.text = $"Pergi ke toko & beli hewan ternak ({beliHewanMisi1Count}/{targetBeliHewanMisi1})";
            
            if (beliHewanMisi1Count >= targetBeliHewanMisi1)
            {
                isKeTokoDone = true;
                if (txtKeToko != null) txtKeToko.text = "Selesai pergi ke toko & beli hewan ternak!";
                if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnHijauAmbil; 
                
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    // 🔥 PERBAIKAN LOGIKA: Hapus syarat 'isKeTokoClaimed' agar misi pengisian pakan 3D bisa mendata kemajuan secara realtime
    public void NotifyIsiPakanWorld3D()
    {
        if (!isIsiPakanDone)
        {
            isiPakanCount++;
            if (isiPakanCount > targetIsiPakan) isiPakanCount = targetIsiPakan;

            if (txtIsiPakan != null) txtIsiPakan.text = $"Mengisi pakan hewan ({isiPakanCount}/{targetIsiPakan})";

            if (isiPakanCount >= targetIsiPakan)
            {
                isIsiPakanDone = true;
                if (txtIsiPakan != null) txtIsiPakan.text = "Selesai mengisi pakan hewan!";
                
                if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnHijauAmbil; 
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void KlaimRewardBeliPakan()
    {
        if (!isBeliPakanDone) return;

        if (isBeliPakanDone && !isBeliPakanClaimed)
        {
            isBeliPakanClaimed = true;

            PlayRewardEffects(rewardBeliPakan, btnAmbilBeliPakan.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardBeliPakan);
            if (barBeliPakan != null) barBeliPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    public void KlaimRewardIsiPakan()
    {
        if (!isIsiPakanDone) return;

        if (isIsiPakanDone && !isIsiPakanClaimed)
        {
            isIsiPakanClaimed = true;

            PlayRewardEffects(rewardIsiPakan, btnAmbilIsiPakan.transform);

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardIsiPakan);
            if (barIsiPakan != null) barIsiPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    private void PlayRewardEffects(int rewardAmount, Transform buttonTransform)
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.Invoke("SpawnUICoin", 0f); 
            
            if (InventoryManager.instance.uiCoinPrefab != null && InventoryManager.instance.navCoinTarget != null)
            {
                int jumlahKoin = 5;
                for (int i = 0; i < jumlahKoin; i++)
                {
                    GameObject coin = Instantiate(InventoryManager.instance.uiCoinPrefab, misiPanel.transform.parent);
                    coin.transform.SetAsLastSibling();
                    coin.transform.position = buttonTransform.position; 

                    UICoinEffect effect = coin.GetComponent<UICoinEffect>();
                    if (effect == null) effect = coin.AddComponent<UICoinEffect>();
                    
                    int nilaiPerKoin = (i == 0) ? rewardAmount : 0;
                    effect.Init(InventoryManager.instance.navCoinTarget, nilaiPerKoin);
                }
            }

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

    // 🔥 FUNGSI BARU: Mengupdate hitungan nambang (0/15) secara real-time
    // 🔥 FUNGSI BARU: Mengupdate hitungan nambang (0/15) secara real-time
    public void UpdateTambangLogamProgress(int totalCount) {
        if (isTambangLogamClaimed) return;

        // KUNCI: Data harus selalu diupdate dan disimpan di background terlebih dahulu!
        totalLogamMinedCount = totalCount;
        PlayerPrefs.SetInt("Saved_TotalLogamMinedCount", totalLogamMinedCount);
        PlayerPrefs.Save();

        // Logika evaluasi status misi selesai (pindahkan ke luar pengecekan bar UI)
        if (totalLogamMinedCount >= targetTambangLogam) {
            isTambangLogamDone = true;
            if (imgBtnTambangLogam != null) imgBtnTambangLogam.sprite = btnHijauAmbil;
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
        }

        // 🔥 Hanya urusan visual teks dan slider yang dimasukkan ke dalam gerbang activeSelf
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

    // 🔥 FUNGSI BARU: Dipasang ke Button 'btnAmbilTambangLogam' di Inspector
    public void AmbilHadiahTambangLogam() {
        if (isTambangLogamDone && !isTambangLogamClaimed) {
            isTambangLogamClaimed = true;

            PlayRewardEffects(rewardTambangLogam, btnAmbilTambangLogam.transform);

            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardTambangLogam);
            }

            btnAmbilTambangLogam.gameObject.SetActive(false);
            if (txtTambangLogam != null) txtTambangLogam.text = "Misi Selesai!";
            
            if (barTambangLogam != null) barTambangLogam.SetActive(false);
            SimpanProgressGameKeKomputer();
        }
    }
}