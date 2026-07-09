using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZakatEmasPerakPanel : MonoBehaviour
{
    [Header("UI Elements Utama")]
    public Button btnClose;          // Tombol silang 'X' untuk batal/tutup kuis biasa
    public Button btnLanjutKuisBabak; // Untuk navigasi lanjut babak kuis (Babak 1 -> 2 -> 3)
    public Button btnLanjutFormUtama; // Untuk beralih ke form kuis kalkulator (muncul kalau nilai >= 7)
    public Button btnTutupReward;    

    [Header("--- DAFTAR TOMBOL SOAL (SERET KE SINI) ---")]
    [Header("Babak 1")]
    public Button answerA1; public Button answerB1; public Button answerC1;
    public Button answerA2; public Button answerB2; public Button answerC2;
    public Button answerA3; public Button answerB3; public Button answerC3;
    
    [Header("Babak 2")]
    public Button answerA4; public Button answerB4; public Button answerC4;
    public Button answerA5; public Button answerB5; public Button answerC5;
    public Button answerA6; public Button answerB6; public Button answerC6;

    [Header("Babak 3")]
    public Button answerA7; public Button answerB7; public Button answerC7;
    public Button answerA8; public Button answerB8; public Button answerC8;
    public Button answerA9; public Button answerB9; public Button answerC9;

    [Header("Babak 4")]
    public Button answerA10; public Button answerB10; public Button answerC10;

    [Header("Sistem Babak & Hasil")]
    public List<GameObject> panelBabakObjects; // Elemen 0 = Babak 1, Elemen 1 = Babak 2, dst
    public Button btnKalkulasiNilai;
    public Button btnUlangiKuis;
    public GameObject panelHasilKuis; 
    public TMP_Text txtNilaiFormat;    
    public TMP_Text txtApresiasi;      

    [Header("UI Form Kuis Elements")]
    public GameObject panelFormKuis;
    public TMP_Text txtHartaku; 
    public TMP_Text txtDeskripsiZakat; 

    [Header("Dynamic Input Fields")]
    public GameObject containerZakatEmas;  
    public GameObject containerZakatPerak; 
    public TMP_InputField inputZakatEmas;   
    public TMP_InputField inputZakatPerak;  

    [Header("Action Buttons Form")]
    public Button btnSelesaiKuis; 

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSound; // Sound 1 tunggal untuk setiap klik
    public AudioClip wrongSound;
    public AudioClip rewardBacksound;
    public AudioClip clickSound;

    [Header("Reward Panel")]
    public GameObject panelReward;
    public GameObject panelKuisBG;

    [HideInInspector] public bool isEmasWajib = false;
    [HideInInspector] public bool isPerakWajib = false;

    private int currentBabak = 1; 
    private int skorBenar = 0;
    private int jumlahJawabanDiBabakIni = 0;
    private HashSet<int> soalSudahDijawab = new HashSet<int>();

    void Start()
    {
        // Setup tombol dasar sesuai request (Close akan mereset dan menutup)
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners(); 
            btnClose.onClick.AddListener(TombolCloseBatalKuis);
        }

        if (btnLanjutKuisBabak != null)
        {
            btnLanjutKuisBabak.onClick.RemoveAllListeners();
            btnLanjutKuisBabak.onClick.AddListener(LanjutBabakBerikutnya);
        }

        if (btnLanjutFormUtama != null)
        {
            btnLanjutFormUtama.onClick.RemoveAllListeners();
            btnLanjutFormUtama.onClick.AddListener(BukaFormKuis);
        }

        if (btnKalkulasiNilai != null)
        {
            btnKalkulasiNilai.onClick.RemoveAllListeners();
            btnKalkulasiNilai.onClick.AddListener(KalkulasiNilaiAkhir);
        }

        if (btnUlangiKuis != null)
        {
            btnUlangiKuis.onClick.RemoveAllListeners();
            btnUlangiKuis.onClick.AddListener(ResetDanUlangiKuis);
        }

        if (btnSelesaiKuis != null)
        {
            btnSelesaiKuis.onClick.RemoveAllListeners();
            btnSelesaiKuis.onClick.AddListener(ValidateZakatEmasPerak);
        }

        if (btnTutupReward != null)
        {
            btnTutupReward.onClick.RemoveAllListeners();
            btnTutupReward.onClick.AddListener(TutupRewardDanMatikanAudio);
        }

        // 🔥 DAFTARKAN SEMUA TOMBOL JAWABAN SEPERTI DI PERDAGANGAN
        SetupJawaban(answerA1, 1);  SetupJawaban(answerB1, 1);  SetupJawaban(answerC1, 1);
        SetupJawaban(answerA2, 2);  SetupJawaban(answerB2, 2);  SetupJawaban(answerC2, 2);
        SetupJawaban(answerA3, 3);  SetupJawaban(answerB3, 3);  SetupJawaban(answerC3, 3);

        SetupJawaban(answerA4, 4);  SetupJawaban(answerB4, 4);  SetupJawaban(answerC4, 4);
        SetupJawaban(answerA5, 5);  SetupJawaban(answerB5, 5);  SetupJawaban(answerC5, 5);
        SetupJawaban(answerA6, 6);  SetupJawaban(answerB6, 6);  SetupJawaban(answerC6, 6);

        SetupJawaban(answerA7, 7);  SetupJawaban(answerB7, 7);  SetupJawaban(answerC7, 7);
        SetupJawaban(answerA8, 8);  SetupJawaban(answerB8, 8);  SetupJawaban(answerC8, 8);
        SetupJawaban(answerA9, 9);  SetupJawaban(answerB9, 9);  SetupJawaban(answerC9, 9);

        SetupJawaban(answerA10, 10); SetupJawaban(answerB10, 10); SetupJawaban(answerC10, 10);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        InisialisasiAwalKuis();
    }

    void OnEnable()
    {
        // 🛡️ Proteksi Ganda: Jika ternyata sudah selesai, langsung matikan paksa!
        if (ZakatPanelManager.instance != null && ZakatPanelManager.instance.isEmasPerakCompleted)
        {
            Debug.Log("[Proteksi] Panel Emas Perak mendeteksi sudah completed. Mematikan diri!");
            gameObject.SetActive(false);
            return;
        }

        if (panelKuisBG != null) panelKuisBG.SetActive(true);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelReward != null) panelReward.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);
        InisialisasiAwalKuis();
    }

    void InisialisasiAwalKuis()
    {
        currentBabak = 1; skorBenar = 0; jumlahJawabanDiBabakIni = 0;
        soalSudahDijawab.Clear();

        if (btnLanjutKuisBabak != null) btnLanjutKuisBabak.gameObject.SetActive(false);
        if (btnLanjutFormUtama != null) btnLanjutFormUtama.gameObject.SetActive(false);
        if (btnKalkulasiNilai != null) btnKalkulasiNilai.gameObject.SetActive(false);
        if (btnUlangiKuis != null) btnUlangiKuis.gameObject.SetActive(false);

        UpdateVisualBabak();
    }

    void SetupJawaban(Button btn, int nomorSoal)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => EksekusiKlikJawaban(btn, nomorSoal));
    }

    void EksekusiKlikJawaban(Button tombolDitekan, int nomorSoal)
    {
        // 1. KUNCI: Jika soal ini sudah pernah dijawab, blokir agar tidak bisa klik opsi lain
        if (soalSudahDijawab.Contains(nomorSoal)) return;

        // Ambil komponen Parent dari tombol ini (yaitu objek Soal)
        Transform soalParent = tombolDitekan.transform.parent;
        Button[] tombolPasangan = (soalParent != null) ? soalParent.GetComponentsInChildren<Button>(true) : new Button[0];

        // Cari tahu apakah tombol yang ditekan ini adalah jawaban yang benar
        bool isCorrect = CekApakahJawabanBenar(nomorSoal, tombolDitekan);

        // 2. EVALUASI JAWABAN LANGSUNG
        if (isCorrect)
        {
            // JIKA BENAR: Putar sound correct & beri warna Hijau Terang murni
            if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
            tombolDitekan.GetComponent<Image>().color = Color.green; // Tetap hijau biasa agar dibaca sistem kalkulasi lamamu
        }
        else
        {
            // JIKA SALAH: Putar sound wrong & beri warna Merah
            if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);
            tombolDitekan.GetComponent<Image>().color = Color.red;

            // Beri tanda Hijau Sage pada jawaban yang seharusnya benar (Hanya untuk koreksi visual)
            TandaiJawabanBenarDiUI(nomorSoal);
        }

        // 3. Simpan status tracking soal[cite: 6]
        soalSudahDijawab.Add(nomorSoal);
        jumlahJawabanDiBabakIni++;

        // Atur kemunculan tombol navigasi babak[cite: 6]
        if (currentBabak <= 3)
        {
            if (jumlahJawabanDiBabakIni >= 3)
            {
                if (btnLanjutKuisBabak != null) btnLanjutKuisBabak.gameObject.SetActive(true);
            }
        }
        else if (currentBabak == 4)
        {
            if (btnKalkulasiNilai != null) btnKalkulasiNilai.gameObject.SetActive(true);
        }
    }

    // 🔥 FUNGSI PEMBANTU 1: Mengetahui kunci jawaban real-time berdasarkan data KalkulasiNilaiAkhir milikmu[cite: 6]
    private bool CekApakahJawabanBenar(int nomorSoal, Button tombolDitekan)
    {
        switch (nomorSoal)
        {
            case 1: return tombolDitekan == answerB1; // Soal 1 = B1 True[cite: 6]
            case 2: return tombolDitekan == answerA2; // Soal 2 = A2 True[cite: 6]
            case 3: return tombolDitekan == answerC3; // Soal 3 = C3 True[cite: 6]
            case 4: return tombolDitekan == answerB4; // Soal 4 = B4 True[cite: 6]
            case 5: return tombolDitekan == answerB5; // Soal 5 = B5 True[cite: 6]
            case 6: return tombolDitekan == answerA6; // Soal 6 = A6 True[cite: 6]
            case 7: return tombolDitekan == answerA7; // Soal 7 = A7 True[cite: 6]
            case 8: return tombolDitekan == answerA8; // Soal 8 = A8 True[cite: 6]
            case 9: return tombolDitekan == answerA9; // Soal 9 = A9 True[cite: 6]
            case 10: return tombolDitekan == answerA10; // Soal 10 = A10 True[cite: 6]
            default: return false;
        }
    }

    // 🔥 FUNGSI PEMBANTU 2: Mewarnai jawaban yang benar dengan warna Hijau Sage saat pemain salah
    private void TandaiJawabanBenarDiUI(int nomorSoal)
    {
        Button tombolBenar = null;
        Color hijauSage = new Color(0.49f, 0.97f, 0.49f, 1f); // Menyesuaikan hex code hijau sage[cite: 7]

        switch (nomorSoal)
        {
            case 1: tombolBenar = answerB1; break;
            case 2: tombolBenar = answerA2; break;
            case 3: tombolBenar = answerC3; break;
            case 4: tombolBenar = answerB4; break;
            case 5: tombolBenar = answerB5; break;
            case 6: tombolBenar = answerA6; break;
            case 7: tombolBenar = answerA7; break;
            case 8: tombolBenar = answerA8; break;
            case 9: tombolBenar = answerA9; break;
            case 10: tombolBenar = answerA10; break;
        }

        if (tombolBenar != null)
        {
            tombolBenar.GetComponent<Image>().color = hijauSage;
        }
    }

    void LanjutBabakBerikutnya()
    {
        currentBabak++;
        UpdateVisualBabak();
    }

    void UpdateVisualBabak()
    {
        for (int i = 0; i < panelBabakObjects.Count; i++)
        {
            if (panelBabakObjects[i] != null) panelBabakObjects[i].SetActive((i + 1) == currentBabak);
        }
        jumlahJawabanDiBabakIni = 0;
        if (btnLanjutKuisBabak != null) btnLanjutKuisBabak.gameObject.SetActive(false);
    }

    void KalkulasiNilaiAkhir()
    {   if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(true);
        if (btnKalkulasiNilai != null) btnKalkulasiNilai.gameObject.SetActive(false);

        skorBenar = 0;

        // 🔥 KUNCI JAWABAN KUIS EMAS PERAK
        // format: CheckSkorTombol(NamaVariabelTombol, ApakahIniKunciJawabanYangBenar);
        // Silakan sesuaikan letak kata 'true' di bawah ini dengan kunci jawaban kuis materi Emas & Perak milikmu!
        
        // Soal 1
        CheckSkorTombol(answerA1, false);   CheckSkorTombol(answerB1, true);  CheckSkorTombol(answerC1, false);
        // Soal 2
        CheckSkorTombol(answerA2, true);  CheckSkorTombol(answerB2, false);   CheckSkorTombol(answerC2, false);
        // Soal 3
        CheckSkorTombol(answerA3, false);  CheckSkorTombol(answerB3, false);  CheckSkorTombol(answerC3, true);
        // Soal 4
        CheckSkorTombol(answerA4, false);   CheckSkorTombol(answerB4, true);  CheckSkorTombol(answerC4, false);
        // Soal 5
        CheckSkorTombol(answerA5, false);  CheckSkorTombol(answerB5, true);   CheckSkorTombol(answerC5, false);
        // Soal 6
        CheckSkorTombol(answerA6, true);  CheckSkorTombol(answerB6, false);  CheckSkorTombol(answerC6, false);
        // Soal 7
        CheckSkorTombol(answerA7, true);   CheckSkorTombol(answerB7, false);  CheckSkorTombol(answerC7, false);
        // Soal 8
        CheckSkorTombol(answerA8, true);  CheckSkorTombol(answerB8, false);   CheckSkorTombol(answerC8, false);
        // Soal 9
        CheckSkorTombol(answerA9, true);  CheckSkorTombol(answerB9, false);  CheckSkorTombol(answerC9, false);
        // Soal 10
        CheckSkorTombol(answerA10, true); CheckSkorTombol(answerB10, false);  CheckSkorTombol(answerC10, false);

        if (txtNilaiFormat != null)
        {
            txtNilaiFormat.text = skorBenar.ToString() + "/10";
        }

        if (txtApresiasi != null)
        {
            if (skorBenar >= 1 && skorBenar <= 3) txtApresiasi.text = "Jangan berkecil hati, mari pelajari kembali modul zakat emas/perak dan coba lagi!";
            else if (skorBenar >= 4 && skorBenar <= 6) txtApresiasi.text = "Cukup baik! Sedikit lagi kamu bisa memahami konsep zakat emas/perak dengan sempurna.";
            else if (skorBenar >= 7 && skorBenar <= 9) txtApresiasi.text = "Luar biasa! Pemahamanmu mengenai zakat emas/perak sudah sangat matang.";
            else if (skorBenar == 10) txtApresiasi.text = "Sempurna! Kamu berhasil menjawab seluruh pertanyaan kuis dengan benar!";
        }

        if (btnUlangiKuis != null) btnUlangiKuis.gameObject.SetActive(true);

        if (skorBenar >= 7)
        {
            if (btnLanjutFormUtama != null) btnLanjutFormUtama.gameObject.SetActive(true);
            if (JurnalManager.instance != null) JurnalManager.instance.isEmasLockedInJurnal = true;
        }
        else
        {
            if (btnLanjutFormUtama != null) btnLanjutFormUtama.gameObject.SetActive(false);
        }
    }

    void CheckSkorTombol(Button btn, bool isCorrectKey)
    {
        if (btn != null && isCorrectKey)
        {
            if (btn.GetComponent<Image>().color == Color.green)
            {
                skorBenar++;
            }
        }
    }

    void ResetDanUlangiKuis()
    {
        Button[] semuaTombol = new Button[] { 
            answerA1, answerB1, answerC1, answerA2, answerB2, answerC2, answerA3, answerB3, answerC3,
            answerA4, answerB4, answerC4, answerA5, answerB5, answerC5, answerA6, answerB6, answerC6,
            answerA7, answerB7, answerC7, answerA8, answerB8, answerC8, answerA9, answerB9, answerC9,
            answerA10, answerB10, answerC10
        };

        foreach (Button b in semuaTombol)
        {
            if (b != null) b.GetComponent<Image>().color = Color.white;
        }

        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);
        InisialisasiAwalKuis();
    }

    public void TombolCloseBatalKuis()
    {
        ResetDanUlangiKuis();

        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelReward != null) panelReward.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);

        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
        else gameObject.SetActive(false);
    }

    // --- TETAP MENJAGA LOGIKA FORM KALKULATOR EMAS PERAK BAWAAN (TIDAK BERUBAH) ---
    void BukaFormKuis()
    {   if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(true);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);

        ConfigureFormDinamis();
    }

    public void ConfigureFormDinamis()
    {
        if (MoneyManager.instance == null || JurnalManager.instance == null) return;

        int emasSekarang = MoneyManager.instance.totalEmas;
        int perakSekarang = MoneyManager.instance.totalPerak;

        isEmasWajib = emasSekarang >= JurnalManager.instance.nisabEmasKriteria;
        isPerakWajib = perakSekarang >= JurnalManager.instance.nisabPerakKriteria;

        if (isEmasWajib && isPerakWajib)
        {
            txtHartaku.text = $"Emas : {emasSekarang} Gram\nPerak : {perakSekarang} Gram";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "dari total simpanan emas dan perak sebesar 2.5% pada tahun ini sejumlah :";
        }
        else if (isEmasWajib)
        {
            txtHartaku.text = $"Emas : {emasSekarang} Gram";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "dari total simpanan emas sebesar 2.5% pada tahun ini sejumlah :";
        }
        else if (isPerakWajib)
        {
            txtHartaku.text = $"Perak : {perakSekarang} Gram";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "dari total simpanan perak sebesar 2.5% pada tahun ini sejumlah :";
        }

        if (containerZakatEmas != null) containerZakatEmas.SetActive(isEmasWajib);
        if (containerZakatPerak != null) containerZakatPerak.SetActive(isPerakWajib);

        if (inputZakatEmas != null) inputZakatEmas.text = "";
        if (inputZakatPerak != null) inputZakatPerak.text = "";
    }

    public void ValidateZakatEmasPerak()
    {   if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (MoneyManager.instance == null) return;

        bool emasValid = true;
        bool perakValid = true;

        float correctEmasAmount = MoneyManager.instance.totalEmas * 0.025f;
        float correctPerakAmount = MoneyManager.instance.totalPerak * 0.025f;

        // --- 1. VALIDASI EMAS ---
        if (isEmasWajib && inputZakatEmas != null)
        {
            // Bersihkan teks " gr", spasi, dan ganti koma menjadi titik desimal
            string cleanEmas = inputZakatEmas.text.Replace(" gr", "").Replace(" ", "").Replace(",", ".");
            
            if (float.TryParse(cleanEmas, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float guessEmas))
            {
                emasValid = Mathf.Abs(guessEmas - correctEmasAmount) < 0.1f;
            }
            else 
            {
                emasValid = false;
            }
        }

        // --- 2. VALIDASI PERAK ---
        if (isPerakWajib && inputZakatPerak != null)
        {
            // SEKARANG SUDAH AMAN: Perak juga dibersihkan dari " gr" dan spasi
            string cleanPerak = inputZakatPerak.text.Replace(" gr", "").Replace(" ", "").Replace(",", ".");
            
            if (float.TryParse(cleanPerak, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float guessPerak))
            {
                perakValid = Mathf.Abs(guessPerak - correctPerakAmount) < 0.1f;
            }
            else 
            {
                perakValid = false;
            }
        }

        // --- 3. EKSEKUSI PEMBAYARAN JIKA KEDUANYA BENAR ---
        if (emasValid && perakValid)
        {
            if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);
            
            if (JurnalManager.instance != null) JurnalManager.instance.isEmasLockedInJurnal = true;
            
            if (isEmasWajib && inputZakatEmas != null)
            {
                string cleanEmas = inputZakatEmas.text.Replace(" gr", "").Replace(" ", "").Replace(",", ".");
                if (int.TryParse(cleanEmas, out int amountEmasToPay))
                    MoneyManager.instance.RemoveEmas(amountEmasToPay);
                else
                    MoneyManager.instance.RemoveEmas(Mathf.RoundToInt(MoneyManager.instance.totalEmas * 0.025f));
            }

            if (isPerakWajib && inputZakatPerak != null)
            {
                string cleanPerak = inputZakatPerak.text.Replace(" gr", "").Replace(" ", "").Replace(",", ".");
                if (int.TryParse(cleanPerak, out int amountPerakToPay))
                    MoneyManager.instance.RemovePerak(amountPerakToPay);
                else
                    MoneyManager.instance.RemovePerak(Mathf.RoundToInt(MoneyManager.instance.totalPerak * 0.025f));
            }

            if (panelReward != null) panelReward.SetActive(true); 
            if (audioSource != null && rewardBacksound != null) audioSource.PlayOneShot(rewardBacksound);
            if (panelFormKuis != null) panelFormKuis.SetActive(false); 
        }
        else
        {
            if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
            Debug.Log($"Jawaban Emas/Perak salah atau format tidak terbaca! Emas Valid: {emasValid}, Perak Valid: {perakValid}");
        }
    }

    void TutupRewardDanMatikanAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isEmasPerakCompleted = true;
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();
        }

        // --- KUNCI UTAMA SINKRONISASI HUD RUPIAH ---
        if (UIManager.instance != null)
        {
            // Paksa tutup panel reward menggunakan UIManager agar openedPanelsCount berkurang murni menjadi 0
            UIManager.instance.ClosePanelMenu(panelReward);
            
            // Pengaman ganda: Jika ada panel sisa yang menggantung, paksa reset ke 0 dan bangunkan HUD Gameplay
            System.Type.GetType("UIManager").GetField("openedPanelsCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(UIManager.instance, 0);
            
            GameObject gameplayHUDObj = (GameObject)System.Type.GetType("UIManager").GetField("gameplayHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(UIManager.instance);
            if (gameplayHUDObj != null) gameplayHUDObj.SetActive(true);
        }
        else
        {
            if (panelReward != null) panelReward.SetActive(false);
        }

        if (Level3Manager.instance != null)
        {
            Level3Manager.instance.TutupRewardDanMasukLevel3();
        }

        gameObject.SetActive(false);
    }
}