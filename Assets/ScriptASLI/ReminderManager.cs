using System.Collections;
using UnityEngine;
using TMPro; // Pastikan menggunakan TextMeshPro untuk teksnya

public class ReminderManager : MonoBehaviour
{
    public static ReminderManager instance;

    [Header("UI Components")]
    [SerializeField] private RectTransform kakekTransform; // Isi dengan Image (1) kakek
    [SerializeField] private GameObject bubbleObject;       // Isi dengan GameObject bubble
    [SerializeField] private TextMeshProUGUI textMessage;   // Isi dengan komponen teks di dalam bubble

    [Header("Animation Settings")]
    [SerializeField] private float speedKakek = 500f;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float durationVisible = 4f;    // Berapa lama pop-up diam muncul di layar
    [SerializeField] private float loopReminderDelay = 10f; // Muncul lagi setiap 10 detik jika pakan belum diisi

    private Vector2 kakekHiddenPos;
    private Vector2 kakekShownPos;
    private bool isReminderActive = false;
    private Coroutine activeReminderRoutine;
    private string pesanPakanHabis = "Pakan di peternakan sudah habis! Tolong diisi ulang.";

    // 🔥 VARIABEL REVISI TUTORIAL BERANAK TERNAK FIRST TIME
    private bool hasShownTutorialTernak = false;
    private string pesanTutorial1 = "Sapi dan kambing yang kamu beli ini bisa beranak loh.";
    private string pesanTutorial2 = "Maka dari itu rajinlah memberinya makan agar ia tumbuh sehat.";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Tentukan posisi awal (Sembunyi di bawah layar) dan posisi akhir kakek
        kakekShownPos = kakekTransform.anchoredPosition;
        kakekHiddenPos = new Vector2(kakekShownPos.x, -kakekTransform.rect.height - 100f);
        
        // Sembunyikan semuanya di awal game
        kakekTransform.anchoredPosition = kakekHiddenPos;
        bubbleObject.SetActive(false);
        textMessage.text = "";
    }

    // Fungsi utama untuk memicu looping reminder dari script luar
    public void TriggerPakanHabisReminder(bool status)
    {
        if (status)
        {
            if (!isReminderActive)
            {
                isReminderActive = true;
                activeReminderRoutine = StartCoroutine(ReminderLoopSequence());
            }
        }
        else
        {
            // Jika pakan sudah diisi, matikan semua looping reminder dan sembunyikan UI
            isReminderActive = false;
            if (activeReminderRoutine != null) StopCoroutine(activeReminderRoutine);
            StartCoroutine(HideSequence());
        }
    }

    private IEnumerator ReminderLoopSequence()
    {
        while (isReminderActive)
        {
            // Lapisan Pengaman: Tunggu sampai tidak ada panel UI apapun yang terbuka sebelum memicu peringatan pakan
            while (IsAnyPanelOpen()) yield return null;

            // 1. Jalankan sekuens animasi memunculkan reminder
            yield return StartCoroutine(ShowSequence());

            // 2. Tunggu selama durasi tertentu sebelum menghilang otomatis
            yield return new WaitForSeconds(durationVisible);

            // 3. Jalankan sekuens animasi menyembunyikan reminder
            yield return StartCoroutine(HideSequence());

            // 4. Tunggu beberapa detik (delay loop) sebelum memunculkan peringatan lagi
            yield return new WaitForSeconds(loopReminderDelay);
        }
    }

    private IEnumerator ShowSequence()
    {
        textMessage.text = "";
        bubbleObject.SetActive(false);

        // Kakek Transisi Naik dari bawah ke atas
        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekShownPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekShownPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekShownPos;

        // Bubble aktif/muncul
        bubbleObject.SetActive(true);

        // 🔥 ANIMASI TIGA TITIK (...) MUNCUL BERURUTAN
        textMessage.text = "."; yield return new WaitForSeconds(0.4f);
        textMessage.text = ".."; yield return new WaitForSeconds(0.4f);
        textMessage.text = "..."; yield return new WaitForSeconds(0.5f);
        textMessage.text = ""; yield return new WaitForSeconds(0.2f);

        // Efek ketik teks pesan satu persatu
        foreach (char letter in pesanPakanHabis.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private IEnumerator HideSequence()
    {
        // Bubble langsung menghilang / tidak aktif
        bubbleObject.SetActive(false);
        textMessage.text = "";

        // Kakek turun ke bawah layar
        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekHiddenPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekHiddenPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekHiddenPos;
    }

    // ====================================================================
    // 🔥 CORE SEKUENS TUTORIAL BERANAK (MUNCUL SETELAH PANEL TOKO TUTUP)
    // ====================================================================
    public void TriggerFirstPurchaseTutorial()
    {
        if (!hasShownTutorialTernak)
        {
            hasShownTutorialTernak = true;
            StartCoroutine(TutorialTernakSequence());
        }
    }

    private System.Collections.IEnumerator TutorialTernakSequence()
    {
        // 1. PENGAMAN: Tunggu sampai seluruh panel UI tertutup rapat
        while (IsAnyPanelOpen()) yield return null;

        // 🔥 DIPERCEPAT: Kurangi jeda setelah panel tutup (langsung jalan tanpa delay lama)
        yield return new WaitForSeconds(0.2f); 

        textMessage.text = "";
        bubbleObject.SetActive(false);

        // 2. Kakek Transisi Naik ke atas layar
        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekShownPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekShownPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekShownPos;

        // ==========================================
        // TEXT DIALOG 1 (Sapi & Kambing bisa beranak)
        // ==========================================
        bubbleObject.SetActive(true);

        // 🔥 ANIMASI TIGA TITIK (...) HANYA DI SINI (AWAL BANGET)
        textMessage.text = "."; yield return new WaitForSeconds(0.3f); // Dipercepat dari 0.4s
        textMessage.text = ".."; yield return new WaitForSeconds(0.3f);
        textMessage.text = "..."; yield return new WaitForSeconds(0.4f); // Dipercepat dari 0.5s
        textMessage.text = ""; yield return new WaitForSeconds(0.1f);

        // Ketik Teks Pertama
        foreach (char letter in pesanTutorial1.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // 🔥 DIPERCEPAT: Durasi teks pertama mejeng di layar dikurangi agar tidak kelamaan
        yield return new WaitForSeconds(durationVisible * 0.7f); 

        // 4. TRANSISI TEKS BARU: Bubble hilang seketika & teks direset
        bubbleObject.SetActive(false);
        textMessage.text = "";
        yield return new WaitForSeconds(0.3f); // Jeda transisi antar teks dipercepat

        // Cek pengaman darurat kembali
        while (IsAnyPanelOpen()) yield return null;

        // ==========================================
        // TEXT DIALOG 2 (Rajin memberi makan)
        // ==========================================
        bubbleObject.SetActive(true);

        // 🔥 PERBAIKAN: TITIK-TITIK DIHAPUS TOTAL DI SINI, langsung ngetik pesan kedua!
        foreach (char letter in pesanTutorial2.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Durasi teks kedua mejeng di layar sebelum kakek pulang
        yield return new WaitForSeconds(durationVisible);

        // 6. Selesai, Bubble hilang dan kakek transisi turun sembunyi
        bubbleObject.SetActive(false);
        textMessage.text = "";
        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekHiddenPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekHiddenPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekHiddenPos;
    }

    // Fungsi pembantu mengecek status keaktifan seluruh master panel game kamu
    private bool IsAnyPanelOpen()
    {
        bool tokoBuka = false;
        if (TokoManager.instance != null)
        {
            // Deteksi master panel utama toko menggunakan reflection karena tipenya private
            GameObject masterToko = (GameObject)System.Type.GetType("TokoManager")
                .GetField("masterTokoPanelUtama", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(TokoManager.instance);

            if (masterToko != null) tokoBuka = masterToko.activeSelf;
            else if (TokoManager.instance.mainTokoPanel != null) tokoBuka = TokoManager.instance.mainTokoPanel.activeSelf;
        }

        bool jurnalBuka = (JurnalManager.instance != null && JurnalManager.instance.jurnalContent.activeSelf);
        bool misiBuka = (TaskManager.instance != null && TaskManager.instance.misiPanel.activeSelf);
        bool carouselZakatBuka = (ZakatPanelManager.instance != null && ZakatPanelManager.instance.zakatCarouselPanel.activeSelf);

        return tokoBuka || jurnalBuka || misiBuka || carouselZakatBuka;
    }
}