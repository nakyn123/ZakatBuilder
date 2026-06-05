using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager instance;

    [Header("Settings - Wood Counts")]
    public int woodKecilCount = 0;
    public int woodSedangCount = 0;
    public int woodBesarCount = 0;

    // 🔥 TAMBAHAN BARU: Hitungan Aset Emas & Perak di Inventory
    [Header("Settings - Logam Mulia Counts")]
    public int asetEmasCount = 0;
    public int asetPerakCount = 0;

    [Header("Settings - Prices")]
    public int priceKecil = 500000;  
    public int priceSedang = 1500000;
    public int priceBesar = 3000000;
    // 🔥 TAMBAHAN BARU: Harga Jual Sesuai Permintaan (Emas 1JT, Perak 500K)
    public int priceAsetEmas = 1000000;
    public int priceAsetPerak = 500000;
    public int pricePakanRumput = 300000;

    [Header("UI Elements")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI woodKecilText;
    public TextMeshProUGUI woodSedangText;
    public TextMeshProUGUI woodBesarText;
    // 🔥 TAMBAHAN BARU: Slot Teks untuk Emas & Perak
    public TextMeshProUGUI asetEmasText;
    public TextMeshProUGUI asetPerakText;
    
    [Header("Coin Effect Settings")]
    public GameObject uiCoinPrefab; 
    public RectTransform navCoinTarget; 
    [SerializeField] private GameObject navCoinObject;

    [Header("Sell Button Positions")]
    public Transform btnJualKecilPos; 
    public Transform btnJualSedangPos; 
    public Transform btnJualBesarPos; 
    // 🔥 TAMBAHAN BARU: Posisi Tombol Jual Emas & Perak
    public Transform btnJualEmasPos;
    public Transform btnJualPerakPos;
    public Transform btnJualRumputPos;

    [Header("Misi Progress - Tetap Bertambah Walau Dijual")]
    public int totalWoodCollected = 0; 

    [Header("Slot GameObjects")]
    public GameObject slotKecil;
    public GameObject slotSedang;
    public GameObject slotBesar;
    // 🔥 TAMBAHAN BARU: Slot Parent di dalam ASET_ARRAY
    public GameObject slotAsetEmas;
    public GameObject slotAsetPerak;
    public RectTransform asetArrayRect; 
    private Transform originalNavCoinParent;

    [Header("Settings - Pakan / Rumput")]
    public int pakanRumputCount = 0; // Menyimpan sisa kuota pakan eceran (bisa sampai banyak)
    public TextMeshProUGUI pakanRumputText; // Tarik Text UI Rumput dari Screenshot 2026-06-05 132554.png
    public GameObject slotPakanRumput;

    void Awake() { instance = this; }

    void Start() {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (navCoinObject != null) {
            originalNavCoinParent = navCoinObject.transform.parent;
        }
        UpdateUI(); 
    }

    // 🔥 FUNGSI BARU: Dipanggil saat transisi Level 3 untuk memindahkan sisa emas & perak mentah ke inventory
    public void KonversiSisaLogamKeAset(int sisaEmas, int sisaPerak) {
        asetEmasCount = sisaEmas;
        asetPerakCount = sisaPerak;
        
        Debug.Log($"<color=orange>[Inventory]</color> Berhasil mengonversi sisa logam mulia menjadi aset: {sisaEmas}g Emas & {sisaPerak}g Perak.");
        UpdateUI();
    }

    public void AddWood(int amount, int type) {
        if (type == 0) woodKecilCount += amount;
        else if (type == 1) woodSedangCount += amount;
        else if (type == 2) woodBesarCount += amount;

        totalWoodCollected += amount;
        UpdateUI();

        if (TaskManager.instance != null) {
            TaskManager.instance.UpdateTebangProgress(totalWoodCollected);
        }
    }

    public void UpdateUI() {
        // Logika Kayu Kecil
        bool punyaKecil = woodKecilCount > 0;
        slotKecil.SetActive(punyaKecil);
        if (punyaKecil) woodKecilText.text = woodKecilCount.ToString() + "x";

        // Logika Kayu Sedang
        bool punyaSedang = woodSedangCount > 0;
        slotSedang.SetActive(punyaSedang);
        if (punyaSedang) woodSedangText.text = woodSedangCount.ToString() + "x";

        // Logika Kayu Besar
        bool punyaBesar = woodBesarCount > 0;
        slotBesar.SetActive(punyaBesar);
        if (punyaBesar) woodBesarText.text = woodBesarCount.ToString() + "x";

        // 🔥 TAMBAHAN BARU: Logika Tampilan UI Slot Emas
        if (slotAsetEmas != null) {
            bool punyaAsetEmas = asetEmasCount > 0;
            slotAsetEmas.SetActive(punyaAsetEmas);
            if (punyaAsetEmas && asetEmasText != null) asetEmasText.text = asetEmasCount.ToString() + "x";
        }

        // 🔥 TAMBAHAN BARU: Logika Tampilan UI Slot Perak
        if (slotAsetPerak != null) {
            bool punyaAsetPerak = asetPerakCount > 0;
            slotAsetPerak.SetActive(punyaAsetPerak);
            if (punyaAsetPerak && asetPerakText != null) asetPerakText.text = asetPerakCount.ToString() + "x";
        }

        if (asetArrayRect != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(asetArrayRect);
        }
        if (slotPakanRumput != null) {
            bool punyaPakan = pakanRumputCount > 0;
            slotPakanRumput.SetActive(punyaPakan);
            if (punyaPakan && pakanRumputText != null) {
                pakanRumputText.text = pakanRumputCount.ToString() + "x";
            }
        }
    }

    public void ToggleInventory() {
        if (inventoryPanel == null) return;
        bool currentState = inventoryPanel.activeSelf;

        if (!currentState) {
            if (UIManager.instance != null) UIManager.instance.OpenPanelMenu(inventoryPanel);
            else inventoryPanel.SetActive(true);

            // 🔥 PAKSA UPDATE: Begitu tas dibuka di Level 3, langsung hitung ulang visual slot pakan & logam mulia
            UpdateUI(); 

            if (navCoinObject != null) {
                navCoinObject.transform.SetParent(inventoryPanel.transform);
                navCoinObject.transform.SetAsLastSibling(); 
                navCoinObject.SetActive(true); 
            }
        } else {
            if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(inventoryPanel);
            else inventoryPanel.SetActive(false);

            if (navCoinObject != null && originalNavCoinParent != null) {
                navCoinObject.transform.SetParent(originalNavCoinParent);
            }
        }
    }

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

    // 🔥 TAMBAHAN BARU: Fungsi Jual Aset Emas
    // ✅ PERBAIKAN: FUNGSI JUAL EMAS
    public void JualAsetEmas() {
        if (asetEmasCount <= 0) return;

        int jumlahDijual = 0;

        if (asetEmasCount >= 10) {
            jumlahDijual = 10;
        } else {
            jumlahDijual = asetEmasCount; 
        }

        asetEmasCount -= jumlahDijual;
        int totalPendapatan = priceAsetEmas * jumlahDijual;

        UpdateUI();
        
        // 🔥 GANTI DI SINI: Gunakan btnJualEmasPos, bukan btnJualKecilPos lagi!
        SpawnUICoin(totalPendapatan, btnJualEmasPos); 

    }

    // ✅ PERBAIKAN: FUNGSI JUAL PERAK
    public void JualAsetPerak() {
        if (asetPerakCount <= 0) return;

        int jumlahDijual = 0;

        if (asetPerakCount >= 100) {
            jumlahDijual = 100;
        } else {
            jumlahDijual = asetPerakCount; 
        }

        asetPerakCount -= jumlahDijual; 
        int totalPendapatan = priceAsetPerak * jumlahDijual;

        UpdateUI();
        
        // 🔥 GANTI DI SINI: Gunakan btnJualPerakPos, bukan btnJualKecilPos lagi!
        SpawnUICoin(totalPendapatan, btnJualPerakPos); 
        
        // if (MoneyManager.instance != null) {
        //     MoneyManager.instance.AddMoney(totalPendapatan);
        // }
    }
    public void JualPakanRumput() {
        // Pastikan rumputnya ada dulu baru bisa dijual
        if (pakanRumputCount > 0) {
            pakanRumputCount--; // Berkurang 1x rumput
            UpdateUI(); // Perbarui teks angka di UI asset
            
            // Munculin efek koin terbang terbang dari posisi tombol rumput
            SpawnUICoin(pricePakanRumput, btnJualRumputPos);
        }
    }

    void SpawnUICoin(int harga, Transform spawnPos) {
        if (uiCoinPrefab == null || navCoinTarget == null || spawnPos == null) return;
        int jumlahKoin = 5; 
        for (int i = 0; i < jumlahKoin; i++) {
            GameObject coin = Instantiate(uiCoinPrefab, inventoryPanel.transform.parent);
            coin.transform.SetAsLastSibling();
            coin.transform.position = spawnPos.position; 
            UICoinEffect effect = coin.AddComponent<UICoinEffect>();
            int nilaiPerKoin = (i == 0) ? harga : 0; 
            effect.Init(navCoinTarget, nilaiPerKoin);
        }
    }

    public void AddPakanDariToko() {
        pakanRumputCount += 6; // Beli 1 dapat 6x pakan
        UpdateUI();
    }

    // Fungsi untuk mengecek dan mengurangi pakan saat dipakai di dunia 3D
    public bool GunakanPakanDiWorld() {
        if (pakanRumputCount > 0) {
            pakanRumputCount--; // Berkurang 1x setiap dipakai mengisi tempat pakan
            UpdateUI();
            return true; // Berhasil menggunakan pakan
        }
        return false; // Gagal karena pakan habis
    }
}