using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour {
    public static TaskManager instance;

    [Header("UI Panels & Notification")]
    public GameObject misiPanel;
    public GameObject asetBlur; 
    public GameObject ikonNotifikasi; // Tarik objek Tanda Seru (di luar panel) ke sini

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

    [Header("Global UI Settings")]
    public Sprite btnAbuAbu; 
    public Sprite btnHijauAmbil; 

    void Awake() { instance = this; }

    void Start() {
        if (misiPanel != null) misiPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
        
        // Misi 2 harus mati total di awal
        if (barTebangPohon != null) barTebangPohon.SetActive(false); 
        isMisi2Started = false; 

        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);

        UpdateMisi1UI();
    }

    // --- FUNGSI TOMBOL IKON MISI ---
    public void OpenMisi() {
        if (misiPanel != null) {
            misiPanel.SetActive(true);
            asetBlur.SetActive(true);

            // 2. SAAT PANEL DIBUKA: Matikan tanda seru (dianggap sudah terbaca)
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
            
            // Refresh tampilan slider jika panel dibuka
            if (InventoryManager.instance != null) {
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected);
            }
        }
    }

    public void CloseMisi() {
        misiPanel.SetActive(false);
        asetBlur.SetActive(false);
    }

    // --- LOGIKA MISI 1 ---
    public void NotifyWoodSold() {
        if (isMisi1Claimed) return;
        isJualDone = true;

        // 3. JIKA MISI SELESAI & PANEL LAGI TUTUP: Munculkan tanda seru lagi
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
                
                // 4. ADA BAR BARU: Munculkan tanda seru jika panel sedang tutup
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

            // TEPAT SAAT INI: Kunci jumlah kayu sebagai titik nol
            if (InventoryManager.instance != null) {
                woodOffset = InventoryManager.instance.totalWoodCollected;
            }

            // SEKARANG: Baru bolehkan misi 2 menghitung
            isMisi2Started = true; 

            if (barTebangPohon != null) {
                barTebangPohon.SetActive(true);
                barTebangPohon.transform.SetAsFirstSibling(); 
                
                // Paksa visual ke 0/30 (Total - Offset = 0)
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected); 
            }

            if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(rewardMisi1);
            btnAmbilTebangJual.gameObject.SetActive(false);
            txtTebangJual.text = "Misi Selesai!";
        }
    }

    public void UpdateTebangProgress(int totalCount) {
        // SYARAT KETAT: Bar harus aktif DAN kunci isMisi2Started harus TRUE
        if (barTebangPohon != null && barTebangPohon.activeSelf && isMisi2Started && !isMisi2Claimed) {
            
            // Logika: Total kayu saat ini dikurangi kayu yang sudah ada di tas saat misi dimulai
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

    // Di dalam TaskManager.cs
    public void AmbilHadiahTebangPohon() {
        if (isTebangDone && !isMisi2Claimed) {
            isMisi2Claimed = true;

            if (MoneyManager.instance != null) {
                MoneyManager.instance.AddMoney(rewardMisi2);
            }

            // --- BARIS JURNAL DI SINI SUDAH DIHAPUS ---

            btnAmbilTebangPohon.gameObject.SetActive(false);
            txtTebang.text = "Misi Selesai!";
        }
    }
}