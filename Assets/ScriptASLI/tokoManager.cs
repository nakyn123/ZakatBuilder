using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TokoManager : MonoBehaviour
{
    public static TokoManager instance;

    [Header("UI Global Toko (HUD Area)")]
    public GameObject iconTokoHUD;          
    public GameObject notifTandaSeruObj;    
    public RectTransform navCoinHUD;        // Objek UI "nav-coin-left"

    [Header("UI Animasi & Audio Teks Minus")]
    public GameObject prefabTeksMinusAnim;   
    public AudioSource audioSourceToko;     
    public AudioClip suaraUangBeli;         
    public RectTransform iconKoinEmasHUDPivot;

    [Header("UI Multi-Panel Dialog (Naratif Pertama Kali)")]
    [Tooltip("Masukkan 3 GameObject Panel Dialogmu (bg, bg(1), bg(2)) secara berurutan")]
    public GameObject[] listDialogPanels;   
    
    [Tooltip("Masukkan 3 TextMeshProUGUI dari masing-masing panel secara berurutan")]
    public TextMeshProUGUI[] listTxtDialogs; 
    
    [Tooltip("Satu tombol next utama yang digunakan bersama")]
    public Button btnNextDialogTunggal;     
    
    [TextArea(3, 5)]
    [Tooltip("Isi dengan 3 cerita dialog berbeda")]
    public string[] pesanDialog = new string[] {
        "Halo! Selamat datang di tokoku.",
        "Di sini aku menyediakan berbagai macam kebutuhan untuk peternakanmu.",
        "barang yang bisa kamu beli. Tapi kalau tujuanmu mencari hewan ternak yang sehat untuk dipelihara, kamu datang ke tempat yang tepat. Kalau butuh pakan atau perlengkapan perawatan, aku juga menyediakannya. silahkan datang kapan saja untuk membeli sesuatu."
    };
    public float kecepatanKetik = 0.02f;

    [Header("UI Toko Multi-Panel References")]
    public GameObject mainTokoPanel;        // toko-panel-asli
    public GameObject panelTokoHewan;       // toko-hewan (1)
    public GameObject panelTokoPakan;       // toko pakan
    public GameObject panelUangKurang;      // UI "Panel Uang Tidak Cukup"

    [Header("UI Peringatan (Jika Belum Level 3)")]
    public GameObject txtPeringatanTokoObj; 

    [HideInInspector] public bool isPlayerInside = false;
    
    private bool sudahPernahDialog = false;
    private Coroutine mengetikCoroutine;
    private bool magnetssedangMengetik = false;

    // Tracker index urutan dialog yang sedang aktif (0, 1, atau 2)
    private int currentDialogIndex = 0;

    private int originalNavCoinSiblingIndex;
    private Transform originalNavCoinParent;
    private GameObject masterTokoPanelUtama;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (txtPeringatanTokoObj != null) txtPeringatanTokoObj.SetActive(false);
        if (iconTokoHUD != null) iconTokoHUD.SetActive(false);
        if (notifTandaSeruObj != null) notifTandaSeruObj.SetActive(false);
        if (mainTokoPanel != null) mainTokoPanel.SetActive(false);
        if (panelUangKurang != null) panelUangKurang.SetActive(false);
        
        // Matikan semua panel dialog di awal game
        MatikanSemuaPanelDialog();

        if (navCoinHUD != null)
        {
            originalNavCoinParent = navCoinHUD.parent;
            originalNavCoinSiblingIndex = navCoinHUD.GetSiblingIndex();
        }

        if (mainTokoPanel != null && mainTokoPanel.transform.parent != null)
        {
            masterTokoPanelUtama = mainTokoPanel.transform.parent.gameObject;
        }

        // Ambil kendali tombol via kode agar aman dan urut
        if (btnNextDialogTunggal != null)
        {
            btnNextDialogTunggal.onClick.RemoveAllListeners();
            btnNextDialogTunggal.onClick.AddListener(TombolNextDialogDiKlik);
            btnNextDialogTunggal.gameObject.SetActive(false); // Sembunyikan dulu di awal
        }
    }

    public void PerbaruiTampilanToko()
    {
        bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;

        if (sudahLevel3)
        {
            if (txtPeringatanTokoObj != null) txtPeringatanTokoObj.SetActive(false);
            if (iconTokoHUD != null) iconTokoHUD.SetActive(true);

            if (!sudahPernahDialog && notifTandaSeruObj != null)
            {
                notifTandaSeruObj.SetActive(true);
            }
        }
        else
        {
            if (txtPeringatanTokoObj != null) txtPeringatanTokoObj.SetActive(true);
        }
    }

    public void TombolTokoHUDDiKlik()
    {
        if (notifTandaSeruObj != null) notifTandaSeruObj.SetActive(false);

        SembuhkanDanBawaNavCoinKeDepan(true);

        if (masterTokoPanelUtama != null)
        {
            if (UIManager.instance != null) UIManager.instance.OpenPanelMenu(masterTokoPanelUtama);
            else masterTokoPanelUtama.SetActive(true);
        }
        else if (mainTokoPanel != null)
        {
            if (UIManager.instance != null) UIManager.instance.OpenPanelMenu(mainTokoPanel);
            else mainTokoPanel.SetActive(true);
        }

        if (!sudahPernahDialog)
        {
            currentDialogIndex = 0; 
            MulaiDialogNaratif();
        }
        else
        {
            BukaSubPanelHewan();
        }
    }

    void MulaiDialogNaratif()
    {
        if (mainTokoPanel != null) mainTokoPanel.SetActive(false);
        if (panelTokoHewan != null) panelTokoHewan.SetActive(false);
        if (panelTokoPakan != null) panelTokoPakan.SetActive(false);
        if (navCoinHUD != null) navCoinHUD.gameObject.SetActive(false);

        // Bersihkan sisa teks lama di semua slot array biar gak tumpang tindih
        if (listTxtDialogs != null)
        {
            for (int i = 0; i < listTxtDialogs.Length; i++)
            {
                if (listTxtDialogs[i] != null) listTxtDialogs[i].text = "";
            }
        }

        MatikanSemuaPanelDialog();
        UpdateStatusPanelDanKetik();
    }

    private void UpdateStatusPanelDanKetik()
    {
        if (listDialogPanels == null || currentDialogIndex >= listDialogPanels.Length) return;

        // 1. Aktifkan panel sesuai urutan index saat ini
        if (listDialogPanels[currentDialogIndex] != null)
        {
            listDialogPanels[currentDialogIndex].SetActive(true);
        }

        // 2. Sembunyikan tombol NEXT selama teks sedang bersiap/mengetik
        if (btnNextDialogTunggal != null)
        {
            btnNextDialogTunggal.gameObject.SetActive(false);
        }

        // 3. Jalankan transisi aman via Coroutine baru agar TextMeshPro siap
        if (mengetikCoroutine != null) StopCoroutine(mengetikCoroutine);
        mengetikCoroutine = StartCoroutine(JalankanMekanikKetikAman());
    }

    // Coroutine jembatan untuk menunggu Unity UI siap
    IEnumerator JalankanMekanikKetikAman()
    {
        // Tunggu sampai akhir frame UI selesai di-render dan diaktifkan oleh Unity
        yield return new WaitForEndOfFrame();

        if (listTxtDialogs != null && currentDialogIndex < listTxtDialogs.Length && listTxtDialogs[currentDialogIndex] != null)
        {
            // Panggil fungsi mengetik bawaanmu setelah objek benar-benar stabil aktif
            yield return StartCoroutine(KetikTeksDialog(listTxtDialogs[currentDialogIndex], pesanDialog[currentDialogIndex]));
        }
    }

    IEnumerator KetikTeksDialog(TextMeshProUGUI targetText, string teksPenuh)
    {
        magnetssedangMengetik = true;
        targetText.text = "";

        foreach (char huruf in teksPenuh.ToCharArray())
        {
            targetText.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        magnetssedangMengetik = false;
        
        // Tampilkan kembali tombol NEXT setelah teks selesai diketik penuh
        if (btnNextDialogTunggal != null) 
        {
            btnNextDialogTunggal.gameObject.SetActive(true);
            btnNextDialogTunggal.transform.SetAsLastSibling(); // Pastikan tombol berada paling depan layer
        }
    }

    public void TombolNextDialogDiKlik()
    {
        if (magnetssedangMengetik) return; 

        // Hentikan coroutine mengetik yang lama agar tidak tabrakan di latar belakang
        if (mengetikCoroutine != null) 
        {
            StopCoroutine(mengetikCoroutine);
        }

        // Pastikan teks pada panel saat ini dikosongkan total sebelum panelnya dimatikan
        if (listTxtDialogs != null && currentDialogIndex < listTxtDialogs.Length && listTxtDialogs[currentDialogIndex] != null)
        {
            listTxtDialogs[currentDialogIndex].text = ""; 
        }

        // Matikan panel yang sedang aktif saat ini
        if (listDialogPanels != null && currentDialogIndex < listDialogPanels.Length && listDialogPanels[currentDialogIndex] != null)
        {
            listDialogPanels[currentDialogIndex].SetActive(false);
        }

        // Maju ke index dialog berikutnya
        currentDialogIndex++; 

        // Jika masih ada dialog selanjutnya, aktifkan dan ketik teks baru
        if (listDialogPanels != null && currentDialogIndex < listDialogPanels.Length)
        {
            UpdateStatusPanelDanKetik();
        }
        else
        {
            // Jika sudah lewat dialog terakhir, tutup panel dialog dan buka toko asli
            if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);
            BukaTokoPertamaKali();
        }
    }

    public void BukaTokoPertamaKali()
    {
        sudahPernahDialog = true; 
        MatikanSemuaPanelDialog();
        if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);
        BukaSubPanelHewan();
    }

    private void MatikanSemuaPanelDialog()
    {
        if (listDialogPanels != null)
        {
            for (int i = 0; i < listDialogPanels.Length; i++)
            {
                if (listDialogPanels[i] != null) listDialogPanels[i].SetActive(false);
            }
        }
    }

    public void BukaSubPanelHewan()
    {
        MatikanSemuaPanelDialog();
        if (mainTokoPanel != null) mainTokoPanel.SetActive(true);

        if (panelTokoHewan != null) panelTokoHewan.SetActive(true);
        if (panelTokoPakan != null) panelTokoPakan.SetActive(false);

        if (navCoinHUD != null) navCoinHUD.gameObject.SetActive(true);
        SembuhkanDanBawaNavCoinKeDepan(true);
    }

    public void BukaSubPanelPakan()
    {
        MatikanSemuaPanelDialog();
        if (mainTokoPanel != null) mainTokoPanel.SetActive(true);

        if (panelTokoHewan != null) panelTokoHewan.SetActive(false);
        if (panelTokoPakan != null) panelTokoPakan.SetActive(true);

        if (navCoinHUD != null) navCoinHUD.gameObject.SetActive(true);
        SembuhkanDanBawaNavCoinKeDepan(true);
    }

    public void CloseTokoPanel()
    {
        GameObject panelToClose = (masterTokoPanelUtama != null) ? masterTokoPanelUtama : mainTokoPanel;

        if (panelToClose != null)
        {
            if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(panelToClose);
            else panelToClose.SetActive(false);
        }

        if (panelUangKurang != null) panelUangKurang.SetActive(false);
        if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);

        SembuhkanDanBawaNavCoinKeDepan(false);
    }

    private void SembuhkanDanBawaNavCoinKeDepan(bool bawaKeDepan)
    {
        if (navCoinHUD != null)
        {
            if (bawaKeDepan)
            {
                if (masterTokoPanelUtama != null)
                {
                    navCoinHUD.SetParent(masterTokoPanelUtama.transform.parent, true);
                    navCoinHUD.gameObject.SetActive(true); 
                    navCoinHUD.SetAsLastSibling();         
                }
            }
            else
            {
                if (originalNavCoinParent != null)
                {
                    navCoinHUD.SetParent(originalNavCoinParent, true);
                    navCoinHUD.SetSiblingIndex(originalNavCoinSiblingIndex);
                }
            }
        }
    }

    #region LOGIKA TRANSAKSI BELI ITEM TOKO
    public void BeliItemToko(int hargaItem)
    {
        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaItem)
            {
                MoneyManager.instance.RemoveMoney(hargaItem);
                Debug.Log("<color=green>[Toko]</color> Pembelian Sukses! Sisa: " + MoneyManager.instance.totalMoney);
                
                JalankanEfekAudioDanKoin(hargaItem);
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
        else
        {
            Debug.LogError("MoneyManager instance tidak ditemukan!");
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyHewanDibeli();
    }

    private void JalankanEfekAudioDanKoin(int hargaYangDibeli)
    {
        if (audioSourceToko != null && suaraUangBeli != null)
        {
            audioSourceToko.PlayOneShot(suaraUangBeli);
        }

        if (prefabTeksMinusAnim != null && navCoinHUD != null)
        {
            Vector3 posisiSpawn = navCoinHUD.position;

            GameObject teksMinusObj = Instantiate(prefabTeksMinusAnim, navCoinHUD.parent);
            teksMinusObj.transform.position = posisiSpawn;

            TeksMinusAnim komponenAnim = teksMinusObj.GetComponent<TeksMinusAnim>();
            if (komponenAnim != null)
            {
                string teksFormatMinus = "-Rp " + hargaYangDibeli.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                komponenAnim.SetupTeksMinus(teksFormatMinus);
            }
        }
    }

    public void BukaPanelUangKurang()
    {
        if (panelUangKurang != null)
        {
            panelUangKurang.SetActive(true);
            panelUangKurang.transform.SetAsLastSibling(); 
        }
    }

    public void TutupPanelUangKurang()
    {
        if (panelUangKurang != null)
        {
            panelUangKurang.SetActive(false);
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            PerbaruiTampilanToko(); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            
            if (iconTokoHUD != null) iconTokoHUD.SetActive(false);
            if (notifTandaSeruObj != null) notifTandaSeruObj.SetActive(false);
            if (txtPeringatanTokoObj != null) txtPeringatanTokoObj.SetActive(false);
            
            bool adaDialogAktif = false;
            if (listDialogPanels != null)
            {
                for (int i = 0; i < listDialogPanels.Length; i++)
                {
                    if (listDialogPanels[i] != null && listDialogPanels[i].activeSelf)
                    {
                        adaDialogAktif = true;
                        break;
                    }
                }
            }

            if (adaDialogAktif)
            {
                if (mengetikCoroutine != null) StopCoroutine(mengetikCoroutine);
            }

            CloseTokoPanel();
        }
    }
    // Fungsi yang ditempel di Button Beli Pakan pada UI Toko Pakan
    public void BeliPakanRumput(int hargaPaketPakan)
    {
        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaPaketPakan)
            {
                MoneyManager.instance.RemoveMoney(hargaPaketPakan);
                Debug.Log("<color=green>[Toko]</color> Berhasil membeli Paket Makanan! Sisa Koin: " + MoneyManager.instance.totalMoney);
                
                // Jalankan efek visual & audio toko bawaanmu
                JalankanEfekAudioDanKoin(hargaPaketPakan);

                // 🔥 MASUKKAN KE INVENTORY: Otomatis jadi 6x pakan eceran
                if (InventoryManager.instance != null) {
                    InventoryManager.instance.AddPakanDariToko();
                }
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
        if (TaskManager.instance != null) TaskManager.instance.NotifyBeliPakan();
    }
}