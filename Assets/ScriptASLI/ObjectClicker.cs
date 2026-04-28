using UnityEngine;
using UnityEngine.EventSystems;

// Script ini mendeteksi sentuhan pada objek 3D
public class ObjectClicker : MonoBehaviour, IPointerDownHandler
{
    public ZakatPanelManager panelManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Membuka panel saat objek diklik/ditekan
        if (panelManager != null)
        {
            panelManager.OpenZakatPanel();
        }
    }
}