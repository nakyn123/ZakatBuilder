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

    private int currentPage = 1; 

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
    [Tooltip("Waktu detik per bulan KHUSUS untuk Haul Ternak Halaman 3")]
    public float timerPerMonthTernak = 3f; 

    [Tooltip("Interval waktu (detik) untuk penambahan otomatis/beranak")]
    public float intervalBeranakTernak = 10f;

    [Tooltip("Jumlah ekor yang bertambah setiap kali interval waktu habis")]
    public int jumlahTambahanPerInterval = 2;        

    private int totalEkorSapiInternal = 0;
    private int totalEkorKambingInternal = 0;
    private int trackerJumlahSapiToko = 0;
    private int trackerJumlahKambingToko = 0;
    private bool isSistemSapiBeranakAktif = false;
    private bool isSistemKambingBeranakAktif = false;

    private int currentHaulMonth = 0;
    private bool isNisabReached = false;
    private bool isHaulComplete = false;
    private bool isNotificationShown = false;
    private bool isDagangCoroutineRunning = false;

    private int currentHaulMonthEmasPerak = 0;
    private bool isEmasPerakNisabReached = false;
    private bool isEmasPerakHaulComplete = false;
    private bool isEmasPerakUnlocked = false;
    private bool isEmasPerakNotificationShown = false;
    private bool isEmasPerakCoroutineRunning = false;

    // State Logic untuk Halaman 3 (Zakat Ternak)
    private int currentHaulMonthTernak = 0;
    private bool isTernakNisabReached = false;
    private bool isTernakHaulComplete = false;
    private bool isTernakUnlocked = false;
    private bool isTernakNotificationShown = false;
    private bool isTernakCoroutineRunning = false;

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
            // --- LOGIKA HALAMAN 1 (PERDAGANGAN) ---
            float currentMoney = MoneyManager.instance.totalMoney;
            txtHartamu.text = "Rp " + currentMoney.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            if (currentMoney >= nisabLimit && !isNisabReached)
            {
                StartZakatLogic();
            }

            // --- LOGIKA HALAMAN 2 (EMAS & PERAK) ---
            if (txtHartaEmas != null) txtHartaEmas.text = MoneyManager.instance.totalEmas.ToString() + " gram";
            if (txtHartaPerak != null) txtHartaPerak.text = MoneyManager.instance.totalPerak.ToString() + " gram";

            CheckEmasPerakNisab();
        }

        // --- LOGIKA HALAMAN 3: UNLOCK & HITUNG DATA TERNAK ---
        CheckLevel3TernakUnlock();
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

    // Coroutine Timer Haul khusus untuk Zakat Ternak Halaman 3
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

    void NextPage()
    {
        if (currentPage < 3)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    void ShowPage(int pageNumber)
    {
        groupZakatPerdagangan.SetActive(false);
        groupZakatEmasPerak.SetActive(false);
        groupZakatTernak.SetActive(false);

        if (pageNumber == 1)
        {
            groupZakatPerdagangan.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(false);
        }
        else if (pageNumber == 2)
        {
            groupZakatEmasPerak.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(true);
        }
        else if (pageNumber == 3)
        {
            groupZakatTernak.SetActive(true);
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
            int sapiSekarang = GetJumlahSapiRealTime();
            int kambingSekarang = GetJumlahKambingRealTime();

            if (sapiSekarang >= nisabSapiKriteria && kambingSekarang >= nisabKambingKriteria)
                txtTernakStatus.text = "Wajib Zakat Sapi & Kambing";
            else if (sapiSekarang >= nisabSapiKriteria)
                txtTernakStatus.text = "Wajib Zakat Sapi";
            else if (kambingSekarang >= nisabKambingKriteria)
                txtTernakStatus.text = "Wajib Zakat Kambing";
            else
                txtTernakStatus.text = "Wajib Zakat Ternak"; 

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
            if (!isTernakUnlocked)
            {
                isTernakUnlocked = true;

                if (panelLockTernak != null) panelLockTernak.SetActive(false);
                if (panelUnlockTernak != null) panelUnlockTernak.SetActive(true);
            }

            // Selalu perbarui teks jumlah kuantitas sapi dan kambing secara berkala
            int sapiSekarang = GetJumlahSapiRealTime();
            int kambingSekarang = GetJumlahKambingRealTime();

            if (txtHartaSapi != null) txtHartaSapi.text = sapiSekarang.ToString() + " Ekor";
            if (txtHartaKambing != null) txtHartaKambing.text = kambingSekarang.ToString() + " Ekor";

            // 🔥 LOGIKA BARU: Jika SUDAH ADA ternak (pembelian pertama sukses), langsung nyalakan timer Haul!
            if (sapiSekarang > 0 || kambingSekarang > 0)
            {
                if (!isTernakCoroutineRunning && !isTernakHaulComplete)
                {
                    StartCoroutine(HaulTimerTernakRoutine());
                }
            }

            // 🔥 EVALUASI NISAB SECARA REAL-TIME: Centang nisab baru menyala jika angka ternak menyentuh batas kriteria
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
                    
                    // Update status UI untuk mengecek apakah Haul juga sudah selesai
                    UpdateTernakStatusUI();
                }
            }
        }
    }

    private int GetJumlahSapiRealTime()
    {
        if (TokoManager.instance != null)
        {
            int sapiDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahSapiDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            
            if (sapiDariToko > trackerJumlahSapiToko)
            {
                int selisihBeli = sapiDariToko - trackerJumlahSapiToko;
                totalEkorSapiInternal += (selisihBeli * 10); 
                trackerJumlahSapiToko = sapiDariToko;

                // 🔥 Pemicu mandiri khusus Sapi
                if (!isSistemSapiBeranakAktif) StartCoroutine(SistemSapiBeranakRoutine());
            }
        }
        return totalEkorSapiInternal; 
    }

    private int GetJumlahKambingRealTime()
    {
        if (TokoManager.instance != null)
        {
            int kambingDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahKambingDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            
            if (kambingDariToko > trackerJumlahKambingToko)
            {
                int selisihBeli = kambingDariToko - trackerJumlahKambingToko;
                totalEkorKambingInternal += (selisihBeli * 10); 
                trackerJumlahKambingToko = kambingDariToko;

                // 🔥 Pemicu mandiri khusus Kambing
                if (!isSistemKambingBeranakAktif) StartCoroutine(SistemKambingBeranakRoutine());
            }
        }
        return totalEkorKambingInternal; 
    }

    // 🔥 COROUTINE KHUSUS SAPI (Ikut waktu interval di Inspector, misal: 10 detik)
    // 🔥 COROUTINE KHUSUS SAPI: Setiap 5 detik bertambah 5 ekor
    IEnumerator SistemSapiBeranakRoutine()
    {
        isSistemSapiBeranakAktif = true;

        while (true)
        {
            yield return new WaitForSeconds(5f); // 5 Detik

            if (totalEkorSapiInternal > 0 && totalEkorSapiInternal < 50)
            {
                totalEkorSapiInternal += 5; // Beranak 5 ekor!
                if (totalEkorSapiInternal > 50) totalEkorSapiInternal = 50; 
                Debug.Log($"<color=white>[Jurnal Ternak]</color> Sapi melahirkan! Jumlah sekarang: {totalEkorSapiInternal} ekor.");
            }
        }
    }

    // 🔥 COROUTINE KHUSUS KAMBING: Setiap 7 detik bertambah 2 ekor
    IEnumerator SistemKambingBeranakRoutine()
    {
        isSistemKambingBeranakAktif = true;

        while (true)
        {
            yield return new WaitForSeconds(7f); // 7 Detik

            if (totalEkorKambingInternal > 0 && totalEkorKambingInternal < 50)
            {
                totalEkorKambingInternal += 2; // Beranak 2 ekor!
                if (totalEkorKambingInternal > 50) totalEkorKambingInternal = 50; 
                Debug.Log($"<color=orange>[Jurnal Ternak]</color> Kambing melahirkan! Jumlah sekarang: {totalEkorKambingInternal} ekor.");
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
        return isEmasPerakNisabReached && isEmasPerakHaulComplete; 
    }

    public bool IsPeternakanUnlocked()
    {
        return isTernakNisabReached && isTernakHaulComplete; 
    }
}