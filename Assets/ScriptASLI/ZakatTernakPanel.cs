using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZakatTernakPanel : MonoBehaviour
{
    [Header("UI Kuis BG Elements")]
    public GameObject panelKuisBG;          // Objek 'kuis-bg'
    public Button btnLanjutKuis;            // Tombol 'lanjut-kuis'
    
    [Tooltip("Masukkan SEMUA tombol jawaban kuis (benar dan salah) dari soal1, soal2, soal3")]
    public List<Button> allQuizButtons; 

    [Tooltip("Masukkan hanya tombol jawaban yang BENAR saja")]
    public List<Button> correctButtons; 
    
    private HashSet<Button> selectedCorrectButtons = new HashSet<Button>();

    [Header("UI Form Kuis Elements")]
    public GameObject panelFormKuis;        // Objek 'form-kuis'
    public TMP_Text txtHartakuTernak;       // Teks 'hartakuemas' (bisa di-rename jadi teks harta ternak)
    public TMP_Text txtDeskripsiZakat;      // Teks 'teks2 (1)'

    [Header("Dropdown Input Fields")]
    public GameObject containerDropdownSapi;     // Objek 'drodown-sapi'
    public GameObject containerDropdownKambing;  // Objek 'drodown-kambing'
    public TMP_Dropdown dropdownSapi;            // Komponen SapiDropdown
    public TMP_Dropdown dropdownKambing;         // Komponen KambingDropdown

    [Header("Action Buttons")]
    public Button btnClose;                 // Tombol 'X' (Close)
    public Button btnSelesaiKuis;           // Tombol 'Selesai' di formulir

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip rewardBacksound; // slot baru untuk backsound reward koin ternak

    [Header("Reward Panel")]
    public GameObject panelReward;
    public Button btnTutupReward;           // 🔥 TAMBAHAN: Tombol OK/Klaim/Tutup di panel reward
    public GameObject panelEndingGame;

    [HideInInspector] public bool isSapiWajib = false;
    [HideInInspector] public bool isKambingWajib = false;

    void Start()
    {
        // 🔥 PERBAIKAN: Fungsi Close khusus untuk Form Kuis / Batal Bayar Ternak
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(() => {
                if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
                else gameObject.SetActive(false);
            });
        }

        if (btnLanjutKuis != null)
        {
            btnLanjutKuis.onClick.RemoveAllListeners();
            btnLanjutKuis.onClick.AddListener(BukaFormKuis);
            btnLanjutKuis.interactable = false;
        }

        if (btnSelesaiKuis != null)
        {
            btnSelesaiKuis.onClick.RemoveAllListeners();
            btnSelesaiKuis.onClick.AddListener(ValidateZakatTernak);
        }

        if (btnTutupReward != null)
        {
            btnTutupReward.onClick.RemoveAllListeners();
            btnTutupReward.onClick.AddListener(KlaimRewardDanTutup);
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        SetupAllQuizButtons();
    }

    void OnEnable()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(true);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelReward != null) panelReward.SetActive(false); // Pastikan reward panel mati saat di-buka kembali
        if (btnLanjutKuis != null) btnLanjutKuis.interactable = false;
        selectedCorrectButtons.Clear();

        foreach (Button btn in allQuizButtons)
        {
            if (btn != null) btn.GetComponent<Image>().color = Color.white;
        }

        ConfigureFormDinamis();
    }

    void SetupAllQuizButtons()
    {
        foreach (Button btn in allQuizButtons)
        {
            if (btn == null) continue;

            bool isCorrectAnswer = correctButtons.Contains(btn);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerSelected(btn, isCorrectAnswer));
        }
    }

    public void OnAnswerSelected(Button clickedBtn, bool isCorrect)
    {
        if (isCorrect)
        {
            if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
            clickedBtn.GetComponent<Image>().color = Color.green;
            
            selectedCorrectButtons.Add(clickedBtn);

            // Jika semua tombol benar yang ada di daftar sudah ditekan, tombol lanjut menyala
            if (selectedCorrectButtons.Count == correctButtons.Count)
            {
                if (btnLanjutKuis != null) btnLanjutKuis.interactable = true;
            }
        }
        else
        {
            if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);
            StartCoroutine(WrongAnswerEffect(clickedBtn));
        }
    }

    IEnumerator WrongAnswerEffect(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        Color origColor = Color.white;
        Vector3 origPos = btn.transform.localPosition;

        img.color = Color.red;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            float x = Random.Range(-1f, 1f) * 5f;
            btn.transform.localPosition = new Vector3(origPos.x + x, origPos.y, origPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        btn.transform.localPosition = origPos;
        img.color = origColor;
    }

    public void ConfigureFormDinamis()
    {
        if (JurnalManager.instance == null) return;

        // 🔥 PERBAIKAN: Panggil langsung secara bersih tanpa Reflection
        int sapiSekarang = JurnalManager.instance.GetJumlahSapiRealTime();
        int kambingSekarang = JurnalManager.instance.GetJumlahKambingRealTime();

        isSapiWajib = sapiSekarang >= JurnalManager.instance.nisabSapiKriteria;
        isKambingWajib = kambingSekarang >= JurnalManager.instance.nisabKambingKriteria;

        if (isSapiWajib && isKambingWajib)
        {
            txtHartakuTernak.text = $"Sapi : {sapiSekarang} Ekor\nKambing : {kambingSekarang} Ekor";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }
        else if (isSapiWajib)
        {
            txtHartakuTernak.text = $"Sapi : {sapiSekarang} Ekor";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }
        else if (isKambingWajib)
        {
            txtHartakuTernak.text = $"Kambing : {kambingSekarang} Ekor";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }

        if (containerDropdownSapi != null) containerDropdownSapi.SetActive(isSapiWajib);
        if (containerDropdownKambing != null) containerDropdownKambing.SetActive(isKambingWajib);

        if (dropdownSapi != null) dropdownSapi.value = 0;
        if (dropdownKambing != null) dropdownKambing.value = 0;
    }

    void BukaFormKuis()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(true);

        // 🔥 TAMBAHAN: Paksa Jurnal untuk mengunci nilainya sekarang juga!
        if (JurnalManager.instance != null)
        {
            JurnalManager.instance.isTernakLockedInJurnal = true;
        }

        // Segarkan form dinamis agar data Sapi & Kambing terbaru yang masuk nisab tertulis berbarengan
        ConfigureFormDinamis();
    }

    public void ValidateZakatTernak()
    {
        if (JurnalManager.instance == null) return;

        int sapiSekarang = JurnalManager.instance.GetJumlahSapiRealTime();
        int kambingSekarang = JurnalManager.instance.GetJumlahKambingRealTime();

        bool sapiValid = false;
        bool kambingValid = false;

        if (isSapiWajib && dropdownSapi != null)
        {
            int idx = dropdownSapi.value;

            if (sapiSekarang >= 30 && sapiSekarang < 40)        sapiValid = (idx == 1);
            else if (sapiSekarang >= 40 && sapiSekarang < 60)   sapiValid = (idx == 2);
            else if (sapiSekarang >= 60 && sapiSekarang < 70)   sapiValid = (idx == 3); 
            else if (sapiSekarang >= 70 && sapiSekarang < 80)   sapiValid = (idx == 4);
            else if (sapiSekarang >= 80 && sapiSekarang < 90)   sapiValid = (idx == 5);
            
            // 🔥 SEKARANG SUDAH DIPECARKAN SECARA PRESISI:
            else if (sapiSekarang >= 90 && sapiSekarang < 100)  sapiValid = (idx == 6); // 90-99 ekor = 3 Tabi' (Index 6)
            else if (sapiSekarang >= 100 && sapiSekarang < 110) sapiValid = (idx == 8); // 100-109 ekor = 1 Musinnah & 2 Tabi' (Index 8)
            
            else if (sapiSekarang >= 110 && sapiSekarang < 120) sapiValid = (idx == 7); // 110-119 ekor = 2 Musinnah & 1 Tabi' (Index 7)
            
            // --- Untuk Kelipatan Atas Menyesuaikan Pergeseran Indeks Dropdown ---
            else if (sapiSekarang >= 120 && sapiSekarang < 130) sapiValid = (idx == 10 || idx == 11); // 3 Musinnah atau 4 Tabi'
            else if (sapiSekarang >= 130 && sapiSekarang < 140) sapiValid = (idx == 10);
            else if (sapiSekarang >= 140 && sapiSekarang < 150) sapiValid = (idx == 11 || idx == 12); // 4 Tabi' atau 2 Tabi' & 2 Musinnah
            else if (sapiSekarang >= 150 && sapiSekarang < 160) sapiValid = (idx == 13); 
            else if (sapiSekarang >= 160 && sapiSekarang < 170) sapiValid = (idx == 14); // 4 ekor Musinnah (Index 14)
            else if (sapiSekarang >= 170 && sapiSekarang < 180) sapiValid = (idx == 15); 
            else if (sapiSekarang >= 180 && sapiSekarang < 200) sapiValid = (idx == 16);
            else if (sapiSekarang >= 200)                       sapiValid = (idx == 17);
        }
        else if (!isSapiWajib)
        {
            sapiValid = true; 
        }

        // --- VALIDASI KAMBING ---
        // --- VALIDASI KAMBING DINAMIS (RANGE SAMPAI RATUSAN) ---
        if (isKambingWajib && dropdownKambing != null)
        {
            int idxKambing = dropdownKambing.value;

            if (kambingSekarang >= 40 && kambingSekarang <= 120)
            {
                kambingValid = (idxKambing == 1); // Wajib 1 ekor Kambing
            }
            else if (kambingSekarang >= 121 && kambingSekarang <= 200)
            {
                kambingValid = (idxKambing == 2); // Wajib 2 ekor Kambing (Cocok untuk 145 ekor!)
            }
        }
        else if (!isKambingWajib)
        {
            kambingValid = true;
        }

        // --- EKSEKUSI PEMOTONGAN JIKA BENAR ---
        if (sapiValid && kambingValid)
        {
            if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);

            // --- EKSEKUSI PEMOTONGAN JIKA BENAR ---
            if (isSapiWajib)
            {
                int currentInternalSapi = (int)typeof(JurnalManager).GetField("totalEkorSapiInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(JurnalManager.instance);
                
                int pengurangSapi = 30; 
                if (sapiSekarang >= 200) pengurangSapi = 200;
                else if (sapiSekarang >= 180) pengurangSapi = 180;
                else if (sapiSekarang >= 170) pengurangSapi = 170;
                else if (sapiSekarang >= 160) pengurangSapi = 160;
                else if (sapiSekarang >= 140) pengurangSapi = 140; 
                else if (sapiSekarang >= 120) pengurangSapi = 120;
                else if (sapiSekarang >= 110) pengurangSapi = 110;
                else if (sapiSekarang >= 100) pengurangSapi = 100; // 🔥 Potong pas 100 ekor jika sukses di range ini
                else if (sapiSekarang >= 90)  pengurangSapi = 90;
                else if (sapiSekarang >= 80)  pengurangSapi = 80;
                else if (sapiSekarang >= 70)  pengurangSapi = 70;
                else if (sapiSekarang >= 60)  pengurangSapi = 60;
                else if (sapiSekarang >= 40)  pengurangSapi = 40;

                typeof(JurnalManager).GetField("totalEkorSapiInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(JurnalManager.instance, Mathf.Max(0, currentInternalSapi - pengurangSapi));
            }

            if (isKambingWajib)
            {
                int currentInternalKambing = (int)typeof(JurnalManager).GetField("totalEkorKambingInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(JurnalManager.instance);
                typeof(JurnalManager).GetField("totalEkorKambingInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(JurnalManager.instance, Mathf.Max(0, currentInternalKambing - 40));
            }

            if (panelReward != null) panelReward.SetActive(true); 
            if (audioSource != null && rewardBacksound != null) audioSource.PlayOneShot(rewardBacksound);
            if (panelFormKuis != null) panelFormKuis.SetActive(false); 
        }
        else
        {
            if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
            Debug.Log("Jawaban kalkulasi jenis/jumlah zakat ternak masih keliru!");
        }
    }

    // 🔥 TAMBAHAN: Fungsi eksekusi reward koin saat menutup panel reward
    void KlaimRewardDanTutup()
    {   
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        if (JurnalManager.instance != null)
        {
            JurnalManager.instance.MatikanSistemBeranak();
        }
        if (MoneyManager.instance != null)
        {
            MoneyManager.instance.AddMoney(100000);
        }

        if (ZakatPanelManager.instance != null) // Sesuai nama ZakatPanelManager kamu
        {
            ZakatPanelManager.instance.isPeternakanCompleted = true; 
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();     
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();  
            ZakatPanelManager.instance.CloseZakatPanel(); // Paksa tutup carousel buku zakatnya
        }

        if (panelReward != null) panelReward.SetActive(false);

        // 🔥 UBAH DI SINI: Munculkan panel ending game kamu!
        if (panelEndingGame != null)
        {
            panelEndingGame.SetActive(true);
        }

        // Tutup master panel kuis ternak ini
        if (UIManager.instance != null) 
        {
            UIManager.instance.ClosePanelMenu(gameObject);
        }
        else 
        {
            gameObject.SetActive(false);
        }
    }
}