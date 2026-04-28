using UnityEngine;
using UnityEngine.UI;

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
    public Button btnBayarZakat;

    [Header("Carousel Settings")]
    public float transitionSpeed = 10f;
    public float centerScale = 1.2f;
    public float sideScale = 0.5f;

    private int currentIndex = 2;
    private Vector2 targetPos;

    [Header("Status Locking (Data Only)")]
    public bool isPerdaganganUnlocked = false;
    public bool isPertanianUnlocked = false;
    public bool isPeternakanUnlocked = false;

    [Header("Mapping Jurnal -> Item")]
    public int indexPerdagangan = 2;
    public int indexPertanian = 1;
    public int indexPeternakan = 3;

    [Header("External")]
    public JurnalManager jurnalManager;
    bool isMoving = false;
    private RectTransform[] items;
    void Awake()
    {   items = new RectTransform[] {
        element0,
        element1,
        element2,
        element3,
        element4
        };
        if (zakatCarouselPanel != null) zakatCarouselPanel.SetActive(false);
        if (asetBlur != null) asetBlur.SetActive(false);
    }

    public void OpenZakatPanel()
    {
        zakatCarouselPanel.SetActive(true);
        asetBlur.SetActive(true);

        // Mulai dari Index 0
        currentIndex = 2; 
        
        isMoving = false; // 🔥 WAJIB (ini penyebab klik harus 2x)

        UpdateTargetPosition(true);
        UpdateItemVisuals();
        UpdateNavButtons();
        UpdatePaymentButton(); // 🔥 tambahin ini juga
    }

    public void CloseZakatPanel()
    {
        zakatCarouselPanel.SetActive(false);
        asetBlur.SetActive(false);
    }

    public void NextItem()
    {
        if (isMoving) return;

        // 🔥 STOP sebelum index terakhir
        if (currentIndex < 3)
        {
            currentIndex++;
            UpdateTargetPosition(false);
            UpdatePaymentButton();
            UpdateNavButtons();
            isMoving = true;
        }
    }

    public void PreviousItem()
    {
        // 🔥 STOP sebelum index pertama
        if (currentIndex > 1)
        {
            currentIndex--;
            UpdateTargetPosition(false);
            UpdatePaymentButton();
            UpdateNavButtons();
            isMoving = true; // 🔥 tambahin ini
        }
    }

    void UpdateNavButtons()
    {
        if (btnPrevious != null)
            btnPrevious.gameObject.SetActive(currentIndex > 1);

        if (btnNext != null)
            btnNext.gameObject.SetActive(currentIndex <  3);
    }

    void UpdateTargetPosition(bool instant)
    {
        if (items.Length == 0) return;

        // Menghitung posisi agar item ke-index berada tepat di tengah viewPort
        float targetX = -items[currentIndex].localPosition.x;
        targetPos = new Vector2(targetX, content.anchoredPosition.y);

        if (instant) {
            content.anchoredPosition = targetPos;
            HandleScaling(); 
        }
    }

    // Ganti Update yang lama dengan ini
    void Update()
    {
        if (!zakatCarouselPanel.activeSelf) return;

        content.anchoredPosition = Vector2.Lerp(
            content.anchoredPosition,
            targetPos,
            Time.deltaTime * transitionSpeed
        );

        if (Vector2.Distance(content.anchoredPosition, targetPos) < 0.1f)
        {
            content.anchoredPosition = targetPos;
            isMoving = false;
        }
    }

    // Tambahkan LateUpdate untuk menghitung skala setelah posisi fix
    void LateUpdate()
    {
        if (!zakatCarouselPanel.activeSelf) return;
        HandleScaling();
    }

    void HandleScaling()
    {
        float maxDistance = viewPort.rect.width / 2f;

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform item = items[i];

            float itemPosX = item.localPosition.x + content.anchoredPosition.x;
            float distance = Mathf.Abs(itemPosX);

            float t = Mathf.Clamp01(distance / maxDistance);

            float targetScale = Mathf.Lerp(centerScale, sideScale, t);

            // 🔥 smooth scaling biar ga patah
            item.localScale = Vector3.Lerp(
                item.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * 10f
            );
            Image img = item.GetComponent<Image>();
        if (img == null) img = item.GetComponentInChildren<Image>();

        if (img != null)
        {
            Color c = img.color;

            if (i == 0 || i == 4)
                c.a = 0f; // element 0 hilang
            else
                c.a = 1f;

            img.color = c;
        }
        }
    }

    public void UpdateItemVisuals()
    {
        if (jurnalManager == null) return;

        // default semua gelap dulu
        for (int i = 0; i < items.Length; i++)
        {
            SetItemState(i, false);
        }

        // nyalakan sesuai jurnal
        if (jurnalManager.IsPerdaganganUnlocked())
            SetItemState(indexPerdagangan, true);

        if (jurnalManager.IsPertanianUnlocked())
            SetItemState(indexPertanian, true);

        if (jurnalManager.IsPeternakanUnlocked())
            SetItemState(indexPeternakan, true);
    }

    void SetItemState(int index, bool isUnlocked) {
        if (index >= items.Length) return;
        
        Image img = items[index].GetComponent<Image>();
        if (img == null) img = items[index].GetComponentInChildren<Image>();
        
        if (img != null) {
            // Jika Unlocked: Warna Putih (Terang), Jika Locked: Warna Gelap
            img.color = isUnlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
        }
    }
    public void UpdatePaymentButton() {
        bool currentUnlocked = false;
        
        // Cek status unlock berdasarkan index yang aktif
        if (currentIndex == 0) currentUnlocked = isPerdaganganUnlocked;
        else if (currentIndex == 1) currentUnlocked = isPertanianUnlocked;
        else if (currentIndex == 2) currentUnlocked = isPeternakanUnlocked;

        if (btnBayarZakat != null) {
            btnBayarZakat.interactable = currentUnlocked;
            btnBayarZakat.GetComponent<Image>().color = currentUnlocked ? Color.white : Color.gray;
        }
    }
    // Tambahkan ini di ZakatPanelManager agar script luar bisa tahu item mana yang di tengah
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}