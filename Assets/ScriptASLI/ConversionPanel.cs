using UnityEngine;
using UnityEngine.UI;

public class ZakatRewardManager : MonoBehaviour
{
    [Header("Reward Panels")]
    [SerializeField] private GameObject rewardEmasPerakPanel;

    [Header("Conversion Panels")]
    // Tarik GameObject 'ConversionPanel' (yang membungkus BG Blur & Dialog) ke sini
    [SerializeField] private GameObject conversionPanel; 
    [SerializeField] private Button closeConversionButton;

    void Start()
    {
        // Pastikan tombol close panel konversi mendengarkan fungsi Close
        if (closeConversionButton != null)
        {
            closeConversionButton.onClick.AddListener(CloseConversionPanel);
        }
    }

    // Fungsi yang dipanggil saat tombol Close di Reward Zakat Emas Perak diklik
    public void OnCloseRewardEmasPerak()
    {
        // 1. Tutup panel reward emas perak
        if (rewardEmasPerakPanel != null)
        {
            rewardEmasPerakPanel.SetActive(false);
        }

        // 2. Langsung munculkan panel konversi beserta BG Blur-nya
        if (conversionPanel != null)
        {
            conversionPanel.SetActive(true);
        }
    }

    // Fungsi untuk menutup panel konversi kembali ke game/menu utama
    private void CloseConversionPanel()
    {
        if (conversionPanel != null)
        {
            conversionPanel.SetActive(false);
        }
    }
}