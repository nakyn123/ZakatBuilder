using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JurnalManager : MonoBehaviour
{
    [Header("UI Panels & General Buttons")]
    public GameObject jurnalContent;
    public GameObject asetBlur;
    public Button btnClose;
    public Button btnNext;
    public Button btnPrevious;

    [Header("Halaman 1: Zakat Perdagangan")]
    public GameObject groupZakatPerdagangan; // Tarik objek 'zakat perdagangan' ke sini
    public GameObject checkNisab;
    public GameObject checkHaul;
    public TextMeshProUGUI txtHartamu;
    public TextMeshProUGUI txtStatus;
    public GameObject messageText;
    public Slider haulSlider;

    [Header("Halaman 2: Zakat Pertanian")]
    public GameObject groupZakatPertanian; // Tarik objek 'zakat pertanian' ke sini

    [Header("Settings")]
    public float nisabLimit = 85000000f; 
    public float timerPerMonth = 120f; 

    private int currentHaulMonth = 0;
    private float timerCounter = 0;
    private bool isNisabReached = false;
    private bool isHaulComplete = false;

    void Start()
    {
        // Setup Awal
        jurnalContent.SetActive(false);
        asetBlur.SetActive(false);
        
        // Mulai di Halaman 1
        ShowPage(1);
        
        // Logika Zakat Awal
        checkNisab.SetActive(false);
        checkHaul.SetActive(false);
        haulSlider.minValue = 0;
        haulSlider.maxValue = 12;
        haulSlider.value = 0;
        messageText.SetActive(false);
        
        UpdateStatusUI();

        // Button Events
        btnClose.onClick.AddListener(CloseJurnal);
        btnNext.onClick.AddListener(() => ShowPage(2));
        btnPrevious.onClick.AddListener(() => ShowPage(1));
    }

    void Update()
    {
        // --- LOGIKA ZAKAT PERDAGANGAN ---
        if (MoneyManager.instance != null)
        {
            float currentMoney = MoneyManager.instance.totalMoney;
            txtHartamu.text = "Hartamu : Rp " + currentMoney.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            if (currentMoney >= nisabLimit && !isNisabReached)
            {
                isNisabReached = true;
                checkNisab.SetActive(true);
            }
        }

        if (isNisabReached && !isHaulComplete)
        {
            timerCounter += Time.deltaTime;
            if (timerCounter >= timerPerMonth)
            {
                timerCounter = 0;
                currentHaulMonth++;
                haulSlider.value = currentHaulMonth;

                if (currentHaulMonth >= 12)
                {
                    isHaulComplete = true;
                    checkHaul.SetActive(true);
                }
            }
        }
        UpdateStatusUI();
    }

    // FUNGSI PINDAH HALAMAN
    void ShowPage(int pageNumber)
    {
        if (pageNumber == 1)
        {
            groupZakatPerdagangan.SetActive(true);
            groupZakatPertanian.SetActive(false);
            
            btnNext.gameObject.SetActive(true);
            btnPrevious.gameObject.SetActive(false);
        }
        else if (pageNumber == 2)
        {
            groupZakatPerdagangan.SetActive(false);
            groupZakatPertanian.SetActive(true);
            
            btnNext.gameObject.SetActive(false); // Sembunyikan btn_next di hal terakhir
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
        }
        else
        {
            txtStatus.text = "Belum Wajib Zakat";
            txtStatus.color = HexToColor("#5A5353");
            messageText.SetActive(false);
        }
    }

    public void OpenJurnal()
    {
        jurnalContent.SetActive(true);
        asetBlur.SetActive(true);
        ShowPage(1); // Reset ke halaman 1 setiap buka jurnal
    }

    public void CloseJurnal()
    {
        jurnalContent.SetActive(false);
        asetBlur.SetActive(false);
    }

    Color HexToColor(string hex)
    {
        Color color = Color.gray;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}