using UnityEngine;

public class ZakatZoneTrigger : MonoBehaviour
{
    public enum TipeZona { KantorZakat, Tambang, Toko, TempatIsiPakan }
    
    [Header("Konfigurasi Zona")]
    public TipeZona jenisZona;
    public UI3DArrowNavigation uiArrowScript;
    public GameObject rawImageDisplay; // Raw Image di Canvas HUD

    [Header("Referensi UI Bar Misi Terkait")]
    public GameObject barNavigasiMisiIni; // Masukkan bar navigasi yang harus hilang saat tiba

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiArrowScript != null && uiArrowScript.gameObject.activeSelf && uiArrowScript.targetDestination == this.transform)
            {
                Debug.Log($"Player sampai di area: {jenisZona}!");
                
                // 1. Matikan panah 3D di HUD
                uiArrowScript.HideArrow();
                if (rawImageDisplay != null) rawImageDisplay.SetActive(false);

                // 2. Hapus bar navigasi "Pergi" dari panel misi
                if (barNavigasiMisiIni != null) {
                    barNavigasiMisiIni.SetActive(false);
                }

                // 3. Pemicu khusus Toko untuk langsung membuka dialog naratif
                if (jenisZona == TipeZona.Toko && TokoManager.instance != null)
                {
                    TokoManager.instance.PemicuMasukTokoPertamaKali();
                }

                // 4. 🌟 BARU: Langsung matikan tabung lingkaran merah ini saat ini juga tanpa delay!
                gameObject.SetActive(false); 
            }
        }
    }
}