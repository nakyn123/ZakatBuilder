using UnityEngine;
using TMPro;
using System.Globalization;

public class MoneyManager : MonoBehaviour {
    public static MoneyManager instance;
    
    [Header("Uang/Koin Utama")]
    public int totalMoney = 0;
    public TextMeshProUGUI moneyText;

    [Header("Babak 2: Emas & Perak")]
    public int totalEmas = 0;
    public int totalPerak = 0;
    public TextMeshProUGUI emasText;   // Tarik Text TMP Emas ke sini di Inspector
    public TextMeshProUGUI perakText;  // Tarik Text TMP Perak ke sini di Inspector

    void Awake() { instance = this; }

    void Start() { 
        UpdateUI(); 
        UpdateEmasPerakUI();
    }

    // --- LOGIKA UTAMA KOIN ---
    public void AddMoney(int amount) {
        totalMoney += amount;
        UpdateUI();
    }

    public void RemoveMoney(int amount) {
        totalMoney -= amount;
        if (totalMoney < 0) totalMoney = 0;
        UpdateUI();
    }

    void UpdateUI() {
        if (moneyText != null)
            moneyText.text = FormatRupiah(totalMoney);
    }

    // --- LOGIKA BABAK 2: EMAS & PERAK ---
    public void AddEmas(int amount) {
        totalEmas += amount;
        UpdateEmasPerakUI();
        
        // Cek apakah ini memicu misinya berjalan atau jurnal terbuka
        if (JurnalManager.instance != null) {
            JurnalManager.instance.CheckEmasPerakNisab();
        }
    }

    public void AddPerak(int amount) {
        totalPerak += amount;
        UpdateEmasPerakUI();

        if (JurnalManager.instance != null) {
            JurnalManager.instance.CheckEmasPerakNisab();
        }
    }

    // === TAMBAHKAN DUA FUNGSI BARU DI BAWAH INI ===
    public void RemoveEmas(int amount) {
        totalEmas -= amount;
        if (totalEmas < 0) totalEmas = 0;
        UpdateEmasPerakUI();
    }

    public void RemovePerak(int amount) {
        totalPerak -= amount;
        if (totalPerak < 0) totalPerak = 0;
        UpdateEmasPerakUI();
    }

    public void UpdateEmasPerakUI() {
        if (emasText != null) emasText.text = totalEmas.ToString() + " gr";
        if (perakText != null) perakText.text = totalPerak.ToString() + " gr";
    }

    string FormatRupiah(int amount) {
        if (amount == 0) return "Rp 0";
        return "Rp " + amount.ToString("N2", new CultureInfo("id-ID"));
    }
}