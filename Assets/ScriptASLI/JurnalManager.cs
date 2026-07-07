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

    [Header("Audio Settings - Tambahan")]
    public AudioSource audioSourceJurnal; 
    public AudioClip suaraBukaJurnal;     
    public AudioClip suaraTutupJurnal;

    private int currentPage = 1; 

    [Header("Halaman 1: Peta Jurnal")]
    public GameObject groupPetaJurnal;

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
    public GameObject groupZakatEmasPerak; 
    public GameObject visualHalamanLock;         
    public GameObject visualHalamanUnlock;       
    public GameObject navCoinLeftPanel;
    public GameObject checkNisabEmasPerak;
    public GameObject checkHaulEmasPerak;
    public GameObject checkZakatEmasPerak;
    public TextMeshProUGUI txtHartaEmas;         
    public TextMeshProUGUI txtHartaPerak;        
    public TextMeshProUGUI txtEmasPerakStatus;   
    public GameObject messageTextEmasPerak;      
    public Slider haulSliderEmasPerak;

    [Header("Halaman 3: Zakat Ternak (Updated)")]
    public GameObject groupZakatTernak;          
    public GameObject panelLockTernak;           
    public GameObject panelUnlockTernak;         
    public GameObject checkNisabTernak;          
    public GameObject checkHaulTernak;           
    public GameObject checkZakatTernak;          
    public TextMeshProUGUI txtHartaSapi;        
    public TextMeshProUGUI txtHartaKambing;     
    public TextMeshProUGUI txtTernakStatus;      
    public GameObject messageTextTernak;         
    public Slider haulSliderTernak;              

    [Header("Settings")]
    public float nisabLimit = 5000f; 
    public float timerPerMonth = 5f; 
    public ZakatPanelManager zakatManager;
    
    public float nisabEmasKriteria = 94f;        
    public float nisabPerakKriteria = 624f;      
    
    public int nisabSapiKriteria = 30;         
    public int nisabKambingKriteria = 40; 

    // 🔥 KONTROL INSPECTOR KHUSUS HAUL & ANAK TERNAK
    [Space(10)]
    [Header("--- Pengaturan Waktu Slider ---")]
    [Tooltip("Waktu detik per bulan untuk jalannya Slider Haul (Sapi & Kambing)")]
    public float timerPerMonthTernak = 3f; 

    [Header("--- Pengaturan Kelahiran Kambing ---")]
    public float intervalBeranakKambing = 7f;

    [Tooltip("Jumlah ekor Kambing yang bertambah setiap kali melahirkan")]
    public int jumlahTambahanKambing = 15; // Sudah otomatis bernilai 15 sesuai keinginanmu!

    [Header("Lock Status After Reward")]
    public bool isDagangLockedInJurnal = false;
    public bool isTernakLockedInJurnal = false;
    public bool isEmasLockedInJurnal = false;

    private int lockedDagangValue = 0;
    private int lockedSapiValue = 0;
    private int lockedKambingValue = 0;
    private int lockedEmasValue = 0;
    private int lockedPerakValue = 0;      

    private int totalEkorSapiInternal = 0;
    private int totalEkorKambingInternal = 0;
    private int trackerJumlahSapiToko = 0;
    private int trackerJumlahKambingToko = 0;
    private bool isSistemSapiBeranakAktif = false;
    private bool isSistemKambingBeranakAktif = false;

    [HideInInspector] public int currentHaulMonth = 0;
    [HideInInspector] public bool isNisabReached = false;
    [HideInInspector] public bool isHaulComplete = false;
    [HideInInspector] public bool isZakatPaid = false; // 🔥 TAMBAHKAN INI yang kurang
    [HideInInspector] public bool isNotificationShown = false;
    [HideInInspector] public bool isDagangCoroutineRunning = false;

    [HideInInspector] public int currentHaulMonthEmasPerak = 0;
    [HideInInspector] public bool isEmasPerakNisabReached = false;
    [HideInInspector] public bool isEmasPerakHaulComplete = false;
    [HideInInspector] public bool isEmasPerakZakatPaid = false; // 🔥 TAMBAHKAN INI yang kurang
    [HideInInspector] public bool isEmasPerakUnlocked = false;
    [HideInInspector] public bool isEmasPerakNotificationShown = false;
    [HideInInspector] public bool isEmasPerakCoroutineRunning = false;

    // State Logic untuk Halaman 3 (Zakat Ternak)
    [HideInInspector] public int currentHaulMonthTernak = 0;
    [HideInInspector] public bool isTernakNisabReached = false;
    [HideInInspector] public bool isTernakHaulComplete = false;
    [HideInInspector] public bool isTernakZakatPaid = false; // 🔥 TAMBAHKAN INI yang kurang
    [HideInInspector] public bool isTernakUnlocked = false;
    [HideInInspector] public bool isTernakNotificationShown = false;
    [HideInInspector] public bool isTernakCoroutineRunning = false;
    private Coroutine coSapiBeranak = null;
    private Coroutine coKambingBeranak = null;

    void Awake() { instance = this; }

    void Start()
    {
        jurnalContent.SetActive(false);
        asetBlur.SetActive(false);
        if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
        
        if (visualHalamanLock != null) visualHalamanLock.SetActive(true);
        if (visualHalamanUnlock != null) visualHalamanUnlock.SetActive(false);
        
        if (panelLockTernak != null) panelLockTernak.SetActive(true);
        if (panelUnlockTernak != null) panelUnlockTernak.SetActive(false);
        
        currentPage = 1;
        ShowPage(currentPage);
        
        checkNisab.SetActive(false);
        checkHaul.SetActive(false);
        if(checkZakatDagang != null) checkZakatDagang.SetActive(false);
        
        haulSlider.minValue = 0;
        haulSlider.maxValue = 12;
        haulSlider.value = 0;
        messageText.SetActive(false);
        
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

        if (checkNisabTernak != null) checkNisabTernak.SetActive(false);
        if (checkHaulTernak != null) checkHaulTernak.SetActive(false);
        if (checkZakatTernak != null) checkZakatTernak.SetActive(false);
        if (messageTextTernak != null) messageTextTernak.SetActive(false);
        
        if (haulSliderTernak != null)
        {
            haulSliderTernak.minValue = 0;
            haulSliderTernak.maxValue = 12;
            haulSliderTernak.value = 0;
        }
        
        UpdateStatusUI();
        UpdateEmasPerakStatusUI();
        UpdateTernakStatusUI();

        btnClose.onClick.AddListener(CloseJurnal);
        btnNext.onClick.AddListener(NextPage);
        btnPrevious.onClick.AddListener(PreviousPage);
    }

    void Update()
    {
        if (MoneyManager.instance != null)
        {
            float currentMoney = MoneyManager.instance.totalMoney;

            if (!isDagangLockedInJurnal)
            {
                txtHartamu.text = "Rp " + currentMoney.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                lockedDagangValue = (int)currentMoney; 
            }
            else
            {
                txtHartamu.text = "Rp " + lockedDagangValue.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
            }

            if (currentMoney >= nisabLimit && !isNisabReached)
            {
                StartZakatLogic();
            }

            if (!isEmasLockedInJurnal)
            {
                int emasSekarang = (MoneyManager.instance != null) ? MoneyManager.instance.totalEmas : 0;
                int perakSekarang = (MoneyManager.instance != null) ? MoneyManager.instance.totalPerak : 0;
                
                txtHartaEmas.text = $"{emasSekarang} Gram";
                txtHartaPerak.text = $"{perakSekarang} Gram";
                
                lockedEmasValue = emasSekarang;
                lockedPerakValue = perakSekarang;
            }
            else
            {
                txtHartaEmas.text = $"{lockedEmasValue} Gram";
                txtHartaPerak.text = $"{lockedPerakValue} Gram";
            }

            CheckEmasPerakNisab();
        }
        CheckLevel3TernakUnlock();
        if (Level3Manager.instance != null && Level3Manager.instance.isBabak3Aktif)
        {
            CekPembelianTokoTernak();
        }

        if (!isTernakLockedInJurnal)
        {
            txtHartaSapi.text = $"{totalEkorSapiInternal} Ekor";
            txtHartaKambing.text = $"{totalEkorKambingInternal} Ekor";
            
            lockedSapiValue = totalEkorSapiInternal;
            lockedKambingValue = totalEkorKambingInternal;
        }
        else
        {
            txtHartaSapi.text = $"{lockedSapiValue} Ekor";
            txtHartaKambing.text = $"{lockedKambingValue} Ekor";
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
        
        if (!isDagangCoroutineRunning)
        {
            StartCoroutine(HaulTimerRoutine());
        }
    }

    IEnumerator HaulTimerRoutine() {
        isDagangCoroutineRunning = true; 
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
        
        isDagangCoroutineRunning = false; 
    }

    IEnumerator HaulTimerEmasPerakRoutine() {
        isEmasPerakCoroutineRunning = true; 
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
        
        isEmasPerakCoroutineRunning = false; 
    }

    IEnumerator HaulTimerTernakRoutine() {
        isTernakCoroutineRunning = true;
        currentHaulMonthTernak = 0;
        isTernakHaulComplete = false;

        while (currentHaulMonthTernak < 12) {
            float timer = 0;
            while (timer < timerPerMonthTernak) { 
                timer += Time.deltaTime;
                if (haulSliderTernak != null) {
                    haulSliderTernak.value = currentHaulMonthTernak + (timer / timerPerMonthTernak);
                }
                yield return null;
            }
            currentHaulMonthTernak++;
            haulSliderTernak.value = currentHaulMonthTernak;
        }

        isTernakHaulComplete = true;
        if (checkHaulTernak != null) checkHaulTernak.SetActive(true);
        UpdateTernakStatusUI();

        isTernakCoroutineRunning = false;
    }

    public void NextPage()
    {
        if (currentPage < 4) // Berubah dari < 3 menjadi < 4
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    public void ShowPage(int pageNumber)
    {
        if (groupPetaJurnal != null) groupPetaJurnal.SetActive(false);
        groupZakatPerdagangan.SetActive(false);
        groupZakatEmasPerak.SetActive(false);
        groupZakatTernak.SetActive(false);

        if (pageNumber == 1)
        {
            if (groupPetaJurnal != null) groupPetaJurnal.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(false); // Halaman pertama tidak bisa mundur
        }
        else if (pageNumber == 2)
        {
            groupZakatPerdagangan.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(true);
        }
        else if (pageNumber == 3)
        {
            groupZakatEmasPerak.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(true);
        }
        else if (pageNumber == 4)
        {
            groupZakatTernak.SetActive(true);
            btnNext.gameObject.SetActive(false); // Halaman terakhir tidak bisa maju lagi
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
                zakatManager.isPerdaganganUnlocked = false;
                zakatManager.UpdateItemVisuals();
            }
        }
    }

    void UpdateEmasPerakStatusUI()
    {
        if (isEmasPerakNisabReached && isEmasPerakHaulComplete)
        {
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
                zakatManager.isEmasPerakUnlocked = true; 
                zakatManager.UpdateItemVisuals();
            }
            if (!isEmasPerakNotificationShown && !jurnalContent.activeSelf) {
                if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(true);
                isEmasPerakNotificationShown = true; 
            }

            // 🌟 HUBUNGKAN KE TASK MANAGER BABAK 2:
            // Jika pemain sudah selesai menambang 15 kali, langsung munculkan tombol ke Kantor Zakat
            if (TaskManager.instance != null)
            {
                TaskManager.instance.CekPemicuZakatEmasPerak();
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

    void UpdateTernakStatusUI()
    {
        if (isTernakNisabReached && isTernakHaulComplete)
        {
            txtTernakStatus.text = "Wajib Zakat Hewan Ternak";
            txtTernakStatus.color = Color.black;
            
            if (messageTextTernak != null) messageTextTernak.SetActive(true);

            if (zakatManager != null)
            {
                zakatManager.isPeternakanUnlocked = true; 
                zakatManager.UpdateItemVisuals();
            }

            if (!isTernakNotificationShown && !jurnalContent.activeSelf)
            {
                if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(true);
                isTernakNotificationShown = true;
            }

            // 🌟 HUBUNGKAN KE TASK MANAGER BABAK 3:
            // Jika pemain sudah selesai memberi pakan, langsung munculkan tombol ke Kantor Zakat
            if (TaskManager.instance != null)
            {
                TaskManager.instance.CekPemicuZakatTernak();
            }
        }
        else
        {
            txtTernakStatus.text = "Belum Wajib Zakat";
            txtTernakStatus.color = Color.gray;
            if (messageTextTernak != null) messageTextTernak.SetActive(false);

            if (zakatManager != null)
            {
                zakatManager.isPeternakanUnlocked = false;
                zakatManager.UpdateItemVisuals();
            }
        }
    }

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

            if (!isEmasPerakNisabReached)
            {
                isEmasPerakNisabReached = true;
                if (checkNisabEmasPerak != null) checkNisabEmasPerak.SetActive(true);
                if (checkZakatEmasPerak != null) checkZakatEmasPerak.SetActive(true);
                
                if (ikonNotifikasiJurnal != null && !jurnalContent.activeSelf) 
                {
                    ikonNotifikasiJurnal.SetActive(true);
                }

                if (!isEmasPerakCoroutineRunning)
                {
                    StartCoroutine(HaulTimerEmasPerakRoutine());
                }
            }
        } 
    }

    private void CheckLevel3TernakUnlock()
    {
        bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;

        if (sudahLevel3)
        {
            if (panelLockTernak != null && panelLockTernak.activeSelf) panelLockTernak.SetActive(false);
            if (panelUnlockTernak != null && !panelUnlockTernak.activeSelf) panelUnlockTernak.SetActive(true);

            if (!isTernakUnlocked)
            {
                isTernakUnlocked = true;
            }

            int sapiSekarang = GetJumlahSapiRealTime();
            int kambingSekarang = GetJumlahKambingRealTime();

            if (!isTernakLockedInJurnal)
            {
                if (txtHartaSapi != null) txtHartaSapi.text = sapiSekarang.ToString() + " Ekor";
                if (txtHartaKambing != null) txtHartaKambing.text = kambingSekarang.ToString() + " Ekor";
                
                lockedSapiValue = sapiSekarang;
                lockedKambingValue = kambingSekarang;
            }
            else
            {
                if (txtHartaSapi != null) txtHartaSapi.text = lockedSapiValue.ToString() + " Ekor";
                if (txtHartaKambing != null) txtHartaKambing.text = lockedKambingValue.ToString() + " Ekor";
            }

            if (isTernakNisabReached)
            {
                if (!isTernakCoroutineRunning && !isTernakHaulComplete)
                {
                    StartCoroutine(HaulTimerTernakRoutine());
                }
            }

            if (sapiSekarang >= nisabSapiKriteria || kambingSekarang >= nisabKambingKriteria)
            {
                if (!isTernakNisabReached)
                {
                    isTernakNisabReached = true;
                    if (checkNisabTernak != null) checkNisabTernak.SetActive(true);
                    if (checkZakatTernak != null) checkZakatTernak.SetActive(true);

                    if (ikonNotifikasiJurnal != null && !jurnalContent.activeSelf)
                    {
                        ikonNotifikasiJurnal.SetActive(true);
                    }
                }
                UpdateTernakStatusUI();
            }
        }
    }

    // 🔥 PERBAIKAN: Kode pememicu coroutine dipindah ke atas sebelum baris 'return' agar tidak Unreachable
    public int GetJumlahSapiRealTime()
    {
        // 🔥 PERBAIKAN: Jika sedang dikunci (buka form kuis), JANGAN nyalakan coroutine lagi!
        if (!isSistemSapiBeranakAktif && totalEkorSapiInternal > 0 && !isTernakLockedInJurnal) 
        {
            coSapiBeranak = StartCoroutine(SistemSapiBeranakRoutine());
        }
        return totalEkorSapiInternal; 
    }

    public int GetJumlahKambingRealTime()
    {
        // 🔥 PERBAIKAN: Jika sedang dikunci (buka form kuis), JANGAN nyalakan coroutine lagi!
        if (!isSistemKambingBeranakAktif && totalEkorKambingInternal > 0 && !isTernakLockedInJurnal) 
        {
            coKambingBeranak = StartCoroutine(SistemKambingBeranakRoutine());
        }
        return totalEkorKambingInternal; 
    }

    private void CekPembelianTokoTernak()
    {
        if (TokoManager.instance != null)
        {
            // Ambil data jumlah sapi yang dibeli dari TokoManager
            int sapiDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahSapiDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            if (sapiDariToko > trackerJumlahSapiToko)
            {
                int selisihBeli = sapiDariToko - trackerJumlahSapiToko;
                
                // 🔄 UBAH DI SINI: Ganti dari 10 menjadi 5
                totalEkorSapiInternal += (selisihBeli * 5); 
                
                trackerJumlahSapiToko = sapiDariToko;

                if (!isSistemSapiBeranakAktif) coSapiBeranak = StartCoroutine(SistemSapiBeranakRoutine());
            }

            // Ambil data jumlah kambing yang dibeli dari TokoManager
            int kambingDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahKambingDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            if (kambingDariToko > trackerJumlahKambingToko)
            {
                int selisihBeli = kambingDariToko - trackerJumlahKambingToko;
                
                // 🔄 UBAH DI SINI: Ganti dari 10 menjadi 5
                totalEkorKambingInternal += (selisihBeli * 5); 
                
                trackerJumlahKambingToko = kambingDariToko;

                if (!isSistemKambingBeranakAktif) coKambingBeranak = StartCoroutine(SistemKambingBeranakRoutine());
            }
        }
    }

    IEnumerator SistemSapiBeranakRoutine()
    {
        isSistemSapiBeranakAktif = true;

        while (true)
        {
            yield return new WaitForSeconds(5f); // Jeda pengecekan tetap jalan

            // 🛑 FILTER BARU: Tunda pertambahan jika pakan belum diisi/diklaim di TaskManager!
            if (TaskManager.instance != null && !TaskManager.instance.isIsiPakanDone)
            {
                // Jika belum selesai mengisi pakan, skip/lewati bagian kode penambahan di bawahnya
                continue; 
            }

            if (totalEkorSapiInternal > 0)
            {
                // 🔥 STRATEGI BARU: Jika angka ganjil/tanggung (misal 5, 15, 25), bulatkan dulu ke kelipatan 10 di atasnya
                if (totalEkorSapiInternal % 10 != 0)
                {
                    // Contoh: 25 -> 25 + (10 - 5) = 30
                    totalEkorSapiInternal += (10 - (totalEkorSapiInternal % 10));
                }
                else
                {
                    // Jika sudah kelipatan 10, naik normal +10
                    totalEkorSapiInternal += 10;
                }

                // Batasi maksimal sesuai batas awal script-mu
                if (totalEkorSapiInternal > 200) totalEkorSapiInternal = 200;
                
                Debug.Log($"<color=white>[Jurnal Ternak]</color> Sapi melahirkan (Kelipatan 10)! Jumlah sekarang: {totalEkorSapiInternal} ekor.");

                // 📞 HUBUNGKAN KE VISUAL LAPANGAN
                if (TokoManager.instance != null)
                {
                    TokoManager.instance.UpdateVisualHewanBerdasarkanJumlah(totalEkorSapiInternal, totalEkorKambingInternal);
                }
            }
        }
    }

    IEnumerator SistemKambingBeranakRoutine()
    {
        isSistemKambingBeranakAktif = true;

        while (true)
        {
            yield return new WaitForSeconds(intervalBeranakKambing); //[cite: 5]

            // 🛑 FILTER BARU: Tunda jika pakan belum diisi/diklaim di TaskManager!
            if (TaskManager.instance != null && !TaskManager.instance.isIsiPakanDone)
            {
                continue; 
            }

            if (totalEkorKambingInternal > 0)
            {
                totalEkorKambingInternal += jumlahTambahanKambing; //[cite: 5]
                
                Debug.Log($"<color=orange>[Jurnal Ternak]</color> Kambing melahirkan! Jumlah sekarang: {totalEkorKambingInternal} ekor.");

                // 📞 HUBUNGKAN KE VISUAL LAPANGAN: Panggil fungsi visual yang kita buat di TokoManager tadi
                if (TokoManager.instance != null)
                {
                    TokoManager.instance.UpdateVisualHewanBerdasarkanJumlah(totalEkorSapiInternal, totalEkorKambingInternal);
                }
            }
        }
    }

    public void OpenJurnal() 
{ 
    if (audioSourceJurnal != null && suaraBukaJurnal != null) {
        audioSourceJurnal.PlayOneShot(suaraBukaJurnal);
    }

    if (UIManager.instance != null)
    {
        UIManager.instance.OpenPanelMenu(jurnalContent);
    }
    else
    {
        jurnalContent.SetActive(true);
    }

    // 🔥 Force apply blur mandiri milik jurnal dan taruh di paling belakang
    if (asetBlur != null) {
        asetBlur.SetActive(true);
        asetBlur.transform.SetAsFirstSibling(); // urutan paling atas di hierarki = visual paling belakang
    }
    
    // Pastikan halaman buku jurnal didorong ke depan
    if (jurnalContent != null) jurnalContent.transform.SetAsLastSibling();

    if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
}

public void CloseJurnal()
{
    if (audioSourceJurnal != null && suaraTutupJurnal != null) {
        audioSourceJurnal.PlayOneShot(suaraTutupJurnal);
    }

    if (UIManager.instance != null)
    {
        UIManager.instance.ClosePanelMenu(jurnalContent);
    }
    else
    {
        jurnalContent.SetActive(false);
    }

    // 🔥 Matikan blur mandiri milik jurnal
    if (asetBlur != null) asetBlur.SetActive(false);
}
    
    public bool IsPerdaganganUnlocked()
    {
        return isNisabReached && isHaulComplete;
    }

    public bool IsEmasPerakUnlocked()
    {
        return isEmasPerakNisabReached && isEmasPerakHaulComplete; 
    }

    public bool IsPeternakanUnlocked()
    {
        return isTernakNisabReached && isTernakHaulComplete; 
    }

    public void MatikanSistemBeranak()
    {
        if (coSapiBeranak != null)
        {
            StopCoroutine(coSapiBeranak);
            coSapiBeranak = null;
        }
        if (coKambingBeranak != null)
        {
            StopCoroutine(coKambingBeranak);
            coKambingBeranak = null;
        }
        
        isSistemSapiBeranakAktif = false;
        isSistemKambingBeranakAktif = false;
        Debug.Log("<color=red>[Jurnal Ternak]</color> Coroutine beranak dihentikan secara permanen.");
    }
}