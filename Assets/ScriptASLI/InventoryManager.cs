using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager instance;

    [Header("Settings")]
    public int woodCount = 0;
    public int woodPrice = 50;

    [Header("UI Elements")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI woodCountText;
    
    [Header("Coin Effect Settings")]
    public GameObject uiCoinPrefab; // Prefab Image Koin
    public RectTransform navCoinTarget; // Target posisi nav_coin (RectTransform)
    public Transform sellButtonTransform; // Posisi tombol jual buat titik awal koin

    void Awake() { instance = this; }

    void Start() {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        UpdateUI();
    }

    public void AddWood(int amount) {
        woodCount += amount;
        UpdateUI();

        // --- TAMBAHKAN INI ---
        if (TaskManager.instance != null) {
            TaskManager.instance.UpdateTaskUI(woodCount);
        }
    }

    public void UpdateUI() {
        // Sekarang formatnya "1x"
        if(woodCountText != null)
            woodCountText.text = woodCount.ToString() + "x";
    }

    public void ToggleInventory() {
        if (inventoryPanel != null) inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    // FUNGSI BARU: Jual Satu Per Satu
    public void JualSatuKayu() {
        if (woodCount > 0) {
            woodCount--;
            UpdateUI();
            
            // Munculkan koin terbang
            SpawnUICoin();
        }
    }

    void SpawnUICoin() {
        if (uiCoinPrefab == null || navCoinTarget == null) return;

        int jumlahKoin = 5; 
        
        for (int i = 0; i < jumlahKoin; i++) {
            // Spawn koin sebagai anak dari Root Canvas (transform.parent-nya panel)
            // Agar koin tidak tertutup panel, kita set sebagai 'Last Sibling' nanti
            GameObject coin = Instantiate(uiCoinPrefab, inventoryPanel.transform.parent);
            
            // Memastikan koin muncul paling depan di hierarchy UI
            coin.transform.SetAsLastSibling();
            
            // PENTING: Set posisi tepat di tombol jual
            coin.transform.position = sellButtonTransform.position;

            UICoinEffect effect = coin.AddComponent<UICoinEffect>();
            
            int nilaiPerKoin = (i == 0) ? woodPrice : 0; 
            effect.Init(navCoinTarget, nilaiPerKoin);
        }
    }
}