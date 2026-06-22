using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel; // Seret 'pause-panel' ke sini

    [Header("Buttons")]
    [SerializeField] private Button pauseBtn;       // Seret 'pause-btn' ke sini
    [SerializeField] private Button resumeBtn;      // Seret 'resume-btn' ke sini
    [SerializeField] private Button backToHomeBtn;   // Seret 'backtohome-btn' ke sini

    [Header("Audio Settings (Tambahan Baru)")]
    [SerializeField] private AudioSource audioSourcePause; // Tempelkan komponen AudioSource di sini
    [SerializeField] private AudioClip suaraKlikTombol;    // Tarik file audio klik/SFX biasa ke sini
    [SerializeField] private AudioClip suaraBukaPause;     // Tarik file audio khusus saat buka panel jeda (jika ada)

    [Header("Scene Destination")]
    [SerializeField] private string homeSceneName = "HomeScene"; // Ganti dengan nama scene Home-mu

    private void Start()
    {
        // Pastikan panel tertutup saat game dimulai
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Menghubungkan fungsi tombol via kode (lebih aman daripada manual di Inspector)
        if (pauseBtn != null) pauseBtn.onClick.AddListener(PauseGame);
        if (resumeBtn != null) resumeBtn.onClick.AddListener(ResumeGame);
        if (backToHomeBtn != null) backToHomeBtn.onClick.AddListener(GoToHome);
    }

    public void PauseGame()
    {
        // 🔊 MAINKAN SUARA: Efek suara saat game dijeda
        PlaySound(suaraBukaPause != null ? suaraBukaPause : suaraKlikTombol);

        if (pausePanel != null)
        {
            pausePanel.SetActive(true); // Memunculkan panel jeda beserta bg blur di dalamnya
            Time.timeScale = 0f;        // Menghentikan waktu/aktivitas game di background
        }
    }

    public void ResumeGame()
    {
        // 🔊 MAINKAN SUARA: Efek suara saat klik lanjutkan
        PlaySound(suaraKlikTombol);

        if (pausePanel != null)
        {
            pausePanel.SetActive(false); // Menutup panel jeda
            Time.timeScale = 1f;         // Mengembalikan waktu game normal kembali
        }
    }

    public void GoToHome()
    {
        PlaySound(suaraKlikTombol); //

        // 🏃‍♂️ PERINTAHKAN PLAYER UNTUK SIMPAN POSISINYA
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.SimpanPosisiPlayer();
        }

        // 💾 AUTO-SAVE MISI DAN HARTA: Amankan progress terakhir (misi, koin, emas) sebelum keluar
        if (TaskManager.instance != null)
        {
            TaskManager.instance.SimpanProgressGameKeKomputer(); //
        }

        Time.timeScale = 1f; // Kembalikan waktu normal
        SceneManager.LoadScene(homeSceneName); // Berpindah ke scene home
    }

    // Fungsi pembantu untuk memutar audio agar kode di atas tidak duplikat/berantakan
    private void PlaySound(AudioClip clip)
    {
        if (audioSourcePause != null && clip != null)
        {
            audioSourcePause.PlayOneShot(clip);
        }
    }

    private void OnDestroy()
    {
        // Membersihkan listener saat objek dihancurkan untuk menghindari memory leak
        if (pauseBtn != null) pauseBtn.onClick.RemoveListener(PauseGame);
        if (resumeBtn != null) resumeBtn.onClick.RemoveListener(ResumeGame);
        if (backToHomeBtn != null) backToHomeBtn.onClick.RemoveListener(GoToHome);
    }
}