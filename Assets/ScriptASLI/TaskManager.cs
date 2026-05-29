using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Misi 3: Surat Edaran Kades (Babak 2)")]
    public GameObject barEdaranKades;       // Bar Misi Baru (Tanpa Slider)
    public Button btnBukaEdaranKades;       // Tombol yang tulisannya "Buka"
    public GameObject panelEdaranKades;
    
    // 🔥 TAMBAHAN BARU UNTUK TYPEWRITER EFFECT
    public TextMeshProUGUI txtIsiEdaranKades; // Tarik komponen Text TMP isi surat ke sini
    public Button btnCloseEdaranKades;       // Tarik tombol Silang (X) atau Close ke sini
    [TextArea(3, 10)]
    public string teksLengkapEdaran;         // Tulis isi surat edaran kades kamu di Inspector
    public float kecepatanKetik = 0.05f;     // Jeda waktu per huruf (makin kecil makin cepat)

    private Coroutine typewriterCoroutine;

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

        if (panelEdaranKades != null) panelEdaranKades.SetActive(false);
        if (barEdaranKades != null) barEdaranKades.SetActive(false);

        if (ikonNotifikasi != null) ikonNotifikasi.SetActive(true);

        UpdateMisi1UI();
    }

    // --- FUNGSI TOMBOL IKON MISI ---
    public void OpenMisi() {
        if (misiPanel != null) {
            if (UIManager.instance != null)
            {
                UIManager.instance.OpenPanelMenu(misiPanel);
            }
            else
            {
                misiPanel.SetActive(true);
            }

            if (asetBlur != null) asetBlur.SetActive(true);
            // 2. SAAT PANEL DIBUKA: Matikan tanda seru (dianggap sudah terbaca)
            if (ikonNotifikasi != null) ikonNotifikasi.SetActive(false);
            
            // Refresh tampilan slider jika panel dibuka
            if (InventoryManager.instance != null) {
                UpdateTebangProgress(InventoryManager.instance.totalWoodCollected);
            }
        }
    }

    public void CloseMisi() {
        if (UIManager.instance != null)
        {
            UIManager.instance.ClosePanelMenu(misiPanel);
        }
        else
        {
            if (misiPanel != null) misiPanel.SetActive(false);
        }

        if (asetBlur != null) asetBlur.SetActive(false);
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
        // 🔥 MODIFIKASI: Panggilan otomatis di sini DIHAPUS agar misi kades 
        // tidak mencuri start sebelum kuis level 1 diselesaikan & reward diclaim.
    }

    public void AktifkanMisiEdaranKades() {
        if (barEdaranKades != null) {
            barEdaranKades.SetActive(true);

            // 🔥 MODIFIKASI: Memaksa Bar Misi Edaran Kades naik ke urutan paling atas di dalam taskBar Layout Group
            barEdaranKades.transform.SetAsFirstSibling(); 
            
            // Nyalakan notifikasi tanda seru karena ada misi baru masuk di Level 2
            if (!misiPanel.activeSelf && ikonNotifikasi != null) {
                ikonNotifikasi.SetActive(true);
            }
        }
    }

    // Fungsi yang dipasang pada onClick Tombol "Buka" di Bar Edaran Kades
    // Fungsi yang dipasang pada onClick Tombol "Buka" di Bar Edaran Kades
    public void BukaSuratEdaranKades() {
        if (panelEdaranKades != null) {
            // Tutup dulu panel misi utama agar tidak menumpuk
            if (misiPanel != null) misiPanel.SetActive(false);

            panelEdaranKades.SetActive(true);
            if (asetBlur != null) asetBlur.SetActive(true);

            // TEPAT SAAT BUKA: Sembunyikan dulu tombol close/silang agar pemain tidak bisa skip di awal
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(false);
            }

            // Jalankan efek mengetik secara aman
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }
    }

    // 🌟 COROUTINE LOGIKA EFEK MENGETIK 🌟
    IEnumerator TypewriterRoutine() {
        if (txtIsiEdaranKades != null) {
            txtIsiEdaranKades.text = ""; // Kosongkan teks di awal

            // Ketik huruf satu per satu berdasarkan string teksLengkapEdaran
            foreach (char huruf in teksLengkapEdaran.ToCharArray()) {
                txtIsiEdaranKades.text += huruf;
                yield return new WaitForSeconds(kecepatanKetik); // Jeda per karakter
            }

            // 🔥 SETELAH SELESAI MENGETIK: Munculkan tombol close secara otomatis!
            if (btnCloseEdaranKades != null) {
                btnCloseEdaranKades.gameObject.SetActive(true);
                Debug.Log("<color=green>[Typewriter]</color> Teks selesai diketik, tombol close muncul.");
            }
        }
    }

    // Fungsi yang dipasang pada Tombol Silang (X) di Panel Edaran Kades
    // Fungsi yang dipasang pada Tombol Silang (X) di Panel Edaran Kades
    // Fungsi yang dipasang pada Tombol Silang (X) di Panel Edaran Kades
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

            // ====================================================================
            // 🔥 LOGIKA SINKRONISASI BARU SETELAH EDARAN DITUTUP 🔥
            // ====================================================================
            
            // 1. Munculkan semua objek koin di Dunia 3D lewat Level2Manager
            if (Level2Manager.instance != null && Level2Manager.instance.koinLevel2Container != null) {
                Level2Manager.instance.koinLevel2Container.SetActive(true);
                Debug.Log("<color=yellow>[Task Manager]</color> Surat Edaran dibaca, Koin-Koin di Map dimunculkan!");
            }

            // 2. Berikan Reward Tambahan Emas 5 Gram ke Sistem internal MoneyManager
            if (MoneyManager.instance != null) {
                MoneyManager.instance.totalEmas += 5; // Tambah reward 5 gram emas
                MoneyManager.instance.UpdateEmasPerakUI(); // Refresh internal UI data
            }

            // 3. Paksa Update visual Teks Emas Utama di layar monitor biar langsung berubah dari 0 gr jadi 5 gr
            if (Level2Manager.instance != null && Level2Manager.instance.txtEmasUtama != null) {
                Level2Manager.instance.txtEmasUtama.text = MoneyManager.instance.totalEmas + " gr";
            }

            // 4. Update data Nisab Jurnal (karena saldo emas baru saja berubah bertambah)
            if (JurnalManager.instance != null) {
                JurnalManager.instance.CheckEmasPerakNisab();
            }
        }
    }
}