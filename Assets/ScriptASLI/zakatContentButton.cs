using UnityEngine;
using UnityEngine.UI;

public class ZakatContentButton : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetPanel; 
    public int itemIndex; 

    [Header("Audio Settings - Tambahan")]
    public AudioSource audioSourceZakat; // Tarik AudioSource ke sini
    public AudioClip suaraKlikBukaZakat;  // Tarik SFX klik sukses membuka panel zakat

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
        if (manager != null && manager.GetCurrentIndex() == itemIndex)
        {
            // --- 1. VALIDASI PERDAGANGAN ---
            if (itemIndex == manager.indexPerdagangan)
            {
                // Hapus syarat completed-nya biar bisa diklik terus buat testing
                if (!manager.isPerdaganganUnlocked) return; 
            }

            // --- 2. VALIDASI EMAS & PERAK ---
            if (itemIndex == manager.indexEmasPerak)
            {
                if (!manager.isEmasPerakUnlocked) return; 
            }

            // --- 3. VALIDASI PETERNAKAN ---
            if (itemIndex == manager.indexPeternakan)
            {
                if (!manager.isPeternakanUnlocked) return; 
            }

            // Memainkan suara klik sukses
            if (audioSourceZakat != null && suaraKlikBukaZakat != null)
            {
                audioSourceZakat.PlayOneShot(suaraKlikBukaZakat);
            }

            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
            }
        }
    }
}