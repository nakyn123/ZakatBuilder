using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using System.Data;

public class ZakatCalculator : MonoBehaviour
{
    [Header("Form UI")]
    public TMP_Text txtHartaku;
    public TMP_InputField inputZakat;
    public Button btnSelesai;

    [Header("Kalkulator UI")]
    public TMP_Text txtDisplayKalkulator; 
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip wrongSound;
    public AudioClip correctSound;

    public ZakatPerdaganganPanel mainPanelScript;
    [Header("Zakat Emas Perak Context")]
    public ZakatEmasPerakPanel emasPerakPanelScript; // Drag skrip baru tadi ke slot ini di Inspector

    private string expression = ""; 

    void OnEnable()
    {
        // --- SEKARANG AMAN: Hanya merubah teks jika benar-benar berada di panel perdagangan ---
        if (MoneyManager.instance != null && mainPanelScript != null && mainPanelScript.gameObject.activeInHierarchy)
        {
            if (txtHartaku != null)
            {
                txtHartaku.text = FormatRupiah(MoneyManager.instance.totalMoney);
            }
        }
        
        // Jika yang aktif adalah panel emas perak, biarkan skrip dinamis emas perak yang mengontrol teksnya
        if (emasPerakPanelScript != null && emasPerakPanelScript.gameObject.activeInHierarchy)
        {
            emasPerakPanelScript.ConfigureFormDinamis();
        }

        Clear();
        btnSelesai.onClick.RemoveAllListeners(); 
        btnSelesai.onClick.AddListener(ValidateZakat);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // Fungsi baru untuk dipanggil secara paksa saat pindah halaman kuis
    public void SetupHartaRupiah()
    {
        if (MoneyManager.instance != null && mainPanelScript != null && mainPanelScript.gameObject.activeInHierarchy)
        {
            if (txtHartaku != null)
            {
                txtHartaku.text = FormatRupiah(MoneyManager.instance.totalMoney);
            }
        }
    }

    public void PushNumber(string number)
    {
        expression += number;
        UpdateDisplay();
    }

    public void PushOperator(string op)
    {
        if (string.IsNullOrEmpty(expression)) return;

        // Cek jika karakter terakhir sudah spasi (berarti sudah ada operator), jangan double
        if (expression.EndsWith(" ")) return;

        // Tambahkan spasi sebelum dan sesudah operator agar rapi
        expression += " " + op + " "; 
        UpdateDisplay();
    }

    public void PushPercent(string percentLabel)
    {
        if (string.IsNullOrEmpty(expression)) return;

        // Tambahkan label persen ke layar dengan spasi
        expression += " " + percentLabel;
        UpdateDisplay();
    }

    public void ExecuteResult()
    {
        if (string.IsNullOrEmpty(expression)) return;
        try {
            DataTable dt = new DataTable();
            
            string formula = expression.Replace("x", "*")
                                    .Replace("2.5%", "0.025")
                                    .Replace("5%", "0.05")
                                    .Replace("10%", "0.1");

            var result = dt.Compute(formula, "");
            double finalVal = System.Convert.ToDouble(result);

            expression = finalVal.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            UpdateDisplay();

            // --- SISTEM UPDATE INPUT FIELD OTOMATIS BERDASARKAN PANEL YANG AKTIF ---
            if (mainPanelScript != null && mainPanelScript.gameObject.activeInHierarchy)
            {
                // JIKA DI PANEL PERDAGANGAN: Format ke Rupiah dengan titik ribuan
                if (inputZakat != null) 
                {
                    if (double.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
                    {
                        inputZakat.text = "Rp " + parsedValue.ToString("N0", new CultureInfo("id-ID"));
                    }
                    else
                    {
                        inputZakat.text = expression;
                    }
                }
            }
            else if (emasPerakPanelScript != null && emasPerakPanelScript.gameObject.activeInHierarchy)
            {
                // JIKA DI PANEL EMAS/PERAK: Ditambahkan akhiran " gr"
                string formattedGram = expression + " gr";

                if (emasPerakPanelScript.isEmasWajib && emasPerakPanelScript.isPerakWajib)
                {
                    // Jika dua-duanya aktif, isi ke input field yang sedang kosong atau jadikan default ke Emas terlebih dahulu
                    if (string.IsNullOrEmpty(emasPerakPanelScript.inputZakatEmas.text))
                        emasPerakPanelScript.inputZakatEmas.text = formattedGram;
                    else
                        emasPerakPanelScript.inputZakatPerak.text = formattedGram;
                }
                else if (emasPerakPanelScript.isEmasWajib)
                {
                    emasPerakPanelScript.inputZakatEmas.text = formattedGram;
                }
                else if (emasPerakPanelScript.isPerakWajib)
                {
                    emasPerakPanelScript.inputZakatPerak.text = formattedGram;
                }
            }
        } catch {
            expression = "Error";
            UpdateDisplay();
        }
    }

    public void Clear()
    {
        expression = "";
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        txtDisplayKalkulator.text = string.IsNullOrEmpty(expression) ? "0" : expression;
    }

    public void ValidateZakat()
    {
        // 1. Jika yang sedang aktif adalah Panel Perdagangan
        if (mainPanelScript != null && mainPanelScript.gameObject.activeInHierarchy)
        {
            float totalHarta = MoneyManager.instance.totalMoney;
            float correctAmount = totalHarta * 0.025f; 

            // PERBAIKAN: Bersihkan simbol "Rp", spasi, dan titik "." agar bisa di-parse dengan aman
            string cleanInput = inputZakat.text.Replace("Rp", "").Replace(" ", "").Replace(".", "");

            if (float.TryParse(cleanInput, out float userGuess))
            {
                if (Mathf.Abs(userGuess - correctAmount) < 1.0f) 
                {
                    if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);

                    int amountToPay = Mathf.RoundToInt(userGuess);
                    MoneyManager.instance.RemoveMoney(amountToPay);

                    if (mainPanelScript != null)
                    {
                        mainPanelScript.MunculkanReward();
                    }
                }
                else
                {
                    if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
                    Debug.Log($"Jawaban salah! Input: {userGuess}, Seharusnya: {correctAmount}");
                }
            }
        }
        // 2. Jika yang sedang aktif adalah Panel Emas & Perak
        else if (emasPerakPanelScript != null && emasPerakPanelScript.gameObject.activeInHierarchy)
        {
            Debug.Log("[ZakatCalculator] Mengalihkan validasi langsung ke fungsi internal Emas Perak.");
            
            // Panggil langsung fungsi internalnya
            emasPerakPanelScript.ValidateZakatEmasPerak();
        }
    }
    string FormatRupiah(int amount)
    {
        return "Rp " + amount.ToString("N0", new CultureInfo("id-ID"));
    }
}