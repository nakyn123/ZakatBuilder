using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookInput : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 LookInput { get; private set; }
    private bool isDragging = false;
    private int activePointerId = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        // pointerId < 0 biasanya adalah Mouse. 0 ke atas adalah Touch.
        // Jika ingin benar-benar mematikan mouse, gunakan baris di bawah:
        if (eventData.pointerId < 0) return; 

        isDragging = true;
        activePointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && eventData.pointerId == activePointerId)
        {
            // Mengambil delta (selisih gerak) hanya saat jari menyeret
            LookInput = eventData.delta;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId)
        {
            isDragging = false;
            activePointerId = -1;
            LookInput = Vector2.zero;
        }
    }

    void Update()
    {
        // Paksa jadi nol jika tidak ada jari yang menyentuh
        if (!isDragging)
        {
            LookInput = Vector2.zero;
        }
    }
}