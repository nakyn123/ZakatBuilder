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

    private string expression = ""; 

    void OnEnable()
    {
        if (MoneyManager.instance != null)
        {
            txtHartaku.text = FormatRupiah(MoneyManager.instance.totalMoney);
        }
        Clear();
        btnSelesai.onClick.RemoveAllListeners(); 
        btnSelesai.onClick.AddListener(ValidateZakat);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
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
            
            // Siapkan string untuk dihitung sistem
            string formula = expression.Replace("x", "*")
                                       .Replace("2.5%", "0.025")
                                       .Replace("5%", "0.05")
                                       .Replace("10%", "0.1");

            var result = dt.Compute(formula, "");
            double finalVal = System.Convert.ToDouble(result);

            // Munculkan hasil dengan maksimal 3 angka di belakang koma
            expression = finalVal.ToString("G29"); // G29 menghapus nol berlebih di akhir tapi tetap presisi
            
            // Jika ingin fix 3 angka (misal 25.000), gunakan finalVal.ToString("F3")
            
            UpdateDisplay();
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

    // ... (Fungsi ValidateZakat dan FormatRupiah tetap sama)
    void ValidateZakat()
    {
        float totalHarta = MoneyManager.instance.totalMoney;
        float correctAmount = totalHarta * 0.025f;

        if (float.TryParse(inputZakat.text, out float userGuess))
        {
            // Gunakan toleransi kecil untuk perbandingan float
            if (Mathf.Abs(userGuess - correctAmount) < 1.0f) // Ubah 0.01f ke 1.0f jika nominal Rupiah bulat
            {
                if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);

                // --- TAMBAHKAN BARIS INI ---
                if (mainPanelScript != null)
                {
                    mainPanelScript.MunculkanReward();
                }
                else
                {
                    Debug.LogError("Main Panel Script belum diisi di Inspector!");
                }
            }
            else
            {
                if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
            }
        }
    }

    string FormatRupiah(int amount)
    {
        return "Rp " + amount.ToString("N0", new CultureInfo("id-ID"));
    }
}