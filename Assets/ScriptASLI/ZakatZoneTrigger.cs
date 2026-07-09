using UnityEngine;

public class ZakatZoneTrigger : MonoBehaviour
{
    public enum TipeZona { KantorZakat, Tambang, Toko, TempatIsiPakan }
    
    [Header("Konfigurasi Zona")]
    public TipeZona jenisZona;
    public UI3DArrowNavigation uiArrowScript;
    public GameObject rawImageDisplay; 

    [Header("Referensi UI Bar Misi Terkait")]
    public GameObject barNavigasiMisiIni; 

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

                // REVISI: Paksa hilangkan bar misi navigasi ini secara total dari hirarki UI
                if (barNavigasiMisiIni != null) {
                    barNavigasiMisiIni.SetActive(false);
                }

                // 3. Pemicu khusus Toko untuk langsung membuka dialog naratif
                if (jenisZona == TipeZona.Toko && TokoManager.instance != null)
                {
                    TokoManager.instance.PemicuMasukTokoPertamaKali();
                }

                // 4. Langsung matikan tabung lingkaran merah zona ini
                gameObject.SetActive(false); 
            }
        }
    }
}