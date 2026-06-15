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
        if (!isSistemSapiBeranakAktif && totalEkorSapiInternal > 0) 
        {
            coSapiBeranak = StartCoroutine(SistemSapiBeranakRoutine());
        }
        return totalEkorSapiInternal; 
    }

    public int GetJumlahKambingRealTime()
    {
        if (!isSistemKambingBeranakAktif && totalEkorKambingInternal > 0) 
        {
            coKambingBeranak = StartCoroutine(SistemKambingBeranakRoutine());
        }
        return totalEkorKambingInternal; 
    }

    private void CekPembelianTokoTernak()
    {
        if (TokoManager.instance != null)
        {
            int sapiDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahSapiDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            if (sapiDariToko > trackerJumlahSapiToko)
            {
                int selisihBeli = sapiDariToko - trackerJumlahSapiToko;
                totalEkorSapiInternal += (selisihBeli * 10); 
                trackerJumlahSapiToko = sapiDariToko;

                if (!isSistemSapiBeranakAktif) coSapiBeranak = StartCoroutine(SistemSapiBeranakRoutine());
            }

            int kambingDariToko = (int)System.Type.GetType("TokoManager").GetField("jumlahKambingDibeli", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TokoManager.instance);
            if (kambingDariToko > trackerJumlahKambingToko)
            {
                int selisihBeli = kambingDariToko - trackerJumlahKambingToko;
                totalEkorKambingInternal += (selisihBeli * 10); 
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
            yield return new WaitForSeconds(3f); 

            if (totalEkorSapiInternal > 0)
            {
                totalEkorSapiInternal += 5; 
                if (totalEkorSapiInternal > 200) totalEkorSapiInternal = 200; 
                
                Debug.Log($"<color=white>[Jurnal Ternak]</color> Sapi melahirkan! Jumlah sekarang: {totalEkorSapiInternal} ekor.");
            }
        }
    }

    // 🔥 PERBAIKAN: Variabel disesuaikan dengan 'jumlahTambahanKambing' dan diletakkan di dalam while loop dengan benar
    IEnumerator SistemKambingBeranakRoutine()
    {
        isSistemKambingBeranakAktif = true;

        while (true)
        {
            yield return new WaitForSeconds(intervalBeranakKambing); 

            if (totalEkorKambingInternal > 0)
            {
                totalEkorKambingInternal += jumlahTambahanKambing; 
                
                Debug.Log($"<color=orange>[Jurnal Ternak]</color> Kambing melahirkan! Jumlah sekarang: {totalEkorKambingInternal} ekor.");
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

        if (asetBlur != null) asetBlur.SetActive(true);
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