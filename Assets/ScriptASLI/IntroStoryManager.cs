using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroStoryManager : MonoBehaviour
{
    public static IntroStoryManager instance;

    [Header("UI Panels (Urutan Sesuai Hierarchy)")]
    [Tooltip("Elemen 0-6 = Panel 10-16")]
    public GameObject[] introPanels; 
    public GameObject panel17;
    public GameObject panel18;
    public GameObject panel19;
    public GameObject panelLevel1; // Panel khusus Level 1 (punya tombol X)

    [Header("UI Tambahan (HUD Game)")]
    [Tooltip("Tarik objek Parent HUD / UI Utama game yang berisi peta, tombol tas, hp, dll ke sini")]
    public GameObject hudGameplay; 

    [Header("Visual Efek Transisi")]
    [Tooltip("Tarik objek Image Kabut/Mist Putih ke sini")]
    public Image imgMistPutih;
    [Tooltip("Berapa lama kabut putih akan menghilang secara perlahan (Fade Out)")]
    public float durasiMistFadeOut = 2.5f;

    [Header("Text Components")]
    public TextMeshProUGUI[] txtPanels; // TMP Text Panel 10-16
    public TextMeshProUGUI txtPanel17;
    public TextMeshProUGUI txtPanel18;
    public TextMeshProUGUI txtPanel19;

    [Header("Strings Cerita")]
    [TextArea(3, 10)] public string[] teksCeritaIntro; // Isi Panel 10-16
    [TextArea(3, 10)] public string teksPanel17;
    [TextArea(3, 10)] public string teksPanel18;
    [TextArea(3, 10)] public string teksPanel19;

    [Header("Navigation Buttons")]
    public Button btnNextGlobal;       // Tombol Next yang dipakai bergantian
    public Button btnXPanelLevel1;     // Tombol X khusus di Panel Level 1

    [Header("Settings")]
    public float kecepatanKetik = 0.05f;

    private int currentPanelIndex = 0;
    private Coroutine typewriterCoroutine;
    private bool sedangMengetIK = false;
    private bool introSelesai = false;
    
    private enum StoryState { Intro10_16, SelesaiTebang17, SelesaiJual18_19 }
    private StoryState currentState = StoryState.Intro10_16;

    void Awake()
{
    instance = this;

    // #if UNITY_EDITOR
    // // 🔥 TAMBAHAN: Paksa reset flag restart ke 1 agar TaskManager membaca dari awal saat play di editor
    // PlayerPrefs.SetInt("IsRestarted", 1);
    // PlayerPrefs.SetInt("IntroSelesai", 0);
    // PlayerPrefs.DeleteKey("Panel17Selesai");
    // PlayerPrefs.DeleteKey("Panel18Selesai");
    // PlayerPrefs.Save();
    // #endif
}

    void Start()
    {
        if (btnNextGlobal != null) btnNextGlobal.onClick.AddListener(OnBtnNextClicked);
        if (btnXPanelLevel1 != null) btnXPanelLevel1.onClick.AddListener(TutupPanelLevel1);

        // 🔥 FIX UTAMA 1: Paksa UIManager mereset hitungannya ke 0 murni agar HUD tidak terkunci mati
        if (UIManager.instance != null)
        {
            typeof(UIManager).GetField("openedPanelsCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(UIManager.instance, 0);
        }

        // 2. Matikan semua panel awal cerita secara bersih di awal frame
        MatikanSemuaPanelAwal();

        // 🔥 FIX UTAMA 2: Cek apakah game dimulai dari awal murni (Fresh Player atau Hasil Klik Restart)
        if (PlayerPrefs.GetInt("IntroSelesai", 0) == 0)
        {
            currentState = StoryState.Intro10_16;
            currentPanelIndex = 0; // Mulai wajib dari Panel Index ke-0 (Panel 10)

            // Matikan HUD utama agar fokus menikmati sekuens intro
            ToggleHUD(false);
            
            if (imgMistPutih != null)
            {
                imgMistPutih.gameObject.SetActive(true);
                imgMistPutih.color = new Color(1f, 1f, 1f, 1f);
            }
            
            // Panggil fungsi untuk mulai menampilkan ketikan cerita panel 10 murni
            MulaiIntroCerita();
        }
        else
        {
            // Jika meload save game biasa, kabut dimatikan dan HUD dinyalakan langsung
            if (imgMistPutih != null) imgMistPutih.gameObject.SetActive(false);
            
            if(btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);
            if(panelLevel1 != null) panelLevel1.SetActive(false);
            ToggleHUD(true);
        }
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Pointer.current != null && 
            UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame && 
            !introSelesai)
        {
            if (sedangMengetIK)
            {
                SkipKetikkan();
            }
        }
    }

    private void MatikanSemuaPanelAwal()
    {
        for (int i = 0; i < introPanels.Length; i++)
        {
            if (introPanels[i] != null) introPanels[i].SetActive(false);
        }
        if (panel17 != null) panel17.SetActive(false);
        if (panel18 != null) panel18.SetActive(false);
        if (panel19 != null) panel19.SetActive(false);
        if (panelLevel1 != null) panelLevel1.SetActive(false);
        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);
    }

    void MulaiIntroCerita()
    {
        currentPanelIndex = 0;
        AktivasiPanelIntro(currentPanelIndex);
        
        // 🔥 Mulai proses menghilangnya kabut secara perlahan berbarengan dengan munculnya panel 10
        if (imgMistPutih != null && imgMistPutih.gameObject.activeSelf)
        {
            StartCoroutine(EfekMistFadeOut());
        }
    }

    // Coroutine pengontrol hilangnya kabut putih secara berkala (Slow Fade Out)
    private IEnumerator EfekMistFadeOut()
    {
        float timer = 0f;
        Color warnaAsli = imgMistPutih.color;

        while (timer < durasiMistFadeOut)
        {
            timer += Time.deltaTime;
            float progressAlpha = Mathf.Lerp(1f, 0f, timer / durasiMistFadeOut);
            
            // Ubah nilai Alpha (transparansi) secara lambat
            imgMistPutih.color = new Color(warnaAsli.r, warnaAsli.g, warnaAsli.b, progressAlpha);
            yield return null;
        }

        // Matikan objek gambar kabut setelah benar-benar transparan agar klik tidak terhalang
        imgMistPutih.gameObject.SetActive(false);
    }

    void AktivasiPanelIntro(int index)
    {
        if (index >= introPanels.Length) return;

        if (index > 0 && introPanels[index - 1] != null) introPanels[index - 1].SetActive(false);
        if (introPanels[index] != null) introPanels[index].SetActive(true);

        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanels[index], teksCeritaIntro[index]));
    }

    // --- PEMICU DARI LUAR DENGAN JEDA 1.5 DETIK (EXTERNAL TRIGGERS) ---

    public void TriggerPanel17SelesaiTebang()
    {
        StartCoroutine(JedaMunculPanel17());
    }

    private IEnumerator JedaMunculPanel17()
    {
        yield return new WaitForSeconds(1.5f);
        TutupSemuaPanelGameplayLain();
        ToggleHUD(false);

        currentState = StoryState.SelesaiTebang17;
        if (panel17 != null) panel17.SetActive(true);
        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanel17, teksPanel17));
    }

    public void TriggerPanel18_19SelesaiJual()
    {
        StartCoroutine(JedaMunculPanel18_19());
    }

    private IEnumerator JedaMunculPanel18_19()
    {
        yield return new WaitForSeconds(1.5f);
        TutupSemuaPanelGameplayLain();
        ToggleHUD(false);

        currentState = StoryState.SelesaiJual18_19;
        currentPanelIndex = 18; 
        if (panel18 != null) panel18.SetActive(true);
        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanel18, teksPanel18));
    }

    IEnumerator TypewriterAdvanced(TextMeshProUGUI tmpText, string teksLengkap)
    {
        if (tmpText == null) yield break;
        
        sedangMengetIK = true;
        for (int i = 0; i <= teksLengkap.Length; i++)
        {
            string teksTampil = teksLengkap.Substring(0, i);
            string teksSembunyi = teksLengkap.Substring(i);
            tmpText.text = teksTampil + "<color=#00000000>" + teksSembunyi + "</color>";
            yield return new WaitForSeconds(kecepatanKetik);
        }
        tmpText.text = teksLengkap;
        sedangMengetIK = false;
        
        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(true);
    }

    void SkipKetikkan()
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        sedangMengetIK = false;

        if (currentState == StoryState.Intro10_16)
        {
            if (txtPanels[currentPanelIndex] != null) txtPanels[currentPanelIndex].text = teksCeritaIntro[currentPanelIndex];
        }
        else if (currentState == StoryState.SelesaiTebang17)
        {
            if (txtPanel17 != null) txtPanel17.text = teksPanel17;
        }
        else if (currentState == StoryState.SelesaiJual18_19)
        {
            if (currentPanelIndex == 18 && txtPanel18 != null) txtPanel18.text = teksPanel18;
            if (currentPanelIndex == 19 && txtPanel19 != null) txtPanel19.text = teksPanel19;
        }

        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(true);
    }

    public void OnBtnNextClicked()
    {
        if (sedangMengetIK) return;

        if (currentState == StoryState.Intro10_16)
        {
            if (currentPanelIndex < introPanels.Length - 1)
            {
                currentPanelIndex++;
                AktivasiPanelIntro(currentPanelIndex);
            }
            else
            {
                PlayerPrefs.SetInt("IntroSelesai", 1);
                PlayerPrefs.Save();
                MasukKeGameplaySementata();
            }
        }
        else if (currentState == StoryState.SelesaiTebang17)
        {
            if (panel17 != null) panel17.SetActive(false);
            if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);
            ToggleHUD(true); 
        }
        else if (currentState == StoryState.SelesaiJual18_19)
        {
            if (currentPanelIndex == 18)
            {
                currentPanelIndex = 19;
                if (panel18 != null) panel18.SetActive(false);
                if (panel19 != null) panel19.SetActive(true);
                if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);

                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanel19, teksPanel19));
            }
            else if (currentPanelIndex == 19)
            {
                if (panel19 != null) panel19.SetActive(false);
                if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);

                if (panelLevel1 != null) panelLevel1.SetActive(true); 
            }
        }
    }

    void MasukKeGameplaySementata()
    {
        if (introPanels[introPanels.Length - 1] != null) introPanels[introPanels.Length - 1].SetActive(false);
        if (btnNextGlobal != null) btnNextGlobal.gameObject.SetActive(false);
        
        // Paksa aktifkan HUD utama game
        ToggleHUD(true); 

        // Amankan UIManager agar tidak mematikan HUD secara sepihak
        if (UIManager.instance != null)
        {
            // Jika gameplay baru mulai murni, pastikan status panel terbuka di-reset ke 0
            // Kamu bisa menambahkan variabel helper atau fungsi ResetCounter() di UIManager jika diperlukan
        }

        if (TaskManager.instance != null && TaskManager.instance.ikonNotifikasi != null)
        {
            TaskManager.instance.ikonNotifikasi.SetActive(true);
        }
    }

    void TutupPanelLevel1()
    {
        if (panelLevel1 != null) panelLevel1.SetActive(false);
        ToggleHUD(true); 
    }

    private void ToggleHUD(bool status)
    {
        if (hudGameplay != null) hudGameplay.SetActive(status);
    }

    private void TutupSemuaPanelGameplayLain()
    {
        if (TaskManager.instance != null) TaskManager.instance.CloseMisi();
        if (InventoryManager.instance != null && InventoryManager.instance.inventoryPanel.activeSelf) 
        {
            InventoryManager.instance.ToggleInventory();
        }
    }
}