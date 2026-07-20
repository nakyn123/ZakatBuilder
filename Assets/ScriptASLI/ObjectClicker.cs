using UnityEngine;
using UnityEngine.EventSystems;

// Script ini mendeteksi sentuhan pada objek 3D
public class ObjectClicker : MonoBehaviour, IPointerDownHandler
{
    public ZakatPanelManager panelManager;
    
    // Referensi otomatis ke AudioSource di objek yang sama
    private AudioSource audioSource;

    private void Start()
    {
        // Mengambil komponen AudioSource saat game dimulai
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. MAINKAN SUARA KLIK SEGERA
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        // 2. Membuka panel langsung tanpa menunggu frame berikutnya
        if (panelManager != null)
        {
            // Pastikan kita memaksa panel aktif terlebih dahulu jika UIManager bermasalah
            panelManager.OpenZakatPanel();
        }
    }
}