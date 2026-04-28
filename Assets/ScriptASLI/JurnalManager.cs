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

    [Header("Halaman 2: Zakat Pertanian")]
    public GameObject groupZakatPertanian;

    [Header("Settings")]
    public float nisabLimit = 5000f; 
    public float timerPerMonth = 5f; // 5 detik per bulan sesuai keinginanmu
    public ZakatPanelManager zakatManager;
    

    private int currentHaulMonth = 0;
    private bool isNisabReached = false;
    private bool isHaulComplete = false;
    private bool isNotificationShown = false;

    void Awake() { instance = this; }

    void Start()
    {
        jurnalContent.SetActive(false);
        asetBlur.SetActive(false);
        if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
        ShowPage(1);
        
        checkNisab.SetActive(false);
        checkHaul.SetActive(false);
        if(checkZakatDagang != null) checkZakatDagang.SetActive(false);
        
        haulSlider.minValue = 0;
        haulSlider.maxValue = 12;
        haulSlider.value = 0;
        messageText.SetActive(false);
        
        UpdateStatusUI();

        btnClose.onClick.AddListener(CloseJurnal);
        btnNext.onClick.AddListener(() => ShowPage(2));
        btnPrevious.onClick.AddListener(() => ShowPage(1));
    }

    void Update()
    {
        if (MoneyManager.instance != null)
        {
            float currentMoney = MoneyManager.instance.totalMoney;
            
            // Update teks tampilan harta di jurnal
            txtHartamu.text = "Rp " + currentMoney.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            // LOGIKA BARU: Jika uang >= 85jt DAN belum pernah terpicu sebelumnya
            if (currentMoney >= nisabLimit && !isNisabReached)
            {
                StartZakatLogic();
            }
        }
    }

    public void StartZakatLogic() {
        // Tambahkan pengecekan agar tidak jalan berulang kali jika sudah aktif
        if (isNisabReached) return;

        isNisabReached = true;

        // Hidupkan centang
        if (checkNisab != null) checkNisab.SetActive(true);
        if (checkZakatDagang != null) checkZakatDagang.SetActive(true);
        if (ikonNotifikasiJurnal != null && !jurnalContent.activeSelf) 
        {
            ikonNotifikasiJurnal.SetActive(true);
        }
        // Mulai slider Haul
        StopAllCoroutines(); 
        StartCoroutine(HaulTimerRoutine());
    }

    IEnumerator HaulTimerRoutine() {
        currentHaulMonth = 0;
        isHaulComplete = false;

        while (currentHaulMonth < 12) {
            float timer = 0;
            while (timer < timerPerMonth) { 
                timer += Time.deltaTime;
                if (haulSlider != null) {
                    // Slider bergerak smooth mengikuti detik
                    haulSlider.value = currentHaulMonth + (timer / timerPerMonth);
                }
                yield return null;
            }
            currentHaulMonth++;
            haulSlider.value = currentHaulMonth; // Pastikan pas di angka bulat setiap bulan
        }

        isHaulComplete = true;
        if (checkHaul != null) checkHaul.SetActive(true);
        UpdateStatusUI(); 
    }

    void ShowPage(int pageNumber)
    {
        if (pageNumber == 1)
        {
            groupZakatPerdagangan.SetActive(true);
            groupZakatPertanian.SetActive(false);
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(false);
        }
        else
        {
            groupZakatPerdagangan.SetActive(false);
            groupZakatPertanian.SetActive(true);
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
                // Jika ini di halaman Perdagangan (Page 1)
                zakatManager.isPerdaganganUnlocked = true;
                zakatManager.UpdateItemVisuals();
            }
            if (!isNotificationShown && !jurnalContent.activeSelf) {
                if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(true);
                isNotificationShown = true; // Tandai agar tidak muncul berulang kali
            }
        }
        else
        {
            txtStatus.text = "Belum Wajib Zakat";
            txtStatus.color = Color.gray; 
            messageText.SetActive(false);
        }
    }

    public void OpenJurnal() { 
        jurnalContent.SetActive(true); 
        asetBlur.SetActive(true); 

        // 4. SAAT JURNAL DIBUKA: Matikan ikon notifikasi (tanda sudah dibaca)
        if (ikonNotifikasiJurnal != null) ikonNotifikasiJurnal.SetActive(false);
    }

    public void CloseJurnal() { jurnalContent.SetActive(false); asetBlur.SetActive(false); }
    public bool IsPerdaganganUnlocked()
    {
        return isNisabReached && isHaulComplete;
    }

    public bool IsPertanianUnlocked()
    {
        return false; // nanti kamu isi sendiri
    }

    public bool IsPeternakanUnlocked()
    {
        return false; // nanti kamu isi sendiri
    }
}