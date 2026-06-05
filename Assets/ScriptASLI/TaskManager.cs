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
            
            bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;
            if (!sudahLevel3 && InventoryManager.instance != null) {
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
        if (barTebangPohon != null && barTebangPohon.activeSelf && isMisi2Started && !isMisi2Claimed) {
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
            txtIsiEdaranKades.text = ""; 
            foreach (char huruf in teksLengkapEdaran.ToCharArray()) {
                txtIsiEdaranKades.text += huruf;
                yield return new WaitForSeconds(kecepatanKetik); 
            }
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

        if (isKeTokoDone && !isKeTokoClaimed)
        {
            isKeTokoClaimed = true;
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardKeToko);
            if (barKeToko != null) barKeToko.SetActive(false);

            if (barBeliPakan != null) barBeliPakan.SetActive(true);
            if (barNisabHewan != null) barNisabHewan.SetActive(true);
            if (barIsiPakan != null) barIsiPakan.SetActive(true);

            if (txtBeliPakan != null) txtBeliPakan.text = "Beli paket pakan rumput di toko";
            if (txtIsiPakan != null) txtIsiPakan.text = "Isi ulang tempat makanan 3D di peternakan";
            
            hewanDibeliCount = 0; 
            if (sliderNisabHewan != null) sliderNisabHewan.value = 0f;
            if (txtNisabHewan != null) txtNisabHewan.text = $"Beli hewan ternak hingga nisab (0/{targetHewanNisab})";

            isiPakanCount = 0; 

            // Munculkan semua tombol 3 misi serentak dengan default warna Abu-Abu
            if (btnAmbilBeliPakan != null) btnAmbilBeliPakan.gameObject.SetActive(true);
            if (imgBtnBeliPakan != null) imgBtnBeliPakan.sprite = btnAbuAbu;

            if (btnAmbilNisabHewan != null) btnAmbilNisabHewan.gameObject.SetActive(true);
            if (imgBtnNisabHewan != null) imgBtnNisabHewan.sprite = btnAbuAbu;

            if (btnAmbilIsiPakan != null) btnAmbilIsiPakan.gameObject.SetActive(true);
            if (imgBtnIsiPakan != null) imgBtnIsiPakan.sprite = btnAbuAbu;
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
        if (!isBeliPakanDone) return; 

        if (isBeliPakanDone && !isBeliPakanClaimed)
        {
            isBeliPakanClaimed = true;
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardBeliPakan);
            if (barBeliPakan != null) barBeliPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    public void KlaimRewardNisabHewan()
    {
        if (!isNisabHewanDone) return; 

        if (isNisabHewanDone && !isNisabHewanClaimed)
        {
            isNisabHewanClaimed = true;
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardNisabHewan);
            if (barNisabHewan != null) barNisabHewan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
        }
    }

    public void KlaimRewardIsiPakan()
    {
        if (!isIsiPakanDone) return; 

        if (isIsiPakanDone && !isIsiPakanClaimed)
        {
            isIsiPakanClaimed = true;
            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardIsiPakan);
            if (barIsiPakan != null) barIsiPakan.SetActive(false); 
            CekSemuaMisiBabak3Selesai();
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