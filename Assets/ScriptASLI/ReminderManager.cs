using System.Collections;
using UnityEngine;
using TMPro;

public class ReminderManager : MonoBehaviour
{
    public static ReminderManager instance;

    [Header("UI Components")]
    [SerializeField] private RectTransform kakekTransform; 
    [SerializeField] private GameObject bubbleObject;       
    [SerializeField] private TextMeshProUGUI textMessage;   

    [Header("Animation Settings")]
    [SerializeField] private float speedKakek = 500f;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float durationVisible = 4f;    
    [SerializeField] private float loopReminderDelay = 10f; 

    private Vector2 kakekHiddenPos;
    private Vector2 kakekShownPos;
    private bool isReminderActive = false;
    private Coroutine activeReminderRoutine;
    private string pesanPakanHabis = "Pakan di peternakan sudah habis! Tolong diisi ulang.";

    // VARIABEL REVISI TUTORIAL BERANAK TERNAK
    private bool hasShownTutorialTernak = false;
    private string pesanTutorial1 = "Sapi dan kambing yang kamu beli ini bisa beranak loh.";
    private string pesanTutorial2 = "Maka dari itu rajinlah memberinya makan agar ia tumbuh sehat.";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        kakekShownPos = kakekTransform.anchoredPosition;
        kakekHiddenPos = new Vector2(kakekShownPos.x, -kakekTransform.rect.height - 100f);
        
        kakekTransform.anchoredPosition = kakekHiddenPos;
        bubbleObject.SetActive(false);
        textMessage.text = "";
    }

    // Fungsi pemicu looping reminder pakan habis
    public void TriggerPakanHabisReminder(bool status)
    {
        // 🛑 LAYER PROTEKSI: Jika ternak sudah mencapai nisab / disuruh ke kantor zakat, BLOKIR reminder pakan
        if (JurnalManager.instance != null && JurnalManager.instance.isTernakNisabReached)
        {
            status = false; 
        }

        if (status)
        {
            if (!isReminderActive)
            {
                isReminderActive = true;
                activeReminderRoutine = StartCoroutine(ReminderLoopSequence());
            }
        }
        else
        {
            isReminderActive = false;
            if (activeReminderRoutine != null) StopCoroutine(activeReminderRoutine);
            StartCoroutine(HideSequence());
        }
    }

    private IEnumerator ReminderLoopSequence()
    {
        while (isReminderActive)
        {
            // 🛑 CEK REAL-TIME: Jika tiba-tiba di tengah jalan nisab tercapai, langsung stop kakek keluar
            if (JurnalManager.instance != null && JurnalManager.instance.isTernakNisabReached)
            {
                isReminderActive = false;
                StartCoroutine(HideSequence());
                yield break;
            }

            while (IsAnyPanelOpen()) yield return null;

            yield return StartCoroutine(ShowSequence());
            yield return new WaitForSeconds(durationVisible);
            yield return StartCoroutine(HideSequence());
            yield return new WaitForSeconds(loopReminderDelay);
        }
    }

    private IEnumerator ShowSequence()
    {
        textMessage.text = "";
        bubbleObject.SetActive(false);

        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekShownPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekShownPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        // 🔥 FIX: Mengubah dari kShownPos menjadi kakekShownPos agar sesuai deklarasi
        kakekTransform.anchoredPosition = kakekShownPos;

        bubbleObject.SetActive(true);

        textMessage.text = "."; yield return new WaitForSeconds(0.4f);
        textMessage.text = ".."; yield return new WaitForSeconds(0.4f);
        textMessage.text = "..."; yield return new WaitForSeconds(0.5f);
        textMessage.text = ""; yield return new WaitForSeconds(0.2f);

        foreach (char letter in pesanPakanHabis.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private IEnumerator HideSequence()
    {
        bubbleObject.SetActive(false);
        textMessage.text = "";

        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekHiddenPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekHiddenPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekHiddenPos;
    }

    // ====================================================================
    // 🔥 TUTORIAL BERANAK (DIPANGGIL SETELAH BELI TERNAK / TOKO TUTUP)
    // ====================================================================
    public void TriggerFirstPurchaseTutorial()
    {
        if (!hasShownTutorialTernak)
        {
            hasShownTutorialTernak = true;
            StartCoroutine(TutorialTernakSequence());
        }
    }

    private IEnumerator TutorialTernakSequence()
    {
        while (IsAnyPanelOpen()) yield return null;
        yield return new WaitForSeconds(0.2f); 

        textMessage.text = "";
        bubbleObject.SetActive(false);

        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekShownPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekShownPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekShownPos;

        bubbleObject.SetActive(true);

        textMessage.text = "."; yield return new WaitForSeconds(0.3f); 
        textMessage.text = ".."; yield return new WaitForSeconds(0.3f);
        textMessage.text = "..."; yield return new WaitForSeconds(0.4f); 
        textMessage.text = ""; yield return new WaitForSeconds(0.1f);

        foreach (char letter in pesanTutorial1.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        yield return new WaitForSeconds(durationVisible * 0.7f); 

        bubbleObject.SetActive(false);
        textMessage.text = "";
        yield return new WaitForSeconds(0.3f); 

        while (IsAnyPanelOpen()) yield return null;

        bubbleObject.SetActive(true);

        foreach (char letter in pesanTutorial2.ToCharArray())
        {
            textMessage.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        yield return new WaitForSeconds(durationVisible);

        bubbleObject.SetActive(false);
        textMessage.text = "";
        while (Vector2.Distance(kakekTransform.anchoredPosition, kakekHiddenPos) > 0.1f)
        {
            kakekTransform.anchoredPosition = Vector2.MoveTowards(kakekTransform.anchoredPosition, kakekHiddenPos, speedKakek * Time.deltaTime);
            yield return null;
        }
        kakekTransform.anchoredPosition = kakekHiddenPos;
    }

    private bool IsAnyPanelOpen()
    {
        bool tokoBuka = false;
        if (TokoManager.instance != null)
        {
            GameObject masterToko = (GameObject)System.Type.GetType("TokoManager")
                .GetField("masterTokoPanelUtama", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(TokoManager.instance);

            if (masterToko != null) tokoBuka = masterToko.activeSelf;
            else if (TokoManager.instance.mainTokoPanel != null) tokoBuka = TokoManager.instance.mainTokoPanel.activeSelf;
        }

        bool jurnalBuka = (JurnalManager.instance != null && JurnalManager.instance.jurnalContent.activeSelf);
        bool misiBuka = (TaskManager.instance != null && TaskManager.instance.misiPanel.activeSelf);
        bool carouselZakatBuka = (ZakatPanelManager.instance != null && ZakatPanelManager.instance.zakatCarouselPanel.activeSelf);

        return tokoBuka || jurnalBuka || misiBuka || carouselZakatBuka;
    }
}