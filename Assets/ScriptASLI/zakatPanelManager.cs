using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZakatPanelManager : MonoBehaviour
{
    public static ZakatPanelManager instance;

    [Header("UI References")]
    public GameObject zakatCarouselPanel;
    public GameObject asetBlur;
    public RectTransform content;
    public RectTransform viewPort;

    [Header("Items")]
    public RectTransform element0;
    public RectTransform element1;
    public RectTransform element2;
    public RectTransform element3;
    public RectTransform element4;
    
    [Header("Navigation Buttons")]
    public Button btnNext;
    public Button btnPrevious;
    public Button btnClose;

    [Header("Tombol Bayar (Objek 3D)")]
    public GameObject btnBayarZakat; 

    [Header("Carousel Settings")]
    public float transitionSpeed = 10f;
    public float centerScale = 1.2f;
    public float sideScale = 0.5f;
    public float itemSpacing = 500f; 

    private int currentIndex = 2;
    private Vector2 targetPos;

    [Header("Status Locking (Data Only)")]
    public bool isPerdaganganUnlocked = false;
    public bool isEmasPerakUnlocked = false;
    public bool isPeternakanUnlocked = false;

    // 🔥 TAMBAHAN BARU: Status tracker apakah zakat sudah pernah diselesaikan/diisi
    [Header("Status Completion")]
    public bool isPerdaganganCompleted = false;
    public bool isEmasPerakCompleted = false;
    public bool isPeternakanCompleted = false;

    // 🔥 TAMBAHAN BARU: Tarik Game Object Image Centang dari Inspector Unity ke sini
    [Header("UI Centang / Checkmark Objects")]
    public GameObject checkmarkPerdagangan;
    public GameObject checkmarkEmasPerak;
    public GameObject checkmarkPeternakan;

    [Header("Mapping Jurnal -> Item")]
    public int indexPerdagangan = 2;
    public int indexEmasPerak = 1;
    public int indexPeternakan = 3;

    [Header("External")]
    public JurnalManager jurnalManager;
    
    private bool isMoving = false;
    private RectTransform[] items;
    private Coroutine movementCoroutine;

    void Awake()
    {   
        instance = this;
        items = new RectTransform[] {
            element0,
            element1,
            element2,
            element3,
            element4
        };
        if (zakatCarouselPanel != null) zakatCarouselPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners(); 
            btnClose.onClick.AddListener(CloseZakatPanel); 
        }
    }

    public void OpenZakatPanel()
    {
        Debug.Log("[ZakatPanel] Membuka panel zakat.");
        if (UIManager.instance != null) UIManager.instance.OpenPanelMenu(zakatCarouselPanel);
        else zakatCarouselPanel.SetActive(true);

        if (asetBlur != null) asetBlur.SetActive(true);

        currentIndex = 2; 
        isMoving = false; 

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        RepositionItemsDynamically();
        UpdateTargetPosition(true);
        UpdateNavButtons();
        UpdatePaymentButtonVisual(); 
        UpdateCheckmarkVisuals(); // 🔥 Sinkronisasi tanda centang saat dibuka
    }

    public void CloseZakatPanel()
    {
        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(zakatCarouselPanel);
        else zakatCarouselPanel.SetActive(false);

        if (asetBlur != null) asetBlur.SetActive(false);
    }

    void RepositionItemsDynamically()
    {
        if (items == null || items.Length == 0) return;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                float calculatedX = (i - 2) * itemSpacing;
                items[i].localPosition = new Vector3(calculatedX, items[i].localPosition.y, items[i].localPosition.z);
            }
        }
    }

    public void NextItem()
    {
        if (isMoving) return;
        if (currentIndex < 3)
        {
            currentIndex++;
            UpdateNavButtons();
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine());
        }
    }

    public void PreviousItem()
    {
        if (isMoving) return;
        if (currentIndex > 1)
        {
            currentIndex--;
            UpdateNavButtons();
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine());
        }
    }

    IEnumerator MoveToTargetRoutine()
    {
        isMoving = true;
        Vector2 startPos = content.anchoredPosition;
        UpdateTargetPosition(false);

        float elapsed = 0f;
        float duration = 0.3f; 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothPercent);
            HandleScaling(); 
            yield return null;
        }

        content.anchoredPosition = targetPos;
        HandleScaling(); 
        isMoving = false;
        UpdatePaymentButtonVisual();
    }

    void UpdateNavButtons()
    {
        if (btnPrevious != null) btnPrevious.gameObject.SetActive(currentIndex > 1);
        if (btnNext != null) btnNext.gameObject.SetActive(currentIndex < 3);
    }

    void UpdateTargetPosition(bool instant)
    {
        if (items.Length == 0 || items[currentIndex] == null) return;
        float targetX = -items[currentIndex].localPosition.x;
        targetPos = new Vector2(targetX, content.anchoredPosition.y);

        if (instant) {
            content.anchoredPosition = targetPos;
            HandleScaling(); 
        }
    }

    void LateUpdate()
    {
        if (!zakatCarouselPanel.activeSelf) return;
        HandleScaling();
    }

    // =================================================================
    // 🔥 PERBAIKAN LOGIKA WARNA (GELAP JIKA KUNCI / SUDAH SELESAI)
    // =================================================================
    void HandleScaling()
    {
        if (viewPort == null || items == null) return;
        float maxDistance = viewPort.rect.width / 2f;

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform item = items[i];
            if (item == null) continue;

            float itemPosX = item.localPosition.x + content.anchoredPosition.x;
            float distance = Mathf.Abs(itemPosX);
            float t = Mathf.Clamp01(distance / maxDistance);
            float targetScale = Mathf.Lerp(centerScale, sideScale, t);

            item.localScale = Vector3.Lerp(item.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);
            
            Image img = item.GetComponent<Image>();
            if (img == null) img = item.GetComponentInChildren<Image>();
            
            if (img != null)
            {
                Color baseColor = Color.white;

                // Cek status masing-masing halaman carousel
                if (i == indexPerdagangan)
                {
                    bool unlocked = (jurnalManager != null ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked);
                    // 🔥 MODIFIKASI: Jika terkunci ATAU sudah komplit, buat warnanya hitam/gelap
                    if (!unlocked || isPerdaganganCompleted) baseColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                    else baseColor = Color.white;
                }
                else if (i == indexEmasPerak)
                {
                    bool unlocked = (jurnalManager != null && jurnalManager.IsEmasPerakUnlocked());
                    // 🔥 MODIFIKASI: Jika terkunci ATAU sudah komplit, buat warnanya hitam/gelap
                    if (!unlocked || isEmasPerakCompleted) baseColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                    else baseColor = Color.white;
                }
                else if (i == indexPeternakan)
                {
                    bool unlocked = (jurnalManager != null && jurnalManager.IsPeternakanUnlocked());
                    // 🔥 MODIFIKASI: Jika terkunci ATAU sudah komplit, buat warnanya hitam/gelap
                    if (!unlocked || isPeternakanCompleted) baseColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                    else baseColor = Color.white;
                }

                if (i == 0 || i == 4) baseColor.a = 0f; 
                else baseColor.a = 1f;

                img.color = baseColor;
            }
        }
    }

    public void UpdateCheckmarkVisuals()
    {
        if (checkmarkPerdagangan != null) checkmarkPerdagangan.SetActive(isPerdaganganCompleted);
        if (checkmarkEmasPerak != null) checkmarkEmasPerak.SetActive(isEmasPerakCompleted);
        if (checkmarkPeternakan != null) checkmarkPeternakan.SetActive(isPeternakanCompleted);

        // Paksa sistem untuk langsung merubah warna panel (menghitam/terang) saat ini juga
        HandleScaling(); 
    }

    public void UpdatePaymentButtonVisual()
    {
        if (btnBayarZakat == null) return;

        bool currentUnlocked = false;
        bool currentCompleted = false;

        if (currentIndex == indexPerdagangan)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked;
            currentCompleted = isPerdaganganCompleted;
        }
        else if (currentIndex == indexEmasPerak)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsEmasPerakUnlocked() : isEmasPerakUnlocked;
            currentCompleted = isEmasPerakCompleted;
        } 
        else if (currentIndex == indexPeternakan)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPeternakanUnlocked() : isPeternakanUnlocked;
            currentCompleted = isPeternakanCompleted;
        }

        Renderer objRenderer = btnBayarZakat.GetComponent<Renderer>();
        if (objRenderer == null) objRenderer = btnBayarZakat.GetComponentInChildren<Renderer>();

        if (objRenderer != null)
        {
            // Tombol 3D menyala putih HANYA jika sudah unlocked DAN belum dikerjakan (belum completed)
            objRenderer.material.color = (currentUnlocked && !currentCompleted) ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void TriggerZakatAction()
    {
        bool currentUnlocked = false;
        bool currentCompleted = false;

        if (currentIndex == indexPerdagangan)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked;
            currentCompleted = isPerdaganganCompleted;
        }
        else if (currentIndex == indexEmasPerak)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsEmasPerakUnlocked() : isEmasPerakUnlocked;
            currentCompleted = isEmasPerakCompleted;
        }
        else if (currentIndex == indexPeternakan)
        {
            currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPeternakanUnlocked() : isPeternakanUnlocked;
            currentCompleted = isPeternakanCompleted;
        }

        // 🔥 MODIFIKASI: Kunci akses jika sudah pernah diselesaikan
        if (currentCompleted)
        {
            Debug.Log("[ZakatPanel] Kamu sudah menunaikan zakat ini!");
            return;
        }

        if (currentUnlocked)
        {
            if (currentIndex == indexPerdagangan) Debug.Log("[ZakatPanel] Membuka Panel Zakat Perdagangan...");
        }
        else
        {
            Debug.Log("[ZakatPanel] Maaf, selesaikan dulu kriteria syarat atau tingkatan level zakat.");
        }
    }

    public int GetCurrentIndex() { return currentIndex; }
    public void UpdateItemVisuals() { UpdatePaymentButtonVisual(); UpdateCheckmarkVisuals(); }
    public void UpdatePaymentButton() { UpdatePaymentButtonVisual(); }
}