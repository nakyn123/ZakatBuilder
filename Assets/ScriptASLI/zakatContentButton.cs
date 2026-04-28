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
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                // Kamu bisa menambahkan animasi transisi di sini
            }
        }
    }
}