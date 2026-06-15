using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ZakatPerdaganganPanel : MonoBehaviour
{
[Header("UI Elements")]
    public Button btnClose;          // Tombol silang 'X' di panel kuis/formulir biasa
    public Button btnLanjut;
    public Button btnCloseReward;    // Slot lama (bisa tetap dipakai atau disesuaikan)
    public Button btnTutupReward;    // 🔥 TAMBAHAN BARU: Tombol khusus di dalam panel reward perdagangan

    [Header("Questions")]
    public Button answerA1; 
    public Button answerB1;
    public Button answerC1;
    
    public Button answerA2;
    public Button answerB2; 
    public Button answerC2;

    public Button answerA3;
    public Button answerB3; 
    public Button answerC3;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Komponen AudioSource
    public AudioClip correctSound;  // Klip suara benar
    public AudioClip wrongSound;
    public AudioClip rewardBacksound; // slot baru untuk backsound reward koin perdagangan

    [Header("Settings")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    [Header("Panel Transition")]
    public GameObject panelKuisBG;
    public GameObject panelFormKuis;
    public GameObject panelZakatCarousel; // Tambahkan ini jika belum ada
    public GameObject panelRewardDagang;

    void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            // 🔥 DISAMAKAN PERSIS dengan ZakatEmasPerakPanel dan ZakatTernakPanel
            btnClose.onClick.AddListener(() => {
                if (UIManager.instance != null) 
                    UIManager.instance.ClosePanelMenu(gameObject);
                else 
                    gameObject.SetActive(false);
            });
        }

        // --- Sisa kode tombol di bawahnya tetap dibiarkan utuh dan tidak diganggu ---
        if (btnTutupReward != null)
        {
            btnTutupReward.onClick.RemoveAllListeners();
            btnTutupReward.onClick.AddListener(KlaimRewardDanClose);
        }
        
        if (btnCloseReward != null)
        {
            btnCloseReward.onClick.RemoveAllListeners();
            btnCloseReward.onClick.AddListener(KlaimRewardDanClose);
        }

        SetupButton(answerA1, true);
        SetupButton(answerB1, false);
        SetupButton(answerC1, false);

        SetupButton(answerA2, false);
        SetupButton(answerB2, true);
        SetupButton(answerC2, false);

        SetupButton(answerA3, false);
        SetupButton(answerB3, true);
        SetupButton(answerC3, false);
        
        if (btnLanjut != null)
        {
            btnLanjut.onClick.RemoveAllListeners();
            btnLanjut.onClick.AddListener(BukaFormKuis);
            btnLanjut.interactable = false;
        }
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }
    void OnEnable()
    {
        // 🔥 RESET ALUR: Setiap kali dibuka dari Carousel, pastikan halaman kuis depan yang menyala, dan form dalam mati
        if (panelKuisBG != null) panelKuisBG.SetActive(true);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);
        
        if (btnLanjut != null) btnLanjut.interactable = false;

        // Kembalikan warna tombol kuis perdagangan menjadi putih bersih untuk dicoba lagi jika sebelumnya salah
        Button[] kuisButtons = new Button[] { answerA1, answerB1, answerC1, answerA2, answerB2, answerC2, answerA3, answerB3, answerC3 };
        foreach (Button btn in kuisButtons)
        {
            if (btn != null)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null) img.color = Color.white;
            }
        }
    }
    void BukaFormKuis()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false); 
        if (panelFormKuis != null) panelFormKuis.SetActive(true); 

        // --- TAMBAHKAN KODE INI ---
        // Paksa kalkulator perdagangan untuk mengambil nilai terbaru dari MoneyManager saat form dibuka
        ZakatCalculator calc = GetComponentInChildren<ZakatCalculator>(true);
        if (calc != null)
        {
            calc.SetupHartaRupiah();
        }
    }

    void SetupButton(Button btn, bool isCorrect)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => OnAnswerSelected(btn, isCorrect));
    }

    void OnAnswerSelected(Button clickedBtn, bool isCorrect)
    {
        Image btnImg = clickedBtn.GetComponent<Image>();
        TMP_Text btnText = clickedBtn.GetComponentInChildren<TMP_Text>();

        if (isCorrect)
        {   PlaySound(correctSound);
            btnImg.color = correctColor;
            // if (btnText != null) btnText.color = Color.white;
            // Jika benar, kita biarkan tetap hijau
            CheckAllAnswers();
        }
        else
        {   PlaySound(wrongSound);
            // Jalankan Coroutine untuk efek sementara
            StartCoroutine(WrongAnswerEffect(clickedBtn, btnImg, btnText));
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Coroutine gabungan untuk warna merah, getar, lalu balik ke putih
    IEnumerator WrongAnswerEffect(Button btn, Image img, TMP_Text text)
    {
        // Simpan data awal
        Color originalColor = Color.white; // Asumsi warna dasar tombol adalah putih
        Color originalTextColor = text != null ? text.color : Color.black; 
        Vector3 originalPos = btn.transform.localPosition;

        // Set ke warna salah
        img.color = wrongColor;
        if (text != null) text.color = Color.white;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            btn.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Kembalikan ke posisi, warna, dan teks semula
        btn.transform.localPosition = originalPos;
        img.color = originalColor;
        if (text != null) text.color = originalTextColor;
    }

    void CheckAllAnswers()
    {
        if (answerA1.GetComponent<Image>().color == correctColor &&
            answerB2.GetComponent<Image>().color == correctColor &&
            answerB3.GetComponent<Image>().color == correctColor)
        {   
            if (JurnalManager.instance != null)
            {
                JurnalManager.instance.isDagangLockedInJurnal = true;
            }
            if (btnLanjut != null) btnLanjut.interactable = true;
        }
    }
    public void MunculkanReward()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelZakatCarousel != null) panelZakatCarousel.SetActive(false);
        
        if (panelRewardDagang != null) panelRewardDagang.SetActive(true);

        // 🔥 UBAH BAGIAN INI: Putar suara reward hanya sekali saja
        if (audioSource != null && rewardBacksound != null)
        {
            audioSource.PlayOneShot(rewardBacksound);
        }
    }

    // Fungsi baru untuk menghandle pemberian uang pasca kuis level 1 selesai
    void KlaimRewardDanClose()
    {
        // 1. Tambah data 100 gram PERAK ke dompet babak 2 secara resmi (Sama persis seperti sistem cheat K)
        if (MoneyManager.instance != null)
        {
            // Menambah 100 perak, otomatis memicu UpdateEmasPerakUI() dan CheckEmasPerakNisab() bawaan game
            MoneyManager.instance.AddPerak(100); 
            Debug.Log("<color=green>[Jalur Normal]</color> Sukses menambahkan 100 Gram Perak dari Reward Kuis!");
        }

        // 2. Akses ZakatPanelManager agar tahu kalau perdagangan sekarang sudah UNLOCKED
        ZakatPanelManager panelManager = FindFirstObjectByType<ZakatPanelManager>();
        if (panelManager != null)
        {
            panelManager.isPerdaganganUnlocked = true;
            panelManager.UpdatePaymentButton();
            panelManager.UpdateItemVisuals();
        }

        // 3. PANGGIL LEVEL 2 MANAGER UNTUK TRANFISI LINGKUNGAN & MISI
        if (Level2Manager.instance != null)
        {
            Level2Manager.instance.SwitchToLevel2();
        }
        else
        {
            Debug.LogError("[ZakatPerdaganganPanel] Level2Manager.instance tidak ditemukan di scene!");
        }

        // 4. Matikan semua sub-panel kuis (Termasuk background kuis luar/papan baliho) dan nonaktifkan GameObject kuis ini
        if (panelKuisBG != null) panelKuisBG.SetActive(false); // 🔥 TAMBAHAN UTAMA: Matikan baliho papan putih agar tidak menyangkut di layar
        if (panelFormKuis != null) panelFormKuis.SetActive(false); 
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);
        
        gameObject.SetActive(false); 
    }
    // =================================================================
    // 🔥 FUNGSI MODULAR UNTUK DIASINGKAN KE ON CLICK () INSPECTOR
    // =================================================================

    /// <summary>
    /// Fungsi khusus untuk Tombol Close (X). 
    /// Mematikan kuis dagang secara paksa dan langsung memerintahkan UIManager untuk menutup paket UI ini agar HUD kembali muncul.
    /// </summary>
    public void TombolCloseBatalKuis()
    {
        // Matikan sub-panel internal secara bersih
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);

        // Tutup via UIManager agar tumpukan stack UI bersih dan HUD dipaksa muncul kembali
        if (UIManager.instance != null)
        {
            UIManager.instance.ClosePanelMenu(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Fungsi alternatif jika kamu ingin mematikan total Buku Carousel Zakat di background secara instan saat tombol diklik.
    /// </summary>
    public void PaksaTutupBukuZakatCarousel()
    {
        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.CloseZakatPanel();
        }
    }

    /// <summary>
    /// Fungsi untuk mematikan GameObject script ini sendiri secara total tanpa lewat UIManager.
    /// </summary>
    public void MatikanObjectIniLangsung()
    {
        gameObject.SetActive(false);
    }
}