using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public static EndingManager instance;

    [Header("Delay Settings")]
    [Tooltip("Jeda waktu setelah close reward diklik sampai panel ending mulai muncul")]
    public float delaySebelumEnding = 2f;

    [Header("UI Panels & Elements")]
    public GameObject panelEndingUtama;
    public CanvasGroup canvasGroupBlur; 
    public RectTransform rectAmplopTutup;
    public Button btnBukaAmplop;
    public GameObject visualAmplopBuka;
    public Button btnKeHome;
    public CanvasGroup canvasGroupBoomPutih; 

    [Header("Animation Settings")]
    public float durasiAmplopNaik = 1.5f;
    public float durasiBlurFadeIn = 1.0f;
    public float durasiGetar = 1.0f;
    public float kekuatanGetar = 10f;
    public Vector2 posisiStartAmplop = new Vector2(0, -1000); 
    public Vector2 posisiEndAmplop = new Vector2(0, 0);       

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip suaraAmplopDatang;  // 🔥 Slot sound amplop-dtg
    public AudioClip suaraAmplopSampai;  // 🔥 Slot sound ketika sudah muncul menetap di tengah
    public AudioClip suaraKlikBukaAmplop;
    public AudioClip suaraDrumrollGetar; // 🔥 Slot sound getar (drumroll) dari kamu
    public AudioClip suaraKlikHome;

    [Header("Scene Settings")]
    public string namaSceneHome = "HomeScene"; 

    private Vector2 posisiAwalAmplopAsli;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panelEndingUtama != null) panelEndingUtama.SetActive(false);
        if (canvasGroupBlur != null) canvasGroupBlur.alpha = 0f;
        if (btnBukaAmplop != null) btnBukaAmplop.gameObject.SetActive(false);
        if (visualAmplopBuka != null) visualAmplopBuka.SetActive(false);
        if (btnKeHome != null) btnKeHome.gameObject.SetActive(false);
        if (canvasGroupBoomPutih != null) {
            canvasGroupBoomPutih.gameObject.SetActive(false);
            canvasGroupBoomPutih.alpha = 0f;
        }

        if (rectAmplopTutup != null) posisiAwalAmplopAsli = rectAmplopTutup.anchoredPosition;
        
        if (btnBukaAmplop != null) btnBukaAmplop.onClick.AddListener(KlikBukaAmplop);
        if (btnKeHome != null) btnKeHome.onClick.AddListener(KlikKeHome);
    }

    public void MulaiSequenceEnding()
    {
        StartCoroutine(SequenceEndingCoroutine());
    }

    private IEnumerator SequenceEndingCoroutine()
    {
        yield return new WaitForSeconds(delaySebelumEnding);

        // 🔥 TAMBAHAN: Tutup HUD Utama saat sekuens ending dimulai
        if (IntroStoryManager.instance != null && IntroStoryManager.instance.hudGameplay != null)
        {
            IntroStoryManager.instance.hudGameplay.SetActive(false);
        }

        if (panelEndingUtama != null) panelEndingUtama.SetActive(true);
        if (rectAmplopTutup != null) rectAmplopTutup.anchoredPosition = posisiStartAmplop;

        if (audioSource != null && suaraAmplopDatang != null)
            audioSource.PlayOneShot(suaraAmplopDatang);

        float elapsed = 0f;
        float durasiMaksimal = Mathf.Max(durasiAmplopNaik, durasiBlurFadeIn);

        while (elapsed < durasiMaksimal)
        {
            elapsed += Time.deltaTime;

            if (rectAmplopTutup != null && durasiAmplopNaik > 0)
            {
                float tAmplop = Mathf.Clamp01(elapsed / durasiAmplopNaik);
                rectAmplopTutup.anchoredPosition = Vector2.Lerp(posisiStartAmplop, posisiEndAmplop, Mathf.SmoothStep(0, 1, tAmplop));
            }

            if (canvasGroupBlur != null && durasiBlurFadeIn > 0)
            {
                float tBlur = Mathf.Clamp01(elapsed / durasiBlurFadeIn);
                canvasGroupBlur.alpha = tBlur;
            }

            yield return null;
        }

        if (rectAmplopTutup != null) rectAmplopTutup.anchoredPosition = posisiEndAmplop;
        if (canvasGroupBlur != null) canvasGroupBlur.alpha = 1f;

        if (btnBukaAmplop != null) btnBukaAmplop.gameObject.SetActive(true);
    }

    private void KlikKeHome()
    {
        if (btnKeHome != null) btnKeHome.interactable = false;

        if (audioSource != null && suaraKlikHome != null)
            audioSource.PlayOneShot(suaraKlikHome);

        // 🔥 TAMBAHAN: AUTO RESET SELURUH HISTORY PERMAINAN KARENA GAME SUDAH TAMAT
        Debug.Log("<color=red>[Ending Manager]</color> Game Selesai! Membersihkan data penyimpanan agar aman dimainkan kembali...");
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();

        StartCoroutine(PindahSceneCoroutine());
    }

    private IEnumerator GetarDanBoomCoroutine()
    {
        // Play sound drumroll saat amplop mulai bergetar dahsyat
        if (audioSource != null && suaraDrumrollGetar != null)
            audioSource.PlayOneShot(suaraDrumrollGetar);

        float elapsed = 0f;
        while (elapsed < durasiGetar)
        {
            elapsed += Time.deltaTime;
            if (rectAmplopTutup != null)
            {
                float offsetX = Random.Range(-kekuatanGetar, kekuatanGetar);
                float offsetY = Random.Range(-kekuatanGetar, kekuatanGetar);
                rectAmplopTutup.anchoredPosition = posisiEndAmplop + new Vector2(offsetX, offsetY);
            }
            yield return null;
        }
        if (rectAmplopTutup != null) rectAmplopTutup.anchoredPosition = posisiEndAmplop;

        // Transisi ke flash putih instan cepat
        if (canvasGroupBoomPutih != null)
        {
            canvasGroupBoomPutih.gameObject.SetActive(true);
            
            float flashIn = 0f;
            while (flashIn < 0.1f)
            {
                flashIn += Time.deltaTime;
                canvasGroupBoomPutih.alpha = Mathf.Clamp01(flashIn / 0.1f);
                yield return null;
            }
        }

        // --- MOMEN LAYAR FULL PUTIH ---
        if (rectAmplopTutup != null) rectAmplopTutup.gameObject.SetActive(false);
        if (btnBukaAmplop != null) btnBukaAmplop.gameObject.SetActive(false);
        
        if (visualAmplopBuka != null) visualAmplopBuka.SetActive(true);
        if (btnKeHome != null) btnKeHome.gameObject.SetActive(true);

        // 🔥 TAMBAHAN: Mainkan suara amplop sampai / sound amplop-buka tepat di sini!
        if (audioSource != null && suaraAmplopSampai != null)
            audioSource.PlayOneShot(suaraAmplopSampai);

        // Layar putih menghilang kembali secara cepat (Fade-out kilat)
        if (canvasGroupBoomPutih != null)
        {
            float flashOut = 0f;
            while (flashOut < 0.2f)
            {
                flashOut += Time.deltaTime;
                canvasGroupBoomPutih.alpha = 1f - Mathf.Clamp01(flashOut / 0.2f);
                yield return null;
            }
            canvasGroupBoomPutih.gameObject.SetActive(false);
        }
    }

    private void KlikBukaAmplop()
    {
        if (btnBukaAmplop != null) btnBukaAmplop.interactable = false;

        if (audioSource != null && suaraKlikBukaAmplop != null)
            audioSource.PlayOneShot(suaraKlikBukaAmplop);

        StartCoroutine(GetarDanBoomCoroutine());
    }

    private IEnumerator PindahSceneCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(namaSceneHome);
    }
}