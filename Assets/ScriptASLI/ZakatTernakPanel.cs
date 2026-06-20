using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZakatTernakPanel : MonoBehaviour
{
    [Header("UI Elements Utama")]
    public Button btnClose;          // Tombol silang 'X' untuk batal/tutup kuis biasa
    public Button btnLanjutKuisBabak; // Untuk navigasi lanjut babak kuis (Babak 1 -> 2 -> 3)
    public Button btnLanjutFormUtama; // Untuk beralih ke form kuis kalkulator formulir (muncul kalau nilai >= 7)

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
    public GameObject panelFormKuis;        // Objek 'form-kuis'[cite: 9]
    public TMP_Text txtHartakuTernak;       // Teks harta ternak[cite: 9]
    public TMP_Text txtDeskripsiZakat;      // Teks deskripsi[cite: 9]

    [Header("Dropdown Input Fields")]
    public GameObject containerDropdownSapi;     // Objek 'drodown-sapi'[cite: 9]
    public GameObject containerDropdownKambing;  // Objek 'drodown-kambing'[cite: 9]
    public TMP_Dropdown dropdownSapi;            // Komponen SapiDropdown[cite: 9]
    public TMP_Dropdown dropdownKambing;         // Komponen KambingDropdown[cite: 9]

    [Header("Action Buttons")]
    public Button btnSelesaiKuis;           // Tombol 'Selesai' di formulir[cite: 9]

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSound;          // Sound 1 tunggal untuk setiap klik
    public AudioClip wrongSound;
    public AudioClip rewardBacksound;       // Slot backsound reward koin ternak[cite: 9]
    public AudioClip clickSound;

    [Header("Reward Panel")]
    public GameObject panelReward;
    public Button btnTutupReward;           // Tombol OK/Klaim/Tutup di panel reward[cite: 9]
    public GameObject panelEndingGame;
    public GameObject panelKuisBG;          // Objek 'kuis-bg'[cite: 9]

    [HideInInspector] public bool isSapiWajib = false;
    [HideInInspector] public bool isKambingWajib = false;
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
            btnSelesaiKuis.onClick.AddListener(ValidateZakatTernak);
        }

        if (btnTutupReward != null)
        {
            btnTutupReward.onClick.RemoveAllListeners();
            btnTutupReward.onClick.AddListener(KlaimRewardDanTutup);
        }

        // DAFTARKAN SEMUA TOMBOL JAWABAN OTOMATIS
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
        if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
        bool sudahPernahDijawab = soalSudahDijawab.Contains(nomorSoal);

        Transform soalParent = tombolDitekan.transform.parent;
        if (soalParent != null)
        {
            Button[] tombolPasangan = soalParent.GetComponentsInChildren<Button>(true);
            foreach (Button b in tombolPasangan)
            {
                if (b != null) b.GetComponent<Image>().color = Color.white;
            }
        }

        tombolDitekan.GetComponent<Image>().color = Color.green;
        if (!sudahPernahDijawab)
        {
            soalSudahDijawab.Add(nomorSoal);
            jumlahJawabanDiBabakIni++;
        }

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

        // 🔥 KUNCI JAWABAN KUIS ZAKAT TERNAK
        // format: CheckSkorTombol(NamaVariabelTombol, ApakahIniKunciJawabanYangBenar);
        // Silakan sesuaikan posisi kata 'true' di bawah ini dengan kunci jawaban kuis materi Ternak milikmu!
        
        // Soal 1
        CheckSkorTombol(answerA1, false);   CheckSkorTombol(answerB1, true);  CheckSkorTombol(answerC1, false);
        // Soal 2
        CheckSkorTombol(answerA2, false);  CheckSkorTombol(answerB2, true);   CheckSkorTombol(answerC2, false);
        // Soal 3
        CheckSkorTombol(answerA3, true);  CheckSkorTombol(answerB3, false);  CheckSkorTombol(answerC3, false);
        // Soal 4
        CheckSkorTombol(answerA4, false);   CheckSkorTombol(answerB4, true);  CheckSkorTombol(answerC4, false);
        // Soal 5
        CheckSkorTombol(answerA5, false);  CheckSkorTombol(answerB5, true);   CheckSkorTombol(answerC5, false);
        // Soal 6
        CheckSkorTombol(answerA6, false);  CheckSkorTombol(answerB6, true);  CheckSkorTombol(answerC6, false);
        // Soal 7
        CheckSkorTombol(answerA7, false);   CheckSkorTombol(answerB7, true);  CheckSkorTombol(answerC7, false);
        // Soal 8
        CheckSkorTombol(answerA8, false);  CheckSkorTombol(answerB8, false);   CheckSkorTombol(answerC8, true);
        // Soal 9
        CheckSkorTombol(answerA9, false);  CheckSkorTombol(answerB9, true);  CheckSkorTombol(answerC9, false);
        // Soal 10
        CheckSkorTombol(answerA10, false); CheckSkorTombol(answerB10, false);  CheckSkorTombol(answerC10, true);

        if (txtNilaiFormat != null)
        {
            txtNilaiFormat.text = skorBenar.ToString() + "/10";
        }

        if (txtApresiasi != null)
        {
            if (skorBenar >= 1 && skorBenar <= 3) txtApresiasi.text = "Jangan berkecil hati, mari pelajari kembali modul zakat hewan ternak dan coba lagi!";
            else if (skorBenar >= 4 && skorBenar <= 6) txtApresiasi.text = "Cukup baik! Sedikit lagi kamu bisa memahami konsep zakat hewan ternak dengan sempurna.";
            else if (skorBenar >= 7 && skorBenar <= 9) txtApresiasi.text = "Luar biasa! Pemahamanmu mengenai zakat hewan ternak sudah sangat matang.";
            else if (skorBenar == 10) txtApresiasi.text = "Sempurna! Kamu berhasil menjawab seluruh pertanyaan kuis dengan benar!";
        }

        if (btnUlangiKuis != null) btnUlangiKuis.gameObject.SetActive(true);

        if (skorBenar >= 7)
        {
            if (btnLanjutFormUtama != null) btnLanjutFormUtama.gameObject.SetActive(true);
            if (JurnalManager.instance != null) JurnalManager.instance.isTernakLockedInJurnal = true;
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

        // 🔥 KARENA BATAL, pancing kembali agar JurnalManager menghitung ulang/menyalakan coroutine beranak
        if (JurnalManager.instance != null)
        {
            JurnalManager.instance.isTernakLockedInJurnal = false;
            JurnalManager.instance.GetJumlahSapiRealTime();
            JurnalManager.instance.GetJumlahKambingRealTime();
        }

        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
        else gameObject.SetActive(false);
    }

    void BukaFormKuis()
    {   
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(true);
        if (panelHasilKuis != null) panelHasilKuis.SetActive(false);

        if (JurnalManager.instance != null)
        {
            JurnalManager.instance.isTernakLockedInJurnal = true;
            // 🔥 STOP TOTAL sistem beranak saat form kuis sedang dibuka/dikerjakan
            JurnalManager.instance.MatikanSistemBeranak(); 
        }

        ConfigureFormDinamis();
    }

    // --- TETAP MENJAGA LOGIKA FORM KALKULATOR TERNAK & DROP-DOWN BAWAAN (SETTLE) ---
    public void ConfigureFormDinamis()
    {
        if (JurnalManager.instance == null) return;

        int sapiSekarang = JurnalManager.instance.GetJumlahSapiRealTime();
        int kambingSekarang = JurnalManager.instance.GetJumlahKambingRealTime();

        isSapiWajib = sapiSekarang >= JurnalManager.instance.nisabSapiKriteria;
        isKambingWajib = kambingSekarang >= JurnalManager.instance.nisabKambingKriteria;

        if (isSapiWajib && isKambingWajib)
        {
            txtHartakuTernak.text = $"Sapi : {sapiSekarang} Ekor\nKambing : {kambingSekarang} Ekor";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }
        else if (isSapiWajib)
        {
            txtHartakuTernak.text = $"Sapi : {sapiSekarang} Ekor";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }
        else if (isKambingWajib)
        {
            txtHartakuTernak.text = $"Kambing : {kambingSekarang} Ekor";
            if (txtDeskripsiZakat != null) txtDeskripsiZakat.text = "sesuai dengan ketentuan berlaku, sejumlah:";
        }

        if (containerDropdownSapi != null) containerDropdownSapi.SetActive(isSapiWajib);
        if (containerDropdownKambing != null) containerDropdownKambing.SetActive(isKambingWajib);

        if (dropdownSapi != null) dropdownSapi.value = 0;
        if (dropdownKambing != null) dropdownKambing.value = 0;
    }

    public void ValidateZakatTernak()
    {   if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
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
            else if (sapiSekarang >= 90 && sapiSekarang < 100)  sapiValid = (idx == 6);
            else if (sapiSekarang >= 100 && sapiSekarang < 110) sapiValid = (idx == 8);
            else if (sapiSekarang >= 110 && sapiSekarang < 120) sapiValid = (idx == 7);
            else if (sapiSekarang >= 120 && sapiSekarang < 130) sapiValid = (idx == 10 || idx == 11);
            else if (sapiSekarang >= 130 && sapiSekarang < 140) sapiValid = (idx == 10);
            else if (sapiSekarang >= 140 && sapiSekarang < 150) sapiValid = (idx == 11 || idx == 12);
            else if (sapiSekarang >= 150 && sapiSekarang < 160) sapiValid = (idx == 13);
            else if (sapiSekarang >= 160 && sapiSekarang < 170) sapiValid = (idx == 14);
            else if (sapiSekarang >= 170 && sapiSekarang < 180) sapiValid = (idx == 15);
            else if (sapiSekarang >= 180 && sapiSekarang < 200) sapiValid = (idx == 16);
            else if (sapiSekarang >= 200)                       sapiValid = (idx == 17);
        }
        else if (!isSapiWajib)
        {
            sapiValid = true;
        }

        if (isKambingWajib && dropdownKambing != null)
        {
            int idxKambing = dropdownKambing.value;

            if (kambingSekarang > 1000)
            {
                kambingSekarang = 1000;
            }

            if (kambingSekarang >= 40 && kambingSekarang <= 120)
            {
                kambingValid = (idxKambing == 1);
            }
            else if (kambingSekarang >= 121 && kambingSekarang <= 200)
            {
                kambingValid = (idxKambing == 2);
            }
            else if (kambingSekarang >= 201 && kambingSekarang <= 300)
            {
                kambingValid = (idxKambing == 3);
            }
            else if (kambingSekarang > 300 && kambingSekarang <= 1000)
            {
                int hitunganZakat = 3 + ((kambingSekarang - 300) / 100);
                kambingValid = (idxKambing == hitunganZakat);
            }
        }
        else if (!isKambingWajib)
        {
            kambingValid = true;
        }

        if (sapiValid && kambingValid)
        {
            if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);

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
                else if (sapiSekarang >= 100) pengurangSapi = 100;
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
        }
    }

    void KlaimRewardDanTutup()
    {   
        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        
        // 🔥 MATIKAN PERMANEN karena sudah berhasil menjawab form kuis dengan benar
        if (JurnalManager.instance != null) 
        {
            JurnalManager.instance.MatikanSistemBeranak();
        }
        
        if (MoneyManager.instance != null) MoneyManager.instance.AddMoney(100000);

        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isPeternakanCompleted = true;
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();
            ZakatPanelManager.instance.CloseZakatPanel();
        }

        if (panelReward != null) panelReward.SetActive(false);
        
        if (EndingManager.instance != null)
        {
            EndingManager.instance.MulaiSequenceEnding();
        }

        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
        else gameObject.SetActive(false);
    }
}