using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZakatPanelManager : MonoBehaviour
{
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
    
    // 🔥 PENGATURAN JARAK BARU VIA KODE
    [Tooltip("Jarak konstan antar item di dalam carousel")]
    public float itemSpacing = 500f; 

    private int currentIndex = 2;
    private Vector2 targetPos;

    [Header("Status Locking (Data Only)")]
    public bool isPerdaganganUnlocked = false;
    public bool isEmasPerakUnlocked = false;
    public bool isPeternakanUnlocked = false;

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
            btnClose.onClick.RemoveAllListeners(); // Bersihkan sisa event lama
            btnClose.onClick.AddListener(CloseZakatPanel); // Hubungkan ke fungsi penutup HUD yang aman
        }
    }

    public void OpenZakatPanel()
    {
        Debug.Log("[ZakatPanel] Membuka panel zakat.");
        if (UIManager.instance != null)
        {
            UIManager.instance.OpenPanelMenu(zakatCarouselPanel);
        }
        else
        {
            zakatCarouselPanel.SetActive(true);
        }

        if (asetBlur != null) asetBlur.SetActive(true);

        currentIndex = 2; 
        isMoving = false; 

        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        // 🔥 Paksa atur ulang posisi semua item secara matematis di awal agar simetris sempurna
        RepositionItemsDynamically();

        UpdateTargetPosition(true);
        UpdateNavButtons();
        UpdatePaymentButtonVisual(); 
    }

    public void CloseZakatPanel()
    {
        // --- PANGGIL UIMANAGER UNTUK MUNCULKAN HUD KEMBALI ---
        if (UIManager.instance != null)
        {
            UIManager.instance.ClosePanelMenu(zakatCarouselPanel);
        }
        else
        {
            zakatCarouselPanel.SetActive(false);
        }

        if (asetBlur != null) asetBlur.SetActive(false);
    }

    // 🔥 FUNGSI BARU: Mengatur posisi X semua element berdasarkan rumus matematika yang adil
    void RepositionItemsDynamically()
    {
        if (items == null || items.Length == 0) return;

        // Kita jadikan element tengah (index 2) sebagai titik nol (0)
        // Rumus: (index - 2) * spacing
        // Index 0 -> (0-2) * 500 = -1000
        // Index 1 -> (1-2) * 500 = -500
        // Index 2 -> (2-2) * 500 = 0
        // Index 3 -> (3-2) * 500 = 500
        // Index 4 -> (4-2) * 500 = 1000
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                float calculatedX = (i - 2) * itemSpacing;
                items[i].localPosition = new Vector3(calculatedX, items[i].localPosition.y, items[i].localPosition.z);
            }
        }
    }

    // =================================================================
    // 🔥 NAVIGASI CAROUSEL
    // =================================================================
    public void NextItem()
    {
        Debug.Log($"[ZakatPanel] Tombol Next diklik. Status isMoving: {isMoving}, CurrentIndex: {currentIndex}");

        if (isMoving) return;

        if (currentIndex < 3)
        {
            currentIndex++;
            Debug.Log($"[ZakatPanel] Index naik menjadi: {currentIndex}. Memulai pergeseran...");
            UpdateNavButtons();
            
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine());
        }
    }

    public void PreviousItem()
    {
        Debug.Log($"[ZakatPanel] Tombol Previous diklik. Status isMoving: {isMoving}, CurrentIndex: {currentIndex}");

        if (isMoving) return;

        if (currentIndex > 1)
        {
            currentIndex--;
            Debug.Log($"[ZakatPanel] Index turun menjadi: {currentIndex}. Memulai pergeseran...");
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
        if (btnPrevious != null)
            btnPrevious.gameObject.SetActive(currentIndex > 1);

        if (btnNext != null)
            btnNext.gameObject.SetActive(currentIndex < 3);
    }

    void UpdateTargetPosition(bool instant)
    {
        if (items.Length == 0 || items[currentIndex] == null) return;

        // Target koordinat X mengikuti posisi dinamis yang baru saja dihitung
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
    // 🔥 MANAGEMENT VISUAL BUTTON 3D & SKALA CAROUSEL
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

                if (i == indexPerdagangan) baseColor = (jurnalManager != null ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
                else if (i == indexEmasPerak) baseColor = (jurnalManager != null && jurnalManager.IsEmasPerakUnlocked()) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
                else if (i == indexPeternakan) baseColor = (jurnalManager != null && jurnalManager.IsPeternakanUnlocked()) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);

                if (i == 0 || i == 4) baseColor.a = 0f; 
                else baseColor.a = 1f;

                img.color = baseColor;
            }
        }
    }

    public void UpdatePaymentButtonVisual()
    {
        if (btnBayarZakat == null) return;

        bool currentUnlocked = false;
        if (currentIndex == indexPerdagangan) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked;
        else if (currentIndex == indexEmasPerak) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsEmasPerakUnlocked() : isEmasPerakUnlocked; 
        else if (currentIndex == indexPeternakan) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPeternakanUnlocked() : isPeternakanUnlocked;

        Renderer objRenderer = btnBayarZakat.GetComponent<Renderer>();
        if (objRenderer == null) objRenderer = btnBayarZakat.GetComponentInChildren<Renderer>();

        if (objRenderer != null)
        {
            objRenderer.material.color = currentUnlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void TriggerZakatAction()
    {
        bool currentUnlocked = false;
        if (currentIndex == indexPerdagangan) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPerdaganganUnlocked() : isPerdaganganUnlocked;
        else if (currentIndex == indexEmasPerak) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsEmasPerakUnlocked() : isEmasPerakUnlocked;
        else if (currentIndex == indexPeternakan) currentUnlocked = (jurnalManager != null) ? jurnalManager.IsPeternakanUnlocked() : isPeternakanUnlocked;

        if (currentUnlocked)
        {
            if (currentIndex == indexPerdagangan) Debug.Log("[ZakatPanel] Membuka Panel Zakat Perdagangan...");
        }
        else
        {
            Debug.Log("[ZakatPanel] maaf selesaikan dulu level zakat pertama.");
        }
    }

    public int GetCurrentIndex() { return currentIndex; }
    public void UpdateItemVisuals() { UpdatePaymentButtonVisual(); }
    public void UpdatePaymentButton() { UpdatePaymentButtonVisual(); }
    // Tambahkan ini di dalam class ZakatPanelManager : MonoBehaviour
}