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
            // --- PENGAMAN UTAMA: JIKA GAME SUDAH TAMAT / ENDING, JANGAN PROSES APAPUN ---
            if (TaskManager.instance != null && TaskManager.instance.isGameEnding)
            {
                return;
            }

            // --- LOGIKA KHUSUS TOKO ---
            if (jenisZona == TipeZona.Toko)
            {
                Debug.Log("Player sampai di area Toko (Memicu dialog awal & pembersihan)");

                if (uiArrowScript != null) uiArrowScript.HideArrow();
                if (rawImageDisplay != null) rawImageDisplay.SetActive(false);

                if (TaskManager.instance != null && TaskManager.instance.barKeToko != null)
                {
                    TaskManager.instance.barKeToko.SetActive(false);
                }
                
                if (barNavigasiMisiIni != null) barNavigasiMisiIni.SetActive(false);

                if (TokoManager.instance != null)
                {
                    TokoManager.instance.PemicuMasukTokoPertamaKali();
                }

                gameObject.SetActive(false);
                return;
            }

            // --- LOGIKA KHUSUS KANTOR ZAKAT (BISA MASUK WALAU TANPA ARROW HUD / GABUTUH BANTUAN) ---
            if (jenisZona == TipeZona.KantorZakat)
            {
                Debug.Log("Player sampai di area Kantor Zakat (Pembersihan Navigasi Mandiri/Otomatis)");

                // 1. Matikan panah HUD & display-nya jika sedang aktif
                if (uiArrowScript != null) uiArrowScript.HideArrow();
                if (rawImageDisplay != null) rawImageDisplay.SetActive(false);

                // 2. Sembunyikan bar "Pergi ke Kantor Zakat" secara paksa dari TaskManager
                if (TaskManager.instance != null && TaskManager.instance.barKeKantorZakat != null)
                {
                    TaskManager.instance.barKeKantorZakat.SetActive(false);
                }

                // 3. Matikan bar penunjang lokal jika ditarik di inspector
                if (barNavigasiMisiIni != null) barNavigasiMisiIni.SetActive(false);

                // 4. Matikan trigger zona ini agar tidak memicu berulang kali
                gameObject.SetActive(false);
                return;
            }

            // --- LOGIKA LAMA UNTUK ZONA LAIN (TAMBANG, TEMPAT ISI PAKAN, DLL) ---
            if (uiArrowScript != null && uiArrowScript.gameObject.activeSelf && uiArrowScript.targetDestination == this.transform)
            {
                Debug.Log($"Player sampai di area: {jenisZona}!");
                
                uiArrowScript.HideArrow();
                if (rawImageDisplay != null) rawImageDisplay.SetActive(false);

                if (barNavigasiMisiIni != null) {
                    barNavigasiMisiIni.SetActive(false);
                }

                gameObject.SetActive(false); 
            }
        }
    }
}