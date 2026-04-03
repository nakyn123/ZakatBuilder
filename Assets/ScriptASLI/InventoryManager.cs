using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager instance;

    [Header("Settings - Wood Counts")]
    public int woodKecilCount = 0;
    public int woodSedangCount = 0;
    public int woodBesarCount = 0;

    [Header("Settings - Prices")]
    public int priceKecil = 500000;  // Tulis 500000 di Inspector
    public int priceSedang = 1500000;
    public int priceBesar = 3000000;

    [Header("UI Elements")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI woodKecilText;
    public TextMeshProUGUI woodSedangText;
    public TextMeshProUGUI woodBesarText;
    
    [Header("Coin Effect Settings")]
    public GameObject uiCoinPrefab; 
    public RectTransform navCoinTarget; 

    // --- PERBAIKAN: Pisahkan Transform untuk tiap tombol ---
    [Header("Sell Button Positions")]
    public Transform btnJualKecilPos; 
    public Transform btnJualSedangPos; 
    public Transform btnJualBesarPos; 

    [Header("Misi Progress - Tetap Bertambah Walau Dijual")]
    public int totalWoodCollected = 0; // Tambahkan ini

    void Awake() { instance = this; }

    void Start() {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        UpdateUI();
    }

    public void AddWood(int amount, int type) {
        if (type == 0) woodKecilCount += amount;
        else if (type == 1) woodSedangCount += amount;
        else if (type == 2) woodBesarCount += amount;

        totalWoodCollected += amount;
        UpdateUI();

        // PERBAIKAN: Ganti UpdateTaskUI menjadi UpdateTebangProgress
        if (TaskManager.instance != null) {
            TaskManager.instance.UpdateTebangProgress(totalWoodCollected);
        }
    }

    public void UpdateUI() {
        if(woodKecilText != null) woodKecilText.text = woodKecilCount.ToString() + "x";
        if(woodSedangText != null) woodSedangText.text = woodSedangCount.ToString() + "x";
        if(woodBesarText != null) woodBesarText.text = woodBesarCount.ToString() + "x";
    }

    public void ToggleInventory() {
        if (inventoryPanel != null) inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    // --- PERBAIKAN: Fungsi jual sekarang mengirim posisi tombolnya ---
    public void JualKayuKecil() {
        if (woodKecilCount > 0) {
            woodKecilCount--;
            UpdateUI();
            SpawnUICoin(priceKecil, btnJualKecilPos);
            if(TaskManager.instance != null) TaskManager.instance.NotifyWoodSold();
        }
    }

    public void JualKayuSedang() {
        if (woodSedangCount > 0) {
            woodSedangCount--;
            UpdateUI();
            SpawnUICoin(priceSedang, btnJualSedangPos);
            if(TaskManager.instance != null) TaskManager.instance.NotifyWoodSold();
        }
    }

    public void JualKayuBesar() {
        if (woodBesarCount > 0) {
            woodBesarCount--;
            UpdateUI();
            SpawnUICoin(priceBesar, btnJualBesarPos);
            if(TaskManager.instance != null) TaskManager.instance.NotifyWoodSold();
        }
    }

    // Fungsi Spawn sekarang menerima parameter posisi
    void SpawnUICoin(int harga, Transform spawnPos) {
        if (uiCoinPrefab == null || navCoinTarget == null || spawnPos == null) return;
        
        int jumlahKoin = 5; 
        for (int i = 0; i < jumlahKoin; i++) {
            GameObject coin = Instantiate(uiCoinPrefab, inventoryPanel.transform.parent);
            coin.transform.SetAsLastSibling();
            
            // Koin muncul di posisi tombol yang diklik
            coin.transform.position = spawnPos.position; 
            
            UICoinEffect effect = coin.AddComponent<UICoinEffect>();
            int nilaiPerKoin = (i == 0) ? harga : 0; 
            effect.Init(navCoinTarget, nilaiPerKoin);
        }
    }
}