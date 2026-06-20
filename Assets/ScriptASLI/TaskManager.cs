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
        isMisi2Started = false; 

        if (panelEdaranKades != null) panelEdaranKades.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);
        if (asetBlurEdaran != null) asetBlurEdaran.SetActive(false);
        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);

        UpdateMisi1UI();
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
        if (isMisi2Claimed) return; 

        if (barTebangPohon != null && barTebangPohon.activeSelf && isMisi2Started) {
            int progressMisiSekarang = totalCount - woodOffset; 
            if (progressMisiSekarang < 0) progressMisiSekarang = 0;

            sliderTebang.maxValue = targetTebang;
            sliderTebang.value = progressMisiSekarang;
            txtTebang.text = "Tebang Pohon (" + progressMisiSekarang.ToString() + "/" + targetTebang.ToString() + ")";

            // 🔥 TAMBAHAN BARU: Cek jika progress tebang sudah menyentuh atau melewati 15 kayu
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