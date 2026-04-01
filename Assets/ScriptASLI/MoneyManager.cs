using UnityEngine;
using TMPro;
using System.Globalization;

public class MoneyManager : MonoBehaviour {
    public static MoneyManager instance;
    public int totalMoney = 0;
    public TextMeshProUGUI moneyText; // Tarik teks nav_coin ke sini

    void Awake() { instance = this; }

    void Start() { UpdateUI(); }

    public void AddMoney(int amount) {
        totalMoney += amount;
        UpdateUI();
    }

    void UpdateUI() {
        if (moneyText != null)
            moneyText.text = FormatRupiah(totalMoney);
    }

    string FormatRupiah(int amount) {
        // Cek jika 0, kembalikan string "0" saja
        if (amount == 0) return "Rp 0";

        // Gunakan CultureInfo Indonesia agar pemisahnya otomatis titik (.)
        // Format "N2" artinya Number dengan 2 angka di belakang koma
        return "Rp " + amount.ToString("N2", new CultureInfo("id-ID"));
    }
}