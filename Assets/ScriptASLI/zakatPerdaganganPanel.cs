using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZakatPerdaganganPanel : MonoBehaviour
{
    [Header("UI Elements Utama")]
    public Button btnClose;          
    public Button btnLanjutKuisBabak; // Untuk lanjut dari babak 1 -> 2 -> 3
    public Button btnLanjutFormUtama; // Untuk lanjut ke form kuis (muncul kalau nilai >= 7)
    public Button btnCloseReward;    
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

    [Header("Audio Settings")]
    public AudioSource audioSource; 
    public AudioClip correctSound; // Sound 1 tunggal untuk setiap klik  
    public AudioClip wrongSound;
    public AudioClip rewardBacksound; 
    public AudioClip clickSound;

    [Header("Panel Transition")]
    public GameObject panelKuisBG;
    public GameObject panelFormKuis;
    public GameObject panelZakatCarousel; 
    public GameObject panelRewardDagang;

    [Header("Panel Pembatas Level")]
    public GameObject panelBabLvl2; // Tarik panel bab-lvl2 kamu ke sini di Inspector
    public Button btnCloseBabLvl2;
    private int currentBabak = 1; 
    private int skorBenar = 0;
    private int jumlahJawabanDiBabakIni = 0;
    private HashSet<int> soalSudahDijawab = new HashSet<int>();

    void Start()
    {
        // Setup tombol dasar
        if (btnClose != null) { btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(TombolCloseBatalKuis); }
        if (btnTutupReward != null) { btnTutupReward.onClick.RemoveAllListeners(); btnTutupReward.onClick.AddListener(KlaimRewardDanClose); }
        if (btnCloseReward != null) { btnCloseReward.onClick.RemoveAllListeners(); btnCloseReward.onClick.AddListener(KlaimRewardDanClose); }
        
        // Setup tombol navigasi babak & form
        if (btnLanjutKuisBabak != null) { btnLanjutKuisBabak.onClick.RemoveAllListeners(); btnLanjutKuisBabak.onClick.AddListener(LanjutBabakBerikutnya); }
        if (btnLanjutFormUtama != null) { btnLanjutFormUtama.onClick.RemoveAllListeners(); btnLanjutFormUtama.onClick.AddListener(BukaFormKuis); }
        if (btnKalkulasiNilai != null) { btnKalkulasiNilai.onClick.RemoveAllListeners(); btnKalkulasiNilai.onClick.AddListener(KalkulasiNilaiAkhir); }
        if (btnUlangiKuis != null) { btnUlangiKuis.onClick.RemoveAllListeners(); btnUlangiKuis.onClick.AddListener(ResetDanUlangiKuis); }

        // 🔥 KITA DAFTARKAN SEMUA TOMBOL JAWABAN SECARA OTOMATIS DI SINI[cite: 4]
        // Parameter: (Tombolnya, Nomor Soal, Is Kunci Jawaban Benar)
        SetupJawaban(answerA1, 1, true);   SetupJawaban(answerB1, 1, false);  SetupJawaban(answerC1, 1, false);
        SetupJawaban(answerA2, 2, false);  SetupJawaban(answerB2, 2, true);   SetupJawaban(answerC2, 2, false);
        SetupJawaban(answerA3, 3, false);  SetupJawaban(answerB3, 3, true);   SetupJawaban(answerC3, 2, false);

        SetupJawaban(answerA4, 4, false);   SetupJawaban(answerB4, 4, true);  SetupJawaban(answerC4, 4, false);
        SetupJawaban(answerA5, 5, false);  SetupJawaban(answerB5, 5, false);   SetupJawaban(answerC5, 5, true);
        SetupJawaban(answerA6, 6, false);  SetupJawaban(answerB6, 6, false);  SetupJawaban(answerC6, 6, true);

        SetupJawaban(answerA7, 7, true);   SetupJawaban(answerB7, 7, false);  SetupJawaban(answerC7, 7, false);
        SetupJawaban(answerA8, 8, false);  SetupJawaban(answerB8, 8, true);   SetupJawaban(answerC8, 8, false);
        SetupJawaban(answerA9, 9, true);  SetupJawaban(answerB9, 9, false);  SetupJawaban(answerC9, 9, false);

        // Contoh Kunci Soal 10 (Ganti true/false nya sesuai modul kamu)
        SetupJawaban(answerA10, 10, false); SetupJawaban(answerB10, 10, true); SetupJawaban(answerC10, 10, false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        InisialisasiAwalKuis();
    }

    void SetupJawaban(Button btn, int nomorSoal, bool isCorrect)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => EksekusiKlikJawaban(btn, nomorSoal, isCorrect));
    }

    void EksekusiKlikJawaban(Button tombolDitekan, int nomorSoal, bool isCorrect)
    {
        // 1. KUNCI: Jika soal ini sudah pernah dijawab, blokir agar tidak bisa klik opsi lain
        if (soalSudahDijawab.Contains(nomorSoal)) return;

        // Ambil semua tombol yang ada di dalam satu kelompok soal (Parent)
        Transform soalParent = tombolDitekan.transform.parent;
        Button[] tombolPasangan = (soalParent != null) ? soalParent.GetComponentsInChildren<Button>(true) : new Button[0];

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

        // 3. Simpan status tracking soal
        soalSudahDijawab.Add(nomorSoal);
        jumlahJawabanDiBabakIni++;

        // Atur kemunculan tombol navigasi babak (Logika bawaan kamu tetap aman)
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

    // 🔥 FUNGSI PEMBANTU: Mewarnai jawaban yang benar dengan warna Hijau Sage saat pemain salah
    private void TandaiJawabanBenarDiUI(int nomorSoal)
    {
        Button tombolBenar = null;

        // Definisikan warna Hijau Sage (R: 135, G: 169, B: 135) -> Versi Normalized Float
        Color hijauSage = new Color(0.49f, 0.97f, 0.49f, 1f);

        switch (nomorSoal)
        {
            case 1: tombolBenar = answerA1; break; // Sesuai mapping SetupJawaban di Start kamu
            case 2: tombolBenar = answerB2; break; 
            case 3: tombolBenar = answerB3; break; 
            case 4: tombolBenar = answerB4; break; 
            case 5: tombolBenar = answerC5; break; 
            case 6: tombolBenar = answerC6; break; 
            case 7: tombolBenar = answerA7; break; 
            case 8: tombolBenar = answerB8; break; 
            case 9: tombolBenar = answerA9; break; 
            case 10: tombolBenar = answerB10; break; 
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

        // --- HITUNG ULANG SKOR BENAR DARI TOMBOL YANG BERWARNA HIJAU SAJA ---
        skorBenar = 0;

        // format: CheckSkorTombol(NamaVariabelTombol, ApakahIniKunciJawabanYangBenar);
        // Silakan sesuaikan posisi kata 'true' di bawah ini dengan kunci jawaban kuis asli dari modulmu!
        
        // Soal 1
        CheckSkorTombol(answerA1, true);   CheckSkorTombol(answerB1, false);  CheckSkorTombol(answerC1, false);
        // Soal 2
        CheckSkorTombol(answerA2, false);  CheckSkorTombol(answerB2, true);   CheckSkorTombol(answerC2, false);
        // Soal 3
        CheckSkorTombol(answerA3, false);  CheckSkorTombol(answerB3, true);  CheckSkorTombol(answerC3, false);
        // Soal 4
        CheckSkorTombol(answerA4, false);   CheckSkorTombol(answerB4, true);  CheckSkorTombol(answerC4, false);
        // Soal 5
        CheckSkorTombol(answerA5, false);  CheckSkorTombol(answerB5, false);   CheckSkorTombol(answerC5, true);
        // Soal 6
        CheckSkorTombol(answerA6, false);  CheckSkorTombol(answerB6, false);  CheckSkorTombol(answerC6, true);
        // Soal 7
        CheckSkorTombol(answerA7, true);   CheckSkorTombol(answerB7, false);  CheckSkorTombol(answerC7, false);
        // Soal 8
        CheckSkorTombol(answerA8, false);  CheckSkorTombol(answerB8, true);   CheckSkorTombol(answerC8, false);
        // Soal 9
        CheckSkorTombol(answerA9, true);  CheckSkorTombol(answerB9, false);  CheckSkorTombol(answerC9, false);
        // Soal 10
        CheckSkorTombol(answerA10, false); CheckSkorTombol(answerB10, true);  CheckSkorTombol(answerC10, false);

        // Update teks format nilai di UI (Contoh: 7/10)
        if (txtNilaiFormat != null)
        {
            txtNilaiFormat.text = skorBenar.ToString() + "/10";
        }

        // Pengkondisian teks apresiasi berdasarkan rentang nilai
        if (txtApresiasi != null)
        {
            if (skorBenar >= 1 && skorBenar <= 3)
            {
                txtApresiasi.text = "Jangan berkecil hati, mari pelajari kembali modul zakat perdagangan dan coba lagi!";
            }
            else if (skorBenar >= 4 && skorBenar <= 6)
            {
                txtApresiasi.text = "Cukup baik! Sedikit lagi kamu bisa memahami konsep zakat dengan sempurna.";
            }
            else if (skorBenar >= 7 && skorBenar <= 9)
            {
                txtApresiasi.text = "Luar biasa! Pemahamanmu mengenai zakat perdagangan sudah sangat matang.";
            }
            else if (skorBenar == 10)
            {
                txtApresiasi.text = "Sempurna! Kamu berhasil menjawab seluruh pertanyaan kuis dengan benar!";
            }
            else
            {
                txtApresiasi.text = "Silakan ulangi kuis untuk menguji pemahamanmu.";
            }
        }

        if (btnUlangiKuis != null)
            btnUlangiKuis.gameObject.SetActive(true);

        if (skorBenar >= 7)
        {
            if (btnLanjutFormUtama != null)
                btnLanjutFormUtama.gameObject.SetActive(true);

            if (JurnalManager.instance != null)
            {
                JurnalManager.instance.isDagangLockedInJurnal = true;
            }
        }
        else
        {
            if (btnLanjutFormUtama != null)
                btnLanjutFormUtama.gameObject.SetActive(false);
        }
    }

    // 🔥 JANGAN LUPA: Masukkan fungsi pembantu baru ini tepat di bawah fungsi KalkulasiNilaiAkhir() tadi!
    void CheckSkorTombol(Button btn, bool isCorrectKey)
    {
        if (btn != null && isCorrectKey)
        {
            // Jika tombol ini adalah kunci jawaban yang benar DAN warnanya saat ini hijau (sedang dipilih)
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

    void OnEnable()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(true);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);
        InisialisasiAwalKuis();
    }

    void BukaFormKuis()
    {   if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (panelKuisBG != null) panelKuisBG.SetActive(false); 
        if (panelFormKuis != null) panelFormKuis.SetActive(true); 
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);

        ZakatCalculator calc = GetComponentInChildren<ZakatCalculator>(true);
        if (calc != null) calc.SetupHartaRupiah();
    }

    public void MunculkanReward()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelZakatCarousel != null) panelZakatCarousel.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(true);
        if (audioSource != null && rewardBacksound != null) audioSource.PlayOneShot(rewardBacksound);
    }

    void KlaimRewardDanClose()
    {   
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (MoneyManager.instance != null) MoneyManager.instance.AddPerak(100);
        
        ZakatPanelManager panelManager = FindFirstObjectByType<ZakatPanelManager>();
        if (panelManager != null) { panelManager.isPerdaganganUnlocked = true; panelManager.UpdatePaymentButton(); panelManager.UpdateItemVisuals(); }
        
        // 🔥 PERBAIKAN MANUALLY ATTACHED BUTTON X
        if (panelBabLvl2 != null)
        {
            panelBabLvl2.SetActive(true);
            
            if (btnCloseBabLvl2 != null)
            {
                btnCloseBabLvl2.onClick.RemoveAllListeners();
                btnCloseBabLvl2.onClick.AddListener(() => {
                    // Ketika tombol X di panel bab 2 diklik:
                    panelBabLvl2.SetActive(false);
                    if (Level2Manager.instance != null) Level2Manager.instance.SwitchToLevel2();
                });
            }
            else
            {
                Debug.LogWarning("[Zakat Perdagangan] Kamu belum memasukkan 'Btn Close Bab Lvl 2' di Inspector!");
            }
        }
        else
        {
            if (Level2Manager.instance != null) Level2Manager.instance.SwitchToLevel2();
        }
        
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);
        gameObject.SetActive(false);
    }

    public void TombolCloseBatalKuis()
    {
        // 🔥 TAMBAHAN: Hapus semua jawaban sebelumnya dan kembalikan warna ke putih semula
        ResetDanUlangiKuis();

        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelRewardDagang != null) panelRewardDagang.SetActive(false);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);

        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
        else gameObject.SetActive(false);
    }

    public void PaksaTutupBukuZakatCarousel() { if (ZakatPanelManager.instance != null) ZakatPanelManager.instance.CloseZakatPanel(); }
    public void MatikanObjectIniLangsung() { gameObject.SetActive(false); }
}