using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookInput : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 LookInput { get; private set; }
    public bool IsRunning { get; private set; } // 🔥 Properti baru untuk dibaca oleh PlayerMovement

    [Header("Sprint Settings")]
    [Tooltip("Berapa lama jari harus ditahan untuk memicu lari")]
    [SerializeField] private float holdDurationForSprint = 0.3f;
    [Tooltip("Berapa jauh toleransi jari bergeser agar tetap dianggap menahan/hold")]
    [SerializeField] private float dragTolerance = 10f;
    private bool isDragging = false;
    private int activePointerId = -1;
    private float touchTime = 0f;
    private Vector2 startTouchPosition;
    private bool checkSprint = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        // pointerId < 0 biasanya adalah Mouse. 0 ke atas adalah Touch.
        // Jika ingin benar-benar mematikan mouse, gunakan baris di bawah:
        if (eventData.pointerId < 0) return; 

        isDragging = true;
        activePointerId = eventData.pointerId;
        // Mulai hitung timer saat pertama kali menyentuh layar
        touchTime = Time.time;
        startTouchPosition = eventData.position;
        checkSprint = true;
        IsRunning = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && eventData.pointerId == activePointerId)
        {
            // Mengambil delta (selisih gerak) hanya saat jari menyeret
            LookInput = eventData.delta;
            // Jika jari terseret terlalu jauh saat baru menyentuh, batalkan pengecekan lari
            if (checkSprint && Vector2.Distance(eventData.position, startTouchPosition) > dragTolerance)
            {
                checkSprint = false;
                IsRunning = false;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId)
        {
            isDragging = false;
            activePointerId = -1;
            LookInput = Vector2.zero;
            checkSprint = false;
            IsRunning = false;
        }
    }

    void Update()
    {
        // Paksa jadi nol jika tidak ada jari yang menyentuh
        if (!isDragging)
        {
            LookInput = Vector2.zero;
        }
        if (isDragging && checkSprint && !IsRunning)
        {
            if (Time.time - touchTime >= holdDurationForSprint)
            {
                IsRunning = true;
                Debug.Log("<color=yellow>[TouchInput]</color> Player Mengisyaratkan LARI (Hold Terdeteksi)!");
            }
        }
    }
}