using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections; // Wajib untuk menjalankan Coroutine
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels & Groups")]
    [SerializeField] private GameObject thumbnailGroup;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject aboutUsPanel;
    [SerializeField] private GameObject panduanPanel;

    [Header("Panel Loading Restart (Tambahan Baru)")]
    [SerializeField] private GameObject loadingRestartPanel; // Seret panel loading restart kamu ke sini
    [SerializeField] private Slider loadingSlider;           // Seret slider restart (tanpa handle) ke sini
    [SerializeField] private TextMeshProUGUI txtLoadingInfo;  // Seret Text TMP loading di bawah slider ke sini
    [SerializeField] private float durasiLoadingRestart = 3.5f; // Atur berapa detik waktu reset/loading-nya

    [Header("Main Buttons (Tetap Pakai Sound)")]
    [SerializeField] private Button mainBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button aboutUsBtn;
    [SerializeField] private Button panduanBtn;

    [Header("Close & Quit Buttons (Gausah Kasih Sound)")]
    [SerializeField] private Button keluarBtn;
    [SerializeField] private Button closeSettingsBtn;
    [SerializeField] private Button closeAboutUsBtn;
    [SerializeField] private Button closePanduanBtn;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer; 
    [SerializeField] private Slider musikSlider;   
    [SerializeField] private Slider sfxSlider;     

    [Header("Audio SFX Buttons")]
    [SerializeField] private AudioSource audioSourceMenu; 
    [SerializeField] private AudioClip suaraKlikTombol;   

    [Header("Restart Settings")]
    [SerializeField] private Button restartBtn;    

    [Header("Scene Destination")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    private void Start()
    {
        // 1. Inisialisasi Panel saat pertama kali buka Home
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutUsPanel != null) aboutUsPanel.SetActive(false);
        if (panduanPanel != null) panduanPanel.SetActive(false);
        if (loadingRestartPanel != null) loadingRestartPanel.SetActive(false); // Pastikan loading mati di awal
        if (thumbnailGroup != null) thumbnailGroup.SetActive(true);

        // 2. Setup Listener Tombol Utama & Close
        if (mainBtn != null) mainBtn.onClick.AddListener(PlayGame);
        if (settingsBtn != null) settingsBtn.onClick.AddListener(OpenSettings);
        if (aboutUsBtn != null) aboutUsBtn.onClick.AddListener(OpenAboutUs);
        if (panduanBtn != null) panduanBtn.onClick.AddListener(OpenPanduan);
        if (keluarBtn != null) keluarBtn.onClick.AddListener(QuitGame);
        if (closeSettingsBtn != null) closeSettingsBtn.onClick.AddListener(CloseSettings);
        if (closeAboutUsBtn != null) closeAboutUsBtn.onClick.AddListener(CloseAboutUs);
        if (closePanduanBtn != null) closePanduanBtn.onClick.AddListener(ClosePanduan);

        // 3. Setup Listener Fitur (Audio & Restart)
        if (restartBtn != null) restartBtn.onClick.AddListener(RestartGameData);
        
        if (musikSlider != null) musikSlider.onValueChanged.AddListener(SetMusikVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        LoadAudioSettings();
    }

    public void PlayGame()
    {
        PlayClickSound(); 
        SceneManager.LoadScene(gameplaySceneName);
    }

    // --- LOGIKA TOMBOL MULAI ULANG (RESTART) ---
    // --- LOGIKA TOMBOL MULAI ULANG (RESTART) ---
    public void RestartGameData()
    {
        PlayClickSound(); //

        // 🔥 1. Bersihkan total seluruh PlayerPrefs di sini sebelum coroutine dimulai
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[Main Menu]</color> Seluruh PlayerPrefs telah dibersihkan murni!");

        // 🔥 2. Panggil Coroutine asli milikmu (Namanya ProsesLoadingResetHistory)
        StartCoroutine(ProsesLoadingResetHistory()); //
    }

    private IEnumerator ProsesLoadingResetHistory()
    {
        // 1. Sembunyikan panel Settings dan Thumbnail Group, lalu munculkan Panel Loading kamu
        if (settingsPanel != null) settingsPanel.SetActive(false);
        ToggleThumbnailGroup(false);
        if (loadingRestartPanel != null) loadingRestartPanel.SetActive(true);

        // 2. Reset nilai awal slider loading
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = durasiLoadingRestart;
            loadingSlider.value = 0f;
        }

        // 3. Lakukan pembersihan data PlayerPrefs secara instan di background
        PlayerPrefs.SetInt("IsRestarted", 1); //
        PlayerPrefs.DeleteKey("IntroSelesai");
        // Tambahkan ini di dalam ProsesLoadingResetHistory() di MainMenuManager.cs
        PlayerPrefs.DeleteKey("IntroSelesai");
        PlayerPrefs.DeleteKey("Panel17Selesai");
        PlayerPrefs.DeleteKey("Panel18Selesai");
        PlayerPrefs.DeleteKey("TotalKayuDitebang"); //
        PlayerPrefs.DeleteKey("JumlahUangPemain"); //
        PlayerPrefs.DeleteKey("EmasPemain"); //
        PlayerPrefs.DeleteKey("SudahPernahDialogToko");  //
        PlayerPrefs.DeleteKey("Saved_IsMisi1Claimed"); //
        PlayerPrefs.DeleteKey("Saved_IsJualDone"); //
        PlayerPrefs.DeleteKey("Saved_IsMisi2Started"); //
        PlayerPrefs.DeleteKey("Saved_IsTebangDone"); //
        PlayerPrefs.DeleteKey("Saved_IsMisi2Claimed"); //
        PlayerPrefs.DeleteKey("Saved_IsKeTokoDone"); //
        PlayerPrefs.DeleteKey("Saved_IsKeTokoClaimed"); //
        PlayerPrefs.DeleteKey("Saved_BeliHewanCount"); //
        PlayerPrefs.DeleteKey("Saved_IsBeliPakanDone"); //
        PlayerPrefs.DeleteKey("Saved_IsBeliPakanClaimed"); //
        PlayerPrefs.DeleteKey("Saved_IsiPakanCount"); //
        PlayerPrefs.DeleteKey("Saved_IsIsiPakanDone"); //
        PlayerPrefs.DeleteKey("Saved_IsIsiPakanClaimed"); //
        PlayerPrefs.DeleteKey("Saved_WoodOffset"); //
        PlayerPrefs.DeleteKey("Saved_PerakPemain"); //
        PlayerPrefs.DeleteKey("Saved_WoodKecil"); //
        PlayerPrefs.DeleteKey("Saved_WoodSedang"); //
        PlayerPrefs.DeleteKey("Saved_WoodBesar"); //
        PlayerPrefs.DeleteKey("Saved_AsetEmas"); //
        PlayerPrefs.DeleteKey("Saved_AsetPerak"); //
        PlayerPrefs.DeleteKey("Saved_PakanRumput"); //
        PlayerPrefs.DeleteKey("Saved_PlayerX"); //
        PlayerPrefs.DeleteKey("Saved_PlayerY"); //
        PlayerPrefs.DeleteKey("Saved_PlayerZ"); //
        PlayerPrefs.Save(); //

        // 4. Jalankan Animasi Bergerak Slider & Loop Teks Mengetik "Sedang mereset history permainanmu..."
        float timerElapsed = 0f;
        string teksTarget = "Sedang mereset history permainanmu...";
        
        while (timerElapsed < durasiLoadingRestart)
        {
            timerElapsed += Time.deltaTime;
            
            // Gerakkan isi bar slider secara realtime
            if (loadingSlider != null) loadingSlider.value = timerElapsed;

            // Membuat efek teks mengetik dan terhapus berulang-ulang berdasarkan sisa waktu (sinusoidal/pingpong)
            if (txtLoadingInfo != null)
            {
                int jumlahHurufMuncul = Mathf.FloorToInt((Time.time * 15f) % (teksTarget.Length + 5));
                if (jumlahHurufMuncul <= teksTarget.Length)
                {
                    txtLoadingInfo.text = teksTarget.Substring(0, jumlahHurufMuncul);
                }
                else
                {
                    // Efek menghapus (mundur)
                    int sisaHapus = (teksTarget.Length + 5) - jumlahHurufMuncul;
                    if(sisaHapus > 0 && sisaHapus <= teksTarget.Length)
                        txtLoadingInfo.text = teksTarget.Substring(0, sisaHapus);
                }
            }

            yield return null;
        }

        // 5. LOADING SELESAI: Kembalikan tampilan utama menu Home semula
        if (loadingRestartPanel != null) loadingRestartPanel.SetActive(false);
        ToggleThumbnailGroup(true);
        Debug.Log("<color=cyan>[MainMenu]</color> Loading Selesai, game siap dimainkan dari awal!");
    }

    // --- LOGIKA AUDIO MIXER & SLIDER ---
    public void SetMusikVolume(float value)
    {
        if (value <= 0.05f) mainMixer.SetFloat("MusikVol", -80f);
        else mainMixer.SetFloat("MusikVol", Mathf.Log10(value) * 20f);
        
        PlayerPrefs.SetFloat("MusikVolumeValue", value);
    }

    public void SetSFXVolume(float value)
    {
        if (value <= 0.05f) mainMixer.SetFloat("SFXVol", -80f);
        else mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20f);
        
        PlayerPrefs.SetFloat("SFXVolumeValue", value);
    }

    private void LoadAudioSettings()
    {
        float savedMusik = PlayerPrefs.GetFloat("MusikVolumeValue", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolumeValue", 0.75f);

        if (musikSlider != null) musikSlider.value = savedMusik;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
    }

    public void OpenSettings() { PlayClickSound(); if (settingsPanel != null) settingsPanel.SetActive(true); ToggleThumbnailGroup(false); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); ToggleThumbnailGroup(true); }
    public void OpenAboutUs() { PlayClickSound(); if (aboutUsPanel != null) aboutUsPanel.SetActive(true); ToggleThumbnailGroup(false); }
    public void CloseAboutUs() { if (aboutUsPanel != null) aboutUsPanel.SetActive(false); ToggleThumbnailGroup(true); }
    public void OpenPanduan() { PlayClickSound(); if (panduanPanel != null) panduanPanel.SetActive(true); ToggleThumbnailGroup(false); }
    public void ClosePanduan() { if (panduanPanel != null) panduanPanel.SetActive(false); ToggleThumbnailGroup(true); }

    private void ToggleThumbnailGroup(bool isActive) { if (thumbnailGroup != null) thumbnailGroup.SetActive(isActive); }

    public void QuitGame() 
    { 
        Debug.Log("Keluar dari Aplikasi...");
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (audioSourceMenu != null && suaraKlikTombol != null)
        {
            audioSourceMenu.PlayOneShot(suaraKlikTombol);
        }
    }

    private void OnDestroy()
    {
        if (mainBtn != null) mainBtn.onClick.RemoveListener(PlayGame);
        if (settingsBtn != null) settingsBtn.onClick.RemoveListener(OpenSettings);
        if (aboutUsBtn != null) aboutUsBtn.onClick.RemoveListener(OpenAboutUs);
        if (panduanBtn != null) panduanBtn.onClick.RemoveListener(OpenPanduan);
        if (keluarBtn != null) keluarBtn.onClick.RemoveListener(QuitGame);
        if (closeSettingsBtn != null) closeSettingsBtn.onClick.RemoveListener(CloseSettings);
        if (closeAboutUsBtn != null) closeAboutUsBtn.onClick.RemoveListener(CloseAboutUs);
        if (closePanduanBtn != null) closePanduanBtn.onClick.RemoveListener(ClosePanduan);
        if (restartBtn != null) restartBtn.onClick.RemoveListener(RestartGameData);
        if (musikSlider != null) musikSlider.onValueChanged.RemoveListener(SetMusikVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}