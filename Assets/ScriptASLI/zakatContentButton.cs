using UnityEngine;
using UnityEngine.UI;

public class ZakatContentButton : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetPanel; // Taruh prefab/GameObject panel perdagangan di sini
    public int itemIndex; // Index item ini di carousel (misal: 2 untuk perdagangan)

    private Button btn;
    private ZakatPanelManager manager;

    void Awake()
    {
        btn = GetComponent<Button>();
        manager = GetComponentInParent<ZakatPanelManager>();

        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        // Hanya bisa diklik jika item ini sedang berada di tengah (fokus)
        if (manager != null && manager.GetCurrentIndex() == itemIndex)
        {
            // --- 1. VALIDASI KUNCI UNTUK PERDAGANGAN ---
            if (itemIndex == manager.indexPerdagangan && !manager.isPerdaganganUnlocked)
            {
                Debug.Log("Konten terkunci! Belum memenuhi kriteria Wajib Zakat Perdagangan.");
                return; 
            }

            // --- 2. VALIDASI KUNCI UNTUK EMAS & PERAK (TAMBAHAN BARU) ---
            if (itemIndex == manager.indexEmasPerak && !manager.isEmasPerakUnlocked)
            {
                Debug.Log("Konten terkunci! Belum memenuhi kriteria Wajib Zakat Emas & Perak.");
                return; // Batalkan proses membuka panel jika belum memenuhi syarat nisab/haul
            }

            // Jika lolos pengecekan, buka panel kuis target
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
            }
        }
    }
}