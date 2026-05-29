using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JurnalManager : MonoBehaviour
{
    public static JurnalManager instance;

    [Header("UI Panels & General Buttons")]
    public GameObject jurnalContent;
    public GameObject asetBlur;
    public GameObject ikonNotifikasiJurnal;
    public Button btnClose;
    public Button btnNext;
    public Button btnPrevious;

    [Header("Halaman 1: Zakat Perdagangan")]
    public GameObject groupZakatPerdagangan; 
    public GameObject checkNisab;
    public GameObject checkHaul;
    public GameObject checkZakatDagang;
    public TextMeshProUGUI txtHartamu;
    public TextMeshProUGUI txtStatus;
    public GameObject messageText;
    public Slider haulSlider;

    [Header("Halaman 2: Zakat Emas & Perak")]
    public GameObject groupZakatEmasPerak; // Ini panel halaman 2 kamu
    public GameObject visualHalamanLock;         // Gambar gembok halaman 2
    public GameObject visualHalamanUnlock;       // Konten asli emas/perak
    public GameObject navCoinLeftPanel;

    // Komponen Checklist & UI Baru untuk Emas/Perak (Disamakan dengan Dagang)
    public GameObject checkNisabEmasPerak;
    public GameObject checkHaulEmasPerak;
    public GameObject checkZakatEmasPerak;
    public TextMeshProUGUI txtHartaEmas;         // Text untuk Gram Emas saat ini
    public TextMeshProUGUI txtHartaPerak;        // Text untuk Gram Perak saat ini
    public TextMeshProUGUI txtEmasPerakStatus;   // Teks status wajib zakat emas/perak
    public GameObject messageTextEmasPerak;      // Pesan deskripsi/pemberitahuan tambahan
    public Slider haulSliderEmasPerak;

    [Header("Settings")]
    public float nisabLimit = 5000f; 
    public float timerPerMonth = 5f; // 5 detik per bulan sesuai keinginanmu
    public ZakatPanelManager zakatManager;
    
    // --- MODIFIKASI BARU: Batas Kriteria Nisab Emas & Perak Sesuai Permintaan (94g & 624g) ---
    public float nisabEmasKriteria = 94f;        
    public float nisabPerakKriteria = 624f;      

    private int currentHaulMonth = 0;
    private bool isNisabReached = false;
    private bool isHaulComplete = false;
    private bool isNotificationShown = false;
    private bool isDagangCoroutineRunning = false;

    // --- MODIFIKASI BARU: State Kunci, Haul, & Notifikasi Babak 2 ---
    private int currentHaulMonthEmasPerak = 0;
    private bool isEmasPerakNisabReached = false;
    private bool isEmasPerakHaulComplete = false;
    private bool isEmasPerakUnlocked = false;
    private bool isEmasPerakNotificationShown = false;
    private bool isEmasPerakCoroutineRunning = false;
    void Awake() { instance = this; }

    void Start()
    {
        jurnalContent.SetActive(false);
        asetBlur.SetActive(false);
        if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
        
        // --- MODIFIKASI BARU: Di awal game, set visual halaman 2 ke kondisi Terkunci ---
        if (visualHalamanLock != null) visualHalamanLock.SetActive(true);
        if (visualHalamanUnlock != null) visualHalamanUnlock.SetActive(false);
        
        ShowPage(1);
        
        checkNisab.SetActive(false);
        checkHaul.SetActive(false);
        if(checkZakatDagang != null) checkZakatDagang.SetActive(false);
        
        haulSlider.minValue = 0;
        haulSlider.maxValue = 12;
        haulSlider.value = 0;
        messageText.SetActive(false);
        
        // --- MODIFIKASI BARU: Reset Visual Page 2 ---
        if (checkNisabEmasPerak != null) checkNisabEmasPerak.SetActive(false);
        if (checkHaulEmasPerak != null) checkHaulEmasPerak.SetActive(false);
        if (checkZakatEmasPerak != null) checkZakatEmasPerak.SetActive(false);
        if (messageTextEmasPerak != null) messageTextEmasPerak.SetActive(false);
        
        if (haulSliderEmasPerak != null)
        {
            haulSliderEmasPerak.minValue = 0;
            haulSliderEmasPerak.maxValue = 12;
            haulSliderEmasPerak.value = 0;
        }
        
        UpdateStatusUI();
        UpdateEmasPerakStatusUI();

        btnClose.onClick.AddListener(CloseJurnal);
        btnNext.onClick.AddListener(() => ShowPage(2));
        btnPrevious.onClick.AddListener(() => ShowPage(1));
    }

    void Update()
    {
        if (MoneyManager.instance != null)
        {
            // --- LOGIKA HALAMAN 1 (PERDAGANGAN) ---
            float currentMoney = MoneyManager.instance.totalMoney;
            txtHartamu.text = "Rp " + currentMoney.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            if (currentMoney >= nisabLimit && !isNisabReached)
            {
                StartZakatLogic();
            }

            // --- MODIFIKASI BARU: LOGIKA HALAMAN 2 (EMAS & PERAK) ---
            // Update teks gramasi emas dan perak secara realtime dari MoneyManager
            if (txtHartaEmas != null) txtHartaEmas.text = MoneyManager.instance.totalEmas.ToString() + " gram";
            if (txtHartaPerak != null) txtHartaPerak.text = MoneyManager.instance.totalPerak.ToString() + " gram";

            // Jalankan pengecekan kriteria pemenuhan nisab emas atau perak
            CheckEmasPerakNisab();
        }
    }

    public void StartZakatLogic() {
        if (isNisabReached) return;

        isNisabReached = true;

        if (checkNisab != null) checkNisab.SetActive(true);
        if (checkZakatDagang != null) checkZakatDagang.SetActive(true);
        if (ikonNotifikasiJurnal != null && !jurnalContent.activeSelf) 
        {
            ikonNotifikasiJurnal.SetActive(true);
        }
        
        // 🔥 PERBAIKAN: Jangan pakai StopAllCoroutines() lagi agar tidak membunuh slider lain
        if (!isDagangCoroutineRunning)
        {
            StartCoroutine(HaulTimerRoutine());
        }
    }

    IEnumerator HaulTimerRoutine() {
        isDagangCoroutineRunning = true; // 🔥 Kunci coroutine aktif
        currentHaulMonth = 0;
        isHaulComplete = false;

        while (currentHaulMonth < 12) {
            float timer = 0;
            while (timer < timerPerMonth) { 
                timer += Time.deltaTime;
                if (haulSlider != null) {
                    haulSlider.value = currentHaulMonth + (timer / timerPerMonth);
                }
                yield return null;
            }
            currentHaulMonth++;
            haulSlider.value = currentHaulMonth; 
        }

        isHaulComplete = true;
        if (checkHaul != null) checkHaul.SetActive(true);
        UpdateStatusUI(); 
        
        isDagangCoroutineRunning = false; // ✅ Buka kunci saat selesai
    }

    // --- MODIFIKASI BARU: Coroutine Timer Haul khusus untuk Emas & Perak ---
    IEnumerator HaulTimerEmasPerakRoutine() {
        isEmasPerakCoroutineRunning = true; // 🔥 Kunci Coroutine menyala
        currentHaulMonthEmasPerak = 0;
        isEmasPerakHaulComplete = false;

        while (currentHaulMonthEmasPerak < 12) {
            float timer = 0;
            while (timer < timerPerMonth) { 
                timer += Time.deltaTime;
                if (haulSliderEmasPerak != null) {
                    haulSliderEmasPerak.value = currentHaulMonthEmasPerak + (timer / timerPerMonth);
                }
                yield return null;
            }
            currentHaulMonthEmasPerak++;
            haulSliderEmasPerak.value = currentHaulMonthEmasPerak; 
        }

        isEmasPerakHaulComplete = true;
        if (checkHaulEmasPerak != null) checkHaulEmasPerak.SetActive(true);
        UpdateEmasPerakStatusUI(); 
        
        isEmasPerakCoroutineRunning = false; // ✅ Lepas kunci saat selesai
    }

    void ShowPage(int pageNumber)
    {
        if (pageNumber == 1)
        {
            groupZakatPerdagangan.SetActive(true);
            groupZakatEmasPerak.SetActive(false);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(false);
        }
        else
        {
            groupZakatPerdagangan.SetActive(false);
            groupZakatEmasPerak.SetActive(true);
            btnNext.gameObject.SetActive(false);
            btnPrevious.gameObject.SetActive(true);
        }
    }

    void UpdateStatusUI()
    {
        if (isNisabReached && isHaulComplete)
        {
            txtStatus.text = "Wajib Zakat";
            txtStatus.color = Color.black;
            messageText.SetActive(true);
            
            if (zakatManager != null)
            {
                // AKTIFKAN KUNCI
                zakatManager.isPerdaganganUnlocked = true;
                zakatManager.UpdateItemVisuals();
            }
            if (!isNotificationShown && !jurnalContent.activeSelf) {
                if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(true);
                isNotificationShown = true; 
            }
        }
        else
        {
            txtStatus.text = "Belum Wajib Zakat";
            txtStatus.color = Color.gray; 
            messageText.SetActive(false);
            
            if (zakatManager != null)
            {
                // --- TAMBAHKAN INI UNTUK MENGUNCI KEMBALI JIKA BELUM WAJIB ZAKAT ---
                zakatManager.isPerdaganganUnlocked = false;
                zakatManager.UpdateItemVisuals();
            }
        }
    }

    void UpdateEmasPerakStatusUI()
    {
        if (isEmasPerakNisabReached && isEmasPerakHaulComplete)
        {
            // Deteksi teks dinamis berdasarkan logam mana yang tembus target kriteria
            int emasSekarang = MoneyManager.instance != null ? MoneyManager.instance.totalEmas : 0;
            int perakSekarang = MoneyManager.instance != null ? MoneyManager.instance.totalPerak : 0;

            if (emasSekarang >= nisabEmasKriteria && perakSekarang >= nisabPerakKriteria)
                txtEmasPerakStatus.text = "Wajib Zakat Emas/Perak";
            else if (emasSekarang >= nisabEmasKriteria)
                txtEmasPerakStatus.text = "Wajib Zakat Emas";
            else
                txtEmasPerakStatus.text = "Wajib Zakat Perak";

            txtEmasPerakStatus.color = Color.black;
            if (messageTextEmasPerak != null) messageTextEmasPerak.SetActive(true);
            
            if (zakatManager != null)
            {
                zakatManager.isEmasPerakUnlocked = true; // Buka akses tombol di carousel manager
                zakatManager.UpdateItemVisuals();
            }
            if (!isEmasPerakNotificationShown && !jurnalContent.activeSelf) {
                if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(true);
                isEmasPerakNotificationShown = true; 
            }
        }
        else
        {
            txtEmasPerakStatus.text = "Belum Wajib Zakat";
            txtEmasPerakStatus.color = Color.gray; 
            if (messageTextEmasPerak != null) messageTextEmasPerak.SetActive(false);
            
            if (zakatManager != null)
            {
                zakatManager.isEmasPerakUnlocked = false;
                zakatManager.UpdateItemVisuals();
            }
        }
    }

    // --- MODIFIKASI BARU: Logika pengecekan akumulasi Nisab Salah Satu Emas / Perak ---
    public void CheckEmasPerakNisab() 
    {
        if (MoneyManager.instance == null) return;

        int emasSekarang = MoneyManager.instance.totalEmas;
        int perakSekarang = MoneyManager.instance.totalPerak;

        if (emasSekarang >= nisabEmasKriteria || perakSekarang >= nisabPerakKriteria) 
        {
            if (!isEmasPerakUnlocked) 
            {
                isEmasPerakUnlocked = true;
                
                if (visualHalamanLock != null) visualHalamanLock.SetActive(false);
                if (visualHalamanUnlock != null) visualHalamanUnlock.SetActive(true);
                if (navCoinLeftPanel != null) navCoinLeftPanel.SetActive(true);

                if (zakatManager != null) 
                {
                    zakatManager.isEmasPerakUnlocked = true; 
                    zakatManager.UpdateItemVisuals();
                }
            }

            // Memicu siklus logika checklist & slider haul halaman 2
            if (!isEmasPerakNisabReached)
            {
                isEmasPerakNisabReached = true;
                if (checkNisabEmasPerak != null) checkNisabEmasPerak.SetActive(true);
                if (checkZakatEmasPerak != null) checkZakatEmasPerak.SetActive(true);
                
                if (ikonNotifikasiJurnal != null && !jurnalContent.activeSelf) 
                {
                    ikonNotifikasiJurnal.SetActive(true);
                }

                // 🔥 PERBAIKAN CRITICAL: Hanya jalankan jika coroutine BELUM berjalan!
                if (!isEmasPerakCoroutineRunning)
                {
                    StartCoroutine(HaulTimerEmasPerakRoutine());
                }
            }
        } 
    }

    public void OpenJurnal() 
    { 
        if (UIManager.instance != null)
        {
            UIManager.instance.OpenPanelMenu(jurnalContent);
        }
        else
        {
            jurnalContent.SetActive(true); 
        }

        if (asetBlur != null) asetBlur.SetActive(true);

        // SAAT JURNAL DIBUKA: Matikan ikon notifikasi (tanda sudah dibaca)
        if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
    }

    public void CloseJurnal()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.ClosePanelMenu(jurnalContent);
        }
        else
        {
            jurnalContent.SetActive(false);
        }

        if (asetBlur != null) asetBlur.SetActive(false);
    }
    
    public bool IsPerdaganganUnlocked()
    {
        return isNisabReached && isHaulComplete;
    }

    public bool IsEmasPerakUnlocked()
    {
        // Kembalikan status final apakah haul & kuis emas/perak sudah terbuka sepenuhnya
        return isEmasPerakNisabReached && isEmasPerakHaulComplete; 
    }

    public bool IsPeternakanUnlocked()
    {
        return false; // nanti kamu isi sendiri
    }
    // Tambahkan ini di dalam class ZakatPanelManager : MonoBehaviour
}