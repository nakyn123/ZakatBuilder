using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour {
    public static TaskManager instance;

    [Header("Misi Settings")]
    public string itemName = "Kayu"; 
    public int targetAmount = 5;
    public int rewardAmount = 500;

    [Header("UI Reference")]
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    public GameObject misiPanel;

    [Header("Claim Button Settings")]
    public Button claimButton; 
    public Image claimButtonImage; 
    public Sprite btnAbuAbu; // Masukkan Sprite Button Grey di sini
    public Sprite btnHijauAmbil; // Masukkan Sprite Button Green di sini

    [Header("Coin Effect")]
    public GameObject uiCoinPrefab; 
    public RectTransform navCoinTarget; 

    private bool isTaskCompleted = false;
    private bool isClaimed = false;

    void Awake() {
        instance = this;
    }

    void Start() {
        if (progressSlider != null) {
            progressSlider.minValue = 0;
            progressSlider.maxValue = targetAmount;
            progressSlider.value = 0;
        }
        
        // Start selalu pakai gambar abu-abu
        if (claimButtonImage != null) claimButtonImage.sprite = btnAbuAbu;

        UpdateTaskUI(0);
    }

    public void UpdateTaskUI(int currentCount) {
        if (isClaimed) return;

        if (progressSlider != null) {
            progressSlider.maxValue = targetAmount;
            progressSlider.value = currentCount;
        }

        if (currentCount >= targetAmount) {
            isTaskCompleted = true;
            
            if (progressText != null) {
                progressText.text = "Selesai!";
            }

            // Ganti ke Gambar Hijau
            if (claimButtonImage != null) claimButtonImage.sprite = btnHijauAmbil;

        } else {
            isTaskCompleted = false;
            
            if (progressText != null) {
                progressText.text = itemName + " (" + currentCount.ToString() + "/" + targetAmount.ToString() + ")";
            }

            // Tetap Gambar Abu-abu
            if (claimButtonImage != null) claimButtonImage.sprite = btnAbuAbu;
        }
    }

    public void AmbilHadiah() {
        // Hanya bisa diklik kalau sudah selesai dan belum pernah diklaim
        if (isTaskCompleted && !isClaimed) {
            isClaimed = true;
            
            // Efek koin terbang
            SpawnClaimCoins();

            // Opsional: Hilangkan gambar tombol atau ganti kembali ke abu setelah diambil
            if (claimButtonImage != null) claimButtonImage.color = new Color(1, 1, 1, 0.5f); 
        }
    }

    void SpawnClaimCoins() {
        if (uiCoinPrefab == null || navCoinTarget == null) return;

        int jumlahVisualKoin = 8; 
        
        for (int i = 0; i < jumlahVisualKoin; i++) {
            GameObject coin = Instantiate(uiCoinPrefab, misiPanel.transform.parent);
            coin.transform.SetAsLastSibling();
            coin.transform.position = claimButton.transform.position;

            UICoinEffect effect = coin.AddComponent<UICoinEffect>();
            
            int nilaiKoinIni = (i == 0) ? rewardAmount : 0; 
            effect.Init(navCoinTarget, nilaiKoinIni);
        }
    }

    public void OpenMisi() {
        misiPanel.SetActive(true);
        if (InventoryManager.instance != null) {
            // Ambil total semua jenis kayu untuk menghitung progres misi
            int progressMisi = InventoryManager.instance.totalWoodCollected;
            UpdateTaskUI(progressMisi);
        }
    }

    public void CloseMisi() {
        misiPanel.SetActive(false);
    }
}