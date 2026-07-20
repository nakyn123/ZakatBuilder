using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroStoryManager : MonoBehaviour
{
    public static IntroStoryManager instance;

    [Header("UI Panels (Urutan Sesuai Hierarchy)")]
    [Tooltip("Elemen 0-5 = Panel 10, 11, 12, 13, 15, 16 (Panel 14 & 19 dihapus)")]
    public GameObject[] introPanels; 
    public GameObject panel17;
    public GameObject panel18;
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
    [Tooltip("TMP Text untuk Panel 10, 11, 12, 13, 15, 16")]
    public TextMeshProUGUI[] txtPanels; 
    public TextMeshProUGUI txtPanel17;
    public TextMeshProUGUI txtPanel18;

    [Header("Strings Cerita")]
    [TextArea(3, 10)] [Tooltip("Isi cerita untuk Panel 10, 11, 12, 13, 15, 16")] public string[] teksCeritaIntro; 
    [TextArea(3, 10)] public string teksPanel17;
    [TextArea(3, 10)] public string teksPanel18;

    [Header("Navigation Buttons")]
    public Button btnNextUmum;         // Tombol Next untuk panel 10 sampai 15
    public Button btnNext16;           // Tombol Next khusus Panel 16
    public Button btnNext17;           // Tombol Next khusus Panel 17
    public Button btnNext18;           // Tombol Next khusus Panel 18
    public Button btnXPanelLevel1;     // Tombol X khusus di Panel Level 1

    [Header("Settings")]
    public float kecepatanKetik = 0.05f;

    private int currentPanelIndex = 0;
    private Coroutine typewriterCoroutine;
    private bool sedangMengetIK = false;
    private bool introSelesai = false;

    [Header("Audio Typewriter Settings")]
    [Tooltip("Masukkan komponen AudioSource yang ada di GameObject ini")]
    public AudioSource audioSourceDialog;
    [Tooltip("Masukkan kumpulan suara pendek untuk variasi suara NPC")]
    public AudioClip[] soundClips;
    [Tooltip("Batas minimum acak pitch (misal 0.85f)")]
    public float minPitch = 0.85f;
    [Tooltip("Batas maksimum acak pitch (misal 1.15f)")]
    public float maxPitch = 1.15f;
    [Tooltip("Suara berbunyi setiap berapa karakter? (1 = setiap huruf, 2 = setiap 2 huruf biar tidak terlalu bising)")]
    public int karakterPerBunyi = 2;
    
    private enum StoryState { Intro10_16, SelesaiTebang17, SelesaiJual18 }
    private StoryState currentState = StoryState.Intro10_16;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Setup Event Listeners Tombol
        if (btnNextUmum != null) btnNextUmum.onClick.AddListener(OnBtnNextUmumClicked);
        if (btnNext16 != null) btnNext16.onClick.AddListener(OnBtnNext16Clicked);
        if (btnNext17 != null) btnNext17.onClick.AddListener(OnBtnNext17Clicked);
        if (btnNext18 != null) btnNext18.onClick.AddListener(OnBtnNext18Clicked);
        if (btnXPanelLevel1 != null) btnXPanelLevel1.onClick.AddListener(TutupPanelLevel1);

        if (UIManager.instance != null)
        {
            typeof(UIManager).GetField("openedPanelsCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(UIManager.instance, 0);
        }

        MatikanSemuaPanelAwal();

        if (PlayerPrefs.GetInt("IntroSelesai", 0) == 0)
        {
            currentState = StoryState.Intro10_16;
            currentPanelIndex = 0; 

            ToggleHUD(false);
            
            if (imgMistPutih != null)
            {
                imgMistPutih.gameObject.SetActive(true);
                imgMistPutih.color = new Color(1f, 1f, 1f, 1f);
            }
            
            MulaiIntroCerita();
        }
        else
        {
            if (imgMistPutih != null) imgMistPutih.gameObject.SetActive(false);
            if (panelLevel1 != null) panelLevel1.SetActive(false);
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
        if (panelLevel1 != null) panelLevel1.SetActive(false);
        
        SetAktifSemuaTombolNext(false);
    }

    private void SetAktifSemuaTombolNext(bool status)
    {
        if (btnNextUmum != null) btnNextUmum.gameObject.SetActive(status);
        if (btnNext16 != null) btnNext16.gameObject.SetActive(status);
        if (btnNext17 != null) btnNext17.gameObject.SetActive(status);
        if (btnNext18 != null) btnNext18.gameObject.SetActive(status);
    }

    void MulaiIntroCerita()
    {
        currentPanelIndex = 0;
        AktivasiPanelIntro(currentPanelIndex);
        
        if (imgMistPutih != null && imgMistPutih.gameObject.activeSelf)
        {
            StartCoroutine(EfekMistFadeOut());
        }
    }

    private IEnumerator EfekMistFadeOut()
    {
        float timer = 0f;
        Color warnaAsli = imgMistPutih.color;

        while (timer < durasiMistFadeOut)
        {
            timer += Time.deltaTime;
            float progressAlpha = Mathf.Lerp(1f, 0f, timer / durasiMistFadeOut);
            imgMistPutih.color = new Color(warnaAsli.r, warnaAsli.g, warnaAsli.b, progressAlpha);
            yield return null;
        }

        imgMistPutih.gameObject.SetActive(false);
    }

    void AktivasiPanelIntro(int index)
    {
        if (index >= introPanels.Length) return;

        if (index > 0 && introPanels[index - 1] != null) introPanels[index - 1].SetActive(false);
        if (introPanels[index] != null) introPanels[index].SetActive(true);

        SetAktifSemuaTombolNext(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanels[index], teksCeritaIntro[index]));
    }

    // --- EXTERNAL TRIGGERS ---

    public void TriggerPanel17SelesaiTebang()
    {
        StartCoroutine(JedaMunculPanel17());
    }

    private IEnumerator JedaMunculPanel17()
    {
        yield return new WaitForSeconds(1f);
        TutupSemuaPanelGameplayLain();
        ToggleHUD(false);

        currentState = StoryState.SelesaiTebang17;
        if (panel17 != null) panel17.SetActive(true);
        SetAktifSemuaTombolNext(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanel17, teksPanel17));
    }

    public void TriggerPanel18SelesaiJual()
    {
        StartCoroutine(JedaMunculPanel18());
    }

    private IEnumerator JedaMunculPanel18()
    {
        yield return new WaitForSeconds(1.5f);
        TutupSemuaPanelGameplayLain();
        ToggleHUD(false);

        currentState = StoryState.SelesaiJual18;
        if (panel18 != null) panel18.SetActive(true);
        SetAktifSemuaTombolNext(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAdvanced(txtPanel18, teksPanel18));
    }

    IEnumerator TypewriterAdvanced(TextMeshProUGUI tmpText, string teksLengkap)
    {
        if (tmpText == null) yield break;
        
        sedangMengetIK = true;
        float pitchMinKarakter = 0.85f;
        float pitchMaxKarakter = 1.15f;

        // Penyesuaian pitch suara (Array: 0=P10, 1=P11, 2=P12, 3=P13, 4=P15, 5=P16)
        if (currentState == StoryState.Intro10_16 && (currentPanelIndex == 0 || currentPanelIndex == 2))
        {
            // Karakter: Laki-laki Muda (Panel 10 & Panel 12)
            pitchMinKarakter = 0.95f;
            pitchMaxKarakter = 1.20f;
        }
        else
        {
            // Karakter: Bapak-bapak Tua
            pitchMinKarakter = 0.55f;
            pitchMaxKarakter = 0.75f;
        }

        for (int i = 0; i <= teksLengkap.Length; i++)
        {
            string teksTampil = teksLengkap.Substring(0, i);
            string teksSembunyi = teksLengkap.Substring(i);
            tmpText.text = teksTampil + "<color=#00000000>" + teksSembunyi + "</color>";

            int sisaKarakter = teksLengkap.Length - i;

            if (i > 0 && i < teksLengkap.Length && i % karakterPerBunyi == 0 && sisaKarakter > 3)
            {
                char karakterSekarang = teksLengkap[i - 1];

                if (karakterSekarang != ' ' && audioSourceDialog != null && soundClips.Length > 0)
                {
                    int randomClipIndex = Random.Range(0, soundClips.Length);
                    audioSourceDialog.clip = soundClips[randomClipIndex];
                    audioSourceDialog.pitch = Random.Range(pitchMinKarakter, pitchMaxKarakter);
                    audioSourceDialog.Play();
                }
            }
            else if (sisaKarakter <= 3 && audioSourceDialog != null && audioSourceDialog.isPlaying)
            {
                audioSourceDialog.Stop();
            }

            yield return new WaitForSeconds(kecepatanKetik);
        }

        if (audioSourceDialog != null) audioSourceDialog.Stop();

        tmpText.text = teksLengkap;
        sedangMengetIK = false;
        
        MunculkanTombolSesuaiKonteks();
    }

    void SkipKetikkan()
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        sedangMengetIK = false;

        if (audioSourceDialog != null) audioSourceDialog.Stop();

        if (currentState == StoryState.Intro10_16)
        {
            if (txtPanels[currentPanelIndex] != null) txtPanels[currentPanelIndex].text = teksCeritaIntro[currentPanelIndex];
        }
        else if (currentState == StoryState.SelesaiTebang17)
        {
            if (txtPanel17 != null) txtPanel17.text = teksPanel17;
        }
        else if (currentState == StoryState.SelesaiJual18)
        {
            if (txtPanel18 != null) txtPanel18.text = teksPanel18;
        }

        MunculkanTombolSesuaiKonteks();
    }

    void MunculkanTombolSesuaiKonteks()
    {
        SetAktifSemuaTombolNext(false);

        if (currentState == StoryState.Intro10_16)
        {
            if (currentPanelIndex == introPanels.Length - 1)
            {
                if (btnNext16 != null) btnNext16.gameObject.SetActive(true);
            }
            else
            {
                if (btnNextUmum != null) btnNextUmum.gameObject.SetActive(true);
            }
        }
        else if (currentState == StoryState.SelesaiTebang17)
        {
            if (btnNext17 != null) btnNext17.gameObject.SetActive(true);
        }
        else if (currentState == StoryState.SelesaiJual18)
        {
            if (btnNext18 != null) btnNext18.gameObject.SetActive(true);
        }
    }

    // --- LOGIKA KLIK TOMBOL NEXT ---

    public void OnBtnNextUmumClicked()
    {
        if (sedangMengetIK) return;

        // Maju dari 10 -> 11 -> 12 -> 13 -> 15
        if (currentPanelIndex < introPanels.Length - 1) 
        {
            currentPanelIndex++;
            AktivasiPanelIntro(currentPanelIndex);
        }
    }

    public void OnBtnNext16Clicked()
    {
        if (sedangMengetIK) return;

        PlayerPrefs.SetInt("IntroSelesai", 1);
        PlayerPrefs.Save();
        MasukKeGameplaySementara();
    }

    public void OnBtnNext17Clicked()
    {
        if (sedangMengetIK) return;

        if (panel17 != null) panel17.SetActive(false);
        SetAktifSemuaTombolNext(false);
        ToggleHUD(true); 
    }

    public void OnBtnNext18Clicked()
    {
        if (sedangMengetIK) return;

        if (panel18 != null) panel18.SetActive(false);
        SetAktifSemuaTombolNext(false);

        // Setelah Panel 18 selesai, langsung munculkan Panel Level 1 (karena Panel 19 sudah dihapus)
        if (panelLevel1 != null) panelLevel1.SetActive(true); 
    }

    void MasukKeGameplaySementara()
    {
        if (introPanels[introPanels.Length - 1] != null) introPanels[introPanels.Length - 1].SetActive(false);
        SetAktifSemuaTombolNext(false);
        
        ToggleHUD(true); 

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