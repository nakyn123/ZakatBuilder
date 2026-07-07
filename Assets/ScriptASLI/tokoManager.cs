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
    public AudioClip suaraBukaToko;    // 🔥 TAMBAHAN BARU
    public AudioClip suaraTutupToko;

    [Header("Kustomisasi Level 3 & Tempat Berdiri")]
    public GameObject squareBerdiriPlayer; // 🔥 Objek Square Baru

    [Header("Pengaturan Limit & Game Object Sapi (Urutan 1-5)")]
    public List<GameObject> listSapi3D = new List<GameObject>();
    public Button btnBeliSapi;
    public Sprite spriteSapiSoldOut;
    public TextMeshProUGUI txtJumlahSapi; // 🔥 TAMBAHAN BARU
    private int jumlahSapiDibeli = 0;

    [Header("Pengaturan Limit & Game Object Kambing (Urutan 1-5)")]
    public List<GameObject> listKambing3D = new List<GameObject>();
    public Button btnBeliKambing;
    public Sprite spriteKambingSoldOut;
    public TextMeshProUGUI txtJumlahKambing;
    private int jumlahKambingDibeli = 0;

    [HideInInspector] public bool isPlayerInside = false;
    [Header("Pengaturan Jeda Auto-Open")]
    [Tooltip("Durasi jeda (detik) dari jalan ke idle setelah collide sebelum panel terbuka")]
    public float jedaBukaPanel = 0.15f; 

    private Coroutine delayBukaTokoCoroutine;
    
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
        
        MatikanSemuaPanelDialog();
        SembuhkanSemuaHewanAwal();

        // 🌟 DIUBAH: Jangan biarkan square berdiri langsung aktif di awal Level 3 sebelum dialog tamat!
        if (squareBerdiriPlayer != null) squareBerdiriPlayer.SetActive(false);

        if (navCoinHUD != null)
        {
            originalNavCoinParent = navCoinHUD.parent;
            originalNavCoinSiblingIndex = navCoinHUD.GetSiblingIndex();
        }

        if (mainTokoPanel != null && mainTokoPanel.transform.parent != null)
        {
            masterTokoPanelUtama = mainTokoPanel.transform.parent.gameObject;
        }

        if (btnNextDialogTunggal != null)
        {
            btnNextDialogTunggal.onClick.RemoveAllListeners();
            btnNextDialogTunggal.onClick.AddListener(TombolNextDialogDiKlik);
            btnNextDialogTunggal.gameObject.SetActive(false);
        }
        if (txtJumlahSapi != null) txtJumlahSapi.text = "Ternak Sapi\n(x5)";
        if (txtJumlahKambing != null) txtJumlahKambing.text = "Ternak Kambing\n(x5)";
    }

    // 🌟 CARI FUNGSI UPDATE() DAN PASTIKAN SEPERTI INI:
    void Update()
    {
        // 🌟 DIUBAH: Square berdiri hanya boleh aktif lewat Update jika dialog pertama sudah selesai ditonton!
        if (sudahPernahDialog)
        {
            UpdateStatusSquareTempatBerdiri();
        }
    }

    // 🌟 TAMBAHKAN FUNGSI BARU INI (UNTUK ALUR PERTAMA KALI MASUK LEWAT NAVIGASI):
    public void PemicuMasukTokoPertamaKali()
    {
        isPlayerInside = true;
        if (notifTandaSeruObj != null) notifTandaSeruObj.SetActive(false);

        // 🔒 KUNCI: Pastikan square & collider belanja mati total saat pemain baru tiba!
        if (squareBerdiriPlayer != null) squareBerdiriPlayer.SetActive(false);

        currentDialogIndex = 0; 
        MulaiDialogNaratif();
    }

    private void SembuhkanSemuaHewanAwal()
    {
        foreach (GameObject sapi in listSapi3D)
        {
            if (sapi != null) sapi.SetActive(false);
        }
        foreach (GameObject kambing in listKambing3D)
        {
            if (kambing != null) kambing.SetActive(false);
        }
    }

    private void UpdateStatusSquareTempatBerdiri()
    {
        bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;
        if (squareBerdiriPlayer != null)
        {
            squareBerdiriPlayer.SetActive(sudahLevel3);
        }
    }

    public void PerbaruiTampilanToko()
    {
        bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;

        UpdateStatusSquareTempatBerdiri();

        if (sudahLevel3)
        {
            if (txtPeringatanTokoObj != null) txtPeringatanTokoObj.SetActive(false);
            // if (iconTokoHUD != null) iconTokoHUD.SetActive(true);

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
    {   if (audioSourceToko != null && suaraBukaToko != null) {
            audioSourceToko.PlayOneShot(suaraBukaToko);
        }
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

        if (listDialogPanels[currentDialogIndex] != null)
        {
            listDialogPanels[currentDialogIndex].SetActive(true);
        }

        if (btnNextDialogTunggal != null)
        {
            btnNextDialogTunggal.gameObject.SetActive(false);
        }

        // 🔥 PERBAIKAN 1: Hentikan coroutine pengetikan langsung di sini sebelum memulai yang baru
        if (mengetikCoroutine != null) StopCoroutine(mengetikCoroutine);
        
        // Mulai pembungkus mekanik ketik aman
        StartCoroutine(JalankanMekanikKetikAman());
    }

     IEnumerator JalankanMekanikKetikAman()
    {
        yield return new WaitForEndOfFrame();

        if (listTxtDialogs != null && currentDialogIndex < listTxtDialogs.Length && listTxtDialogs[currentDialogIndex] != null)
        {
            // 🔥 PERBAIKAN 2: Simpan coroutine ketik inti ke variabel 'mengetikCoroutine' agar bisa di-Stop nantinya
            mengetikCoroutine = StartCoroutine(KetikTeksDialog(listTxtDialogs[currentDialogIndex], pesanDialog[currentDialogIndex]));
            yield return mengetikCoroutine;
        }
    }

    // 📩 FUNGSI DIPANGGIL SAAT PANEL DI-TAP
    public void SkipKetikDialogToko()
    {
        // Jika teks masih berjalan mengetik, potong langsung jadi utuh
        if (magnetssedangMengetik)
        {
            // 🔥 PERBAIKAN 3: Sekarang Coroutine ketik inti bisa dihentikan secara mutlak!
            if (mengetikCoroutine != null) StopCoroutine(mengetikCoroutine);
            
            if (listTxtDialogs != null && currentDialogIndex < listTxtDialogs.Length && listTxtDialogs[currentDialogIndex] != null)
            {
                listTxtDialogs[currentDialogIndex].text = pesanDialog[currentDialogIndex];
            }
            
            magnetssedangMengetik = false; // Tandai teks sudah penuh

            // Aktifkan tombol next tunggal agar player bisa lanjut ke panel berikutnya
            if (btnNextDialogTunggal != null) 
            {
                btnNextDialogTunggal.gameObject.SetActive(true);
                btnNextDialogTunggal.transform.SetAsLastSibling();
            }
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
        
        // Kembalikan tombol next aktif jika teks sudah selesai mengetik secara natural
        if (btnNextDialogTunggal != null) 
        {
            btnNextDialogTunggal.gameObject.SetActive(true);
            btnNextDialogTunggal.transform.SetAsLastSibling();
        }
    }

    public void TombolNextDialogDiKlik()
    {
        if (magnetssedangMengetik) return; 

        if (mengetikCoroutine != null) 
        {
            StopCoroutine(mengetikCoroutine);
        }

        if (listTxtDialogs != null && currentDialogIndex < listTxtDialogs.Length && listTxtDialogs[currentDialogIndex] != null)
        {
            listTxtDialogs[currentDialogIndex].text = ""; 
        }

        if (listDialogPanels != null && currentDialogIndex < listDialogPanels.Length && listDialogPanels[currentDialogIndex] != null)
        {
            listDialogPanels[currentDialogIndex].SetActive(false);
        }

        currentDialogIndex++; 

        if (listDialogPanels != null && currentDialogIndex < listDialogPanels.Length)
        {
            UpdateStatusPanelDanKetik();
        }
        else
        {
            if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);
            BukaTokoPertamaKali();
        }
    }

    public void BukaTokoPertamaKali()
    {
        sudahPernahDialog = true; 
        MatikanSemuaPanelDialog();
        if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);
        
        // 🔒 JANGAN nyalakan square dulu di sini, biarkan menyala saat panel toko ditutup!
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
        if (audioSourceToko != null && suaraTutupToko != null) {
            audioSourceToko.PlayOneShot(suaraTutupToko);
        }
        GameObject panelToClose = (masterTokoPanelUtama != null) ? masterTokoPanelUtama : mainTokoPanel;

        if (panelToClose != null)
        {
            if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(panelToClose);
            else panelToClose.SetActive(false);
        }

        if (panelUangKurang != null) panelUangKurang.SetActive(false);
        if (btnNextDialogTunggal != null) btnNextDialogTunggal.gameObject.SetActive(false);

        SembuhkanDanBawaNavCoinKeDepan(false);

        // 🌟 UTAMA: Aktifkan square & collider tempat berdiri murni setelah dialog tamat DAN panel toko ditutup pertama kali!
        if (sudahPernahDialog && Level3Manager.instance != null && Level3Manager.instance.isBabak3Aktif)
        {
            if (squareBerdiriPlayer != null) squareBerdiriPlayer.SetActive(true);
        }

        if (jumlahSapiDibeli > 0 || jumlahKambingDibeli > 0)
        {
            if (ReminderManager.instance != null)
            {
                ReminderManager.instance.TriggerFirstPurchaseTutorial();
            }
        }
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

    // 🌟 PERBARUI FUNGSI INI DI TOKOMANAGER.CS (HAPUS PEMANGGILAN MISI YANG DOUBLE) 🌟
    public void BeliItemToko(int hargaItem)
    {
        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaItem)
            {
                MoneyManager.instance.RemoveMoney(hargaItem);
                Debug.Log("<color=green>[Toko]</color> Pembelian Sukses! Sisa: " + MoneyManager.instance.totalMoney);
                JalankanEfekAudioDanKoin(hargaItem);
                
                // KUNCI PERBAIKAN: Hapus baris TaskManager.instance.NotifyHewanDibeli() dari sini!
                // Karena ini adalah fungsi beli item umum (seperti pakan/peralatan), bukan beli hewan.
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
    }

    public void BeliSapiSpesifik(int hargaSapi)
    {
        if (jumlahSapiDibeli >= 5) return;

        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaSapi)
            {
                MoneyManager.instance.RemoveMoney(hargaSapi);
                
                if (jumlahSapiDibeli < listSapi3D.Count && listSapi3D[jumlahSapiDibeli] != null)
                {
                    listSapi3D[jumlahSapiDibeli].SetActive(true);
                }

                jumlahSapiDibeli++;
                if (txtJumlahSapi != null) 
                {
                    txtJumlahSapi.text = "Ternak Sapi\n(x" + (5 - jumlahSapiDibeli) + ")";
                }

                if (jumlahSapiDibeli >= 5)
                {
                    if (btnBeliSapi != null)
                    {
                        btnBeliSapi.interactable = true;
                        Image imgBtn = btnBeliSapi.GetComponent<Image>();
                        if (imgBtn != null && spriteSapiSoldOut != null) imgBtn.sprite = spriteSapiSoldOut;
                    }
                }

                JalankanEfekAudioDanKoin(hargaSapi);
                
                if (TaskManager.instance != null) TaskManager.instance.NotifyHewanDibeli();
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
    }

    public void BeliKambingSpesifik(int hargaKambing)
    {
        if (jumlahKambingDibeli >= 5) return;

        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaKambing)
            {
                MoneyManager.instance.RemoveMoney(hargaKambing);
                
                if (jumlahKambingDibeli < listKambing3D.Count && listKambing3D[jumlahKambingDibeli] != null)
                {
                    listKambing3D[jumlahKambingDibeli].SetActive(true);
                }

                jumlahKambingDibeli++;
                if (txtJumlahKambing != null) 
                {
                    txtJumlahKambing.text = "Ternak Kambing\n(x" + (5 - jumlahKambingDibeli) + ")";
                }

                if (jumlahKambingDibeli >= 5)
                {
                    if (btnBeliKambing != null)
                    {
                        btnBeliKambing.interactable = true;
                        Image imgBtn = btnBeliKambing.GetComponent<Image>();
                        if (imgBtn != null && spriteKambingSoldOut != null) imgBtn.sprite = spriteKambingSoldOut;
                    }
                }

                JalankanEfekAudioDanKoin(hargaKambing);
                
                if (TaskManager.instance != null) TaskManager.instance.NotifyHewanDibeli();
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            PerbaruiTampilanToko(); 

            // Cek jika sudah level 3
            bool sudahLevel3 = (Level3Manager.instance != null) && Level3Manager.instance.isBabak3Aktif;
            if (sudahLevel3)
            {
                // 🔥 Hentikan coroutine yang lama jika ada untuk menghindari penumpukan
                if (delayBukaTokoCoroutine != null) StopCoroutine(delayBukaTokoCoroutine);
                
                // 🔥 Jalankan coroutine jeda sebelum membuka panel
                delayBukaTokoCoroutine = StartCoroutine(JalankanJedaBukaToko());
            }
        }
    }
    IEnumerator JalankanJedaBukaToko()
    {
        // Menunggu selama beberapa detik memberikan waktu player transisi ke animasi idle
        yield return new WaitForSeconds(jedaBukaPanel);
        
        // Pastikan player masih berada di dalam area collider saat jeda selesai
        if (isPlayerInside)
        {
            TombolTokoHUDDiKlik();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // 🔥 Batalkan jeda buka panel jika player terlanjur keluar area sebelum waktu habis
            if (delayBukaTokoCoroutine != null) 
            {
                StopCoroutine(delayBukaTokoCoroutine);
            }

            // if (iconTokoHUD != null) iconTokoHUD.SetActive(false);
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

    public void BeliPakanRumput(int hargaPaketPakan)
    {
        if (MoneyManager.instance != null)
        {
            if (MoneyManager.instance.totalMoney >= hargaPaketPakan)
            {
                MoneyManager.instance.RemoveMoney(hargaPaketPakan);
                Debug.Log("<color=green>[Toko]</color> Berhasil membeli Paket Makanan! Sisa Koin: " + MoneyManager.instance.totalMoney);
                JalankanEfekAudioDanKoin(hargaPaketPakan);

                if (InventoryManager.instance != null) {
                    InventoryManager.instance.AddPakanDariToko();
                }

                if (TaskManager.instance != null) TaskManager.instance.NotifyBeliPakan();
            }
            else
            {
                BukaPanelUangKurang();
            }
        }
    }

    public void UpdateVisualHewanBerdasarkanJumlah(int totalSapi, int totalKambing)
    {
        if (listSapi3D.Count >= 5)
        {
            if (totalSapi >= 30 && listSapi3D[1] != null) listSapi3D[1].SetActive(true);
            if (totalSapi >= 50 && listSapi3D[2] != null) listSapi3D[2].SetActive(true);
            if (totalSapi >= 80 && listSapi3D[3] != null) listSapi3D[3].SetActive(true);
            if (totalSapi >= 100 && listSapi3D[4] != null) listSapi3D[4].SetActive(true);
        }

        if (listKambing3D.Count >= 5)
        {
            if (totalKambing >= 30 && listKambing3D[1] != null) listKambing3D[1].SetActive(true);
            if (totalKambing >= 50 && listKambing3D[2] != null) listKambing3D[2].SetActive(true);
            if (totalKambing >= 80 && listKambing3D[3] != null) listKambing3D[3].SetActive(true);
            if (totalKambing >= 100 && listKambing3D[4] != null) listKambing3D[4].SetActive(true);
        }
    }
}