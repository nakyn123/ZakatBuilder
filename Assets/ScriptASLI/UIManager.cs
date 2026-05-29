using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD Parent Object")]
    [SerializeField] private GameObject gameplayHUD;

    private int openedPanelsCount = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Panggil fungsi ini tepat saat membuka panel menu
    /// </summary>
    public void OpenPanelMenu(GameObject panelToOpen)
    {
        if (panelToOpen == null) return;

        // Buka panel target
        panelToOpen.SetActive(true);
        
        // Naikkan hitungan panel yang terbuka
        openedPanelsCount++;
        
        // Sembunyikan HUD utama
        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(false);
        }
    }

    /// <summary>
    /// Panggil fungsi ini tepat saat menutup panel menu
    /// </summary>
    public void ClosePanelMenu(GameObject panelToClose)
    {
        if (panelToClose == null) return;

        // Tutup panel target
        panelToClose.SetActive(false);
        
        // Turunkan hitungan panel
        openedPanelsCount--;
        if (openedPanelsCount < 0) openedPanelsCount = 0;

        // Jika semua panel sudah bersih ditutup, munculkan kembali HUD utama
        if (openedPanelsCount == 0 && gameplayHUD != null)
        {
            gameplayHUD.SetActive(true);
        }
    }
}