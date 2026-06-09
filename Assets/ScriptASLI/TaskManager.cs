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
    
    public TextMeshProUGUI txtIsiEdaranKades; 
    public Button btnCloseEdaranKades;       
    [TextArea(3, 10)]
    public string teksLengkapEdaran;         
    public float kecepatanKetik = 0.05f;     

    private Coroutine typewriterCoroutine;
    private bool edaranSedangMengetik = false;

    [Header("Babak 3: Misi Peternakan")]
    public GameObject barKeToko; 
    public Button btnAmbilKeToko;
    public Image imgBtnKeToko; // 🔥 TAMBAHAN: Komponen Image tombol Toko
    public TextMeshProUGUI txtKeToko;
    public int rewardKeToko = 15000;
    private bool isKeTokoDone = false;
    private bool isKeTokoClaimed = false;

    [Header("Babak 3: 3 Misi Serentak (Muncul setelah Misi 1 Claimed)")]
    [Header("Misi Beli Pakan")]
    public GameObject barBeliPakan; 
    public Button btnAmbilBeliPakan;
    public Image imgBtnBeliPakan; // 🔥 TAMBAHAN: Komponen Image tombol Beli Pakan
    public TextMeshProUGUI txtBeliPakan;
    public int rewardBeliPakan = 5000;
    private bool isBeliPakanDone = false;
    private bool isBeliPakanClaimed = false;

    [Header("Misi Nisab Hewan")]
    public GameObject barNisabHewan; 
    public Button btnAmbilNisabHewan; 
    public Image imgBtnNisabHewan; // 🔥 TAMBAHAN: Komponen Image tombol Nisab Hewan
    public Slider sliderNisabHewan;
    public TextMeshProUGUI txtNisabHewan;
    public int rewardNisabHewan = 25000; 
    private int hewanDibeliCount = 0;
    private int targetHewanNisab = 3;
    private bool isNisabHewanDone = false;
    private bool isNisabHewanClaimed = false; 

    [Header("Misi Isi Pakan")]
    public GameObject barIsiPakan; 
    public Button btnAmbilIsiPakan; 
    public Image imgBtnIsiPakan; // 🔥 TAMBAHAN: Komponen Image tombol Isi Pakan
    public TextMeshProUGUI txtIsiPakan;
    public int rewardIsiPakan = 10000; 
    private int isiPakanCount = 0;
    private int targetIsiPakan = 6;
    private bool isIsiPakanDone = false;
    private bool isIsiPakanClaimed = false; 

    [Header("Global UI Settings")]
    public Sprite btnAbuAbu; 
    public Sprite btnHijauAmbil; 

    void Awake() { instance = this; }

    void Start() {
        if (misiPanel != null) misiPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
        
        if (barTebangPohon != null) barTebangPohon.SetActive(false); 
        isMisi2Started = false; 

        if (panelEdaranKades != null) panelEdaranKades.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);

        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);

        UpdateMisi1UI();
    }

    public void OpenMisi() {
        if (misiPanel != null) {
            if (UIManager.instance != null) {
                UIManager.instance.OpenPanelMenu(misiPanel);
            } else {
                misiPanel.SetActive(true);
            }

            if (asetBlur != null) asetBlur.SetActive(true);
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
            
            // Pengecekan status level 3
            bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;
            
            // Jika TIDAK level 3 DAN misi 2 BELUM diclaim, baru update progress tebang
            if (!sudahLevel3 && !isMisi2Claimed && InventoryManager.instance != null) {
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected);
            }
        }
    }

    public void CloseMisi() {
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
    // Tambahkan pengaman ini: Jika sudah level 3, jangan utak-atik UI level 1 lagi!
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
    // Tambahkan pengaman ini: Jika misi 2 sudah selesai diklaim, kunci rapat-rapat!
    if (isMisi2Claimed) return; 

    if (barTebangPohon != null && barTebangPohon.activeSelf && isMisi2Started) {
        int progressMisiSekarang = totalCount - woodOffset; 
        if (progressMisiSekarang < 0) progressMisiSekarang = 0;

        sliderTebang.maxValue = targetTebang;
        sliderTebang.value = progressMisiSekarang;
        txtTebang.text = "Tebang Pohon (" + progressMisiSekarang.ToString() + "/" + targetTebang.ToString() + ")";

        if (progressMisiSekarang >= targetTebang) {
            isTebangDone = true;
            imgBtnTebangPohon.sprite = btnHijauAmbil;
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
        }
    }
}

    public void AmbilHadiahTebangPohon() {
        if (isTebangDone && !isMisi2Claimed) {
            isMisi2Claimed = true;
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
        if (panelEdaranKades != null) {
            if (misiPanel != null) misiPanel.SetActive(false);

            panelEdaranKades.SetActive(true);
            if (asetBlur != null) asetBlur.SetActive(true);

            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(false);
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }
    }

    IEnumerator TypewriterRoutine() {
        if (txtIsiEdaranKades != null) {
            edaranSedangMengetik = true; // 🔥 Set true saat mulai
            txtIsiEdaranKades.text = ""; 
            foreach (char huruf in teksLengkapEdaran.ToCharArray()) {
                txtIsiEdaranKades.text += huruf;
                yield return new WaitForSeconds(kecepatanKetik); 
            }
            edaranSedangMengetik = false; // 🔥 Set false saat selesai natural
            
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(true);
            }
        }
    }

    // 🔥 FUNGSI BARU: Dipanggil saat panelEdaranKades di-tap/klik
    public void SkipKetikEdaran()
    {
        if (edaranSedangMengetik)
        {
            // 1. Hentikan efek mengetik
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

            // 2. Langsung isi teks secara utuh seketika
            if (txtIsiEdaranKades != null)
            {
                txtIsiEdaranKades.text = teksLengkapEdaran;
            }

            edaranSedangMengetik = false;

            // 3. Munculkan tombol close edaran desa
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
            
            if (asetBlur != null && !misiPanel.activeSelf) {
                asetBlur.SetActive(false);
            }

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

            if (JurnalManager.instance != null) {
                JurnalManager.instance.CheckEmasPerakNisab();
            }
        }
    }

    // ====================================================================
    // 🔥 PERBAIKAN TOTAL SYSTEM LEVEL 3 DENGAN GLOBAL UI SETTINGS AUTOMATIC
    // ====================================================================
    public void MulaiMisiBabak3()
    {
        if (barTebangJual != null) barTebangJual.SetActive(false);
        if (barTebangPohon != null) barTebangPohon.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);

        if (barKeToko != null) {
            barKeToko.SetActive(true);
            barKeToko.transform.SetAsFirstSibling(); 
        }
        
        // Sinyalkan tombol Ambil aktif, set gambarnya menjadi Abu-Abu bawaan sistem global
        if (btnAmbilKeToko != null) btnAmbilKeToko.gameObject.SetActive(true); 
        if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnAbuAbu; 
        if (txtKeToko != null) txtKeToko.text = "Pergi ke toko & beli hewan ternak";

        if (barBeliPakan != null) barBeliPakan.SetActive(false);
        if (barNisabHewan != null) barNisabHewan.SetActive(false);
        if (barIsiPakan != null) barIsiPakan.SetActive(false);

        if (ikonNotifikasi != null && !misiPanel.activeSelf) {
            ikonNotifikasi.SetActive(true);
        }
    }

    public void KlaimRewardKeToko()
    {
        if (!isKeTokoDone) return; // PENGAMAN: Mencegah klik curang sebelum selesai

        if (isKeTokoDone && !isKeTokoClaimed) //
        {
            isKeTokoClaimed = true; //
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardKeToko); //
            if (barKeToko != null) barKeToko.SetActive(false); //[cite: 3]

            // 🔥 SEKUENS BARU: Munculkan 2 misi pakan terlebih dahulu
            if (barBeliPakan != null) barBeliPakan.SetActive(true); //[cite: 3]
            if (barIsiPakan != null) barIsiPakan.SetActive(true); //[cite: 3]
            
            // 🔥 Kunci/Sembunyikan misi nisab hewan untuk sementara waktu
            if (barNisabHewan != null) barNisabHewan.SetActive(false); //[cite: 3]

            if (txtBeliPakan != null) txtBeliPakan.text = "Beli pakan di toko"; //[cite: 3]
            if (txtIsiPakan != null) txtIsiPakan.text = "Isi Pakan Hewan di peternakan"; //[cite: 3]
            
            hewanDibeliCount = 0; //[cite: 3]
            if (sliderNisabHewan != null) sliderNisabHewan.value = 0f; //[cite: 3]
            if (txtNisabHewan != null) txtNisabHewan.text = $"Beli hewan ternak (0/{targetHewanNisab})"; //[cite: 3]

            isiPakanCount = 0; //[cite: 3]

            // Hanya aktifkan tombol untuk Beli Pakan dan Isi Pakan saja
            if (btnAmbilBeliPakan != null) btnAmbilBeliPakan.gameObject.SetActive(true); //[cite: 3]
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnAbuAbu; //[cite: 3]

            if (btnAmbilIsiPakan != null) btnAmbilIsiPakan.gameObject.SetActive(true); //[cite: 3]
            if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnAbuAbu; //[cite: 3]

            // Sembunyikan tombol klaim nisab di awal sekuens ini
            if (btnAmbilNisabHewan != null) btnAmbilNisabHewan.gameObject.SetActive(false);
        }
    }

    public void NotifyBeliPakan()
    {
        if (isKeTokoClaimed && !isBeliPakanDone)
        {
            isBeliPakanDone = true;
            if (txtBeliPakan != null) txtBeliPakan.text = "Selesai membeli paket pakan!";
            
            // Mengubah visual tombol menjadi hijau ambil dari setingan global kamu!
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnHijauAmbil; 
            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }
    }

    public void NotifyHewanDibeli()
    {
        if (!isKeTokoDone)
        {
            isKeTokoDone = true;
            if (txtKeToko != null) txtKeToko.text = "Selesai pergi ke toko & beli hewan ternak!";
            
            // Mengubah visual tombol menjadi hijau ambil dari setingan global kamu!
            if (imgBtnKeToko != null) imgBtnKeToko.sprite = btnHijauAmbil; 
            if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
        }

        if (isKeTokoClaimed && !isNisabHewanDone)
        {
            hewanDibeliCount++;
            if (hewanDibeliCount > targetHewanNisab) hewanDibeliCount = targetHewanNisab;

            if (sliderNisabHewan != null) sliderNisabHewan.value = (float)hewanDibeliCount / targetHewanNisab;
            if (txtNisabHewan != null) txtNisabHewan.text = $"Beli hewan ternak hingga nisab ({hewanDibeliCount}/{targetHewanNisab})";

            if (hewanDibeliCount >= targetHewanNisab)
            {
                isNisabHewanDone = true;
                
                // Mengubah visual tombol menjadi hijau ambil dari setingan global kamu!
                if (imgBtnNisabHewan != null) imgBtnNisabHewan.sprite = btnHijauAmbil; 
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
                
                // Mengubah visual tombol menjadi hijau ambil dari setingan global kamu!
                if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnHijauAmbil; 
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
            }
        }
    }

    public void KlaimRewardBeliPakan()
    {
        if (!isBeliPakanDone) return; //[cite: 3]

        if (isBeliPakanDone && !isBeliPakanClaimed) //[cite: 3]
        {
            isBeliPakanClaimed = true; //[cite: 3]
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardBeliPakan); //[cite: 3]
            if (barBeliPakan != null) barBeliPakan.SetActive(false); //[cite: 3] 
            
            // 🔥 CEK URUTAN: Apakah misi penentu nisab sudah bisa keluar?
            CekUrutanMisiNisabSapi();
            CekSemuaMisiBabak3Selesai(); //[cite: 3]
        }
    }

    public void KlaimRewardIsiPakan()
    {
        if (!isIsiPakanDone) return; //[cite: 3]

        if (isIsiPakanDone && !isIsiPakanClaimed) //[cite: 3]
        {
            isIsiPakanClaimed = true; //[cite: 3]
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardIsiPakan); //[cite: 3]
            if (barIsiPakan != null) barIsiPakan.SetActive(false); //[cite: 3] 
            
            // 🔥 CEK URUTAN: Apakah misi penentu nisab sudah bisa keluar?
            CekUrutanMisiNisabSapi();
            CekSemuaMisiBabak3Selesai(); //[cite: 3]
        }
    }

    public void KlaimRewardNisabHewan()
    {
        if (!isNisabHewanDone) return; //[cite: 3]

        if (isNisabHewanDone && !isNisabHewanClaimed) //[cite: 3]
        {
            isNisabHewanClaimed = true; //[cite: 3]
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardNisabHewan); //[cite: 3]
            if (barNisabHewan != null) barNisabHewan.SetActive(false); //[cite: 3] 
            CekSemuaMisiBabak3Selesai(); //[cite: 3]
        }
    }

    // 🔥 FUNGSI BARU: Mengontrol urutan kemunculan Nisab setelah 2 misi pakan selesai diklaim
    private void CekUrutanMisiNisabSapi()
    {
        if (isBeliPakanClaimed && isIsiPakanClaimed)
        {
            if (barNisabHewan != null && !barNisabHewan.activeSelf)
            {
                barNisabHewan.SetActive(true);
                
                if (btnAmbilNisabHewan != null) btnAmbilNisabHewan.gameObject.SetActive(true);
                if (imgBtnNisabHewan != null) imgBtnNisabHewan.sprite = btnAbuAbu;
                
                // Memicu notifikasi menyala merah di HUD luar agar player tahu ada misi baru
                if (ikonNotifikasi != null && !misiPanel.activeSelf) ikonNotifikasi.SetActive(true);
                
                Debug.Log("<color=yellow>[Task Manager]</color> Misi Pakan Selesai! Misi Nisab Hewan kini diaktifkan.");
            }
        }
    }

    private void CekSemuaMisiBabak3Selesai()
    {
        if (isBeliPakanClaimed && isNisabHewanClaimed && isIsiPakanClaimed)
        {
            Debug.Log("<color=cyan>[Task Manager]</color> Babak 3 SELESAI MUTLAK!");
        }
    }
}