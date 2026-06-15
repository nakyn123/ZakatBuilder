using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZakatEmasPerakPanel : MonoBehaviour
{
    [Header("UI Kuis BG Elements")]
    public GameObject panelKuisBG;
    public Button btnLanjutKuis;
    
    [Tooltip("Masukkan SEMUA tombol jawaban kuis (benar dan salah) yang ada di panel kuis ke sini")]
    public List<Button> allQuizButtons; 

    [Tooltip("Masukkan hanya tombol jawaban yang BENAR saja")]
    public List<Button> correctButtons; 
    
    private HashSet<Button> selectedCorrectButtons = new HashSet<Button>();

    [Header("UI Form Kuis Elements")]
    public GameObject panelFormKuis;
    public TMP_Text txtHartaku; 
    [Tooltip("Tarik objek 'teks2 (1)' yang ada di bawah tulisan harta ke sini untuk merubah teks deskripsi secara dinamis")]
    public TMP_Text txtDeskripsiZakat; 

    [Header("Dynamic Input Fields")]
    public GameObject containerZakatEmas;  
    public GameObject containerZakatPerak; 
    public TMP_InputField inputZakatEmas;   
    public TMP_InputField inputZakatPerak;  

    [Header("Action Buttons")]
    public Button btnClose;
    public Button btnSelesaiKuis; 
    public Button btnTutupReward;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip rewardBacksound;

    [Header("Reward Panel")]
    public GameObject panelReward;

    [HideInInspector] public bool isEmasWajib = false;
    [HideInInspector] public bool isPerakWajib = false;

    void Start()
    {
        // Fungsi Close khusus untuk Form Kuis / Batal Bayar (Hanya menutup dirinya sendiri)
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners(); 
            btnClose.onClick.AddListener(() => {
                if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
                else gameObject.SetActive(false);
            });
        }

        if (btnLanjutKuis != null)
        {
            btnLanjutKuis.onClick.RemoveAllListeners(); 
            btnLanjutKuis.onClick.AddListener(BukaFormKuis);
            btnLanjutKuis.interactable = false;
        }

        // Fungsi khusus untuk menutup panel REWARD dan pindah ke level 3
        if (btnTutupReward != null)
        {
            btnTutupReward.onClick.RemoveAllListeners();
            btnTutupReward.onClick.AddListener(TutupRewardDanMatikanAudio);
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        SetupAllQuizButtons();
    }

    void OnEnable()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(true);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (btnLanjutKuis != null) btnLanjutKuis.interactable = false;
        selectedCorrectButtons.Clear();

        foreach (Button btn in allQuizButtons)
        {
            if (btn != null) btn.GetComponent<Image>().color = Color.white;
        }

        ConfigureFormDinamis();
    }

    void SetupAllQuizButtons()
    {
        foreach (Button btn in allQuizButtons)
        {
            if (btn == null) continue;

            bool isCorrectAnswer = correctButtons.Contains(btn);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerSelected(btn, isCorrectAnswer));
        }
    }

    public void OnAnswerSelected(Button clickedBtn, bool isCorrect)
    {
        if (isCorrect)
        {
            if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
            clickedBtn.GetComponent<Image>().color = Color.green;
            
            selectedCorrectButtons.Add(clickedBtn);

            if (selectedCorrectButtons.Count == correctButtons.Count)
            {
                if (btnLanjutKuis != null) btnLanjutKuis.interactable = true;
            }
        }
        else
        {
            if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);
            StartCoroutine(WrongAnswerEffect(clickedBtn));
        }
    }

    IEnumerator WrongAnswerEffect(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        Color origColor = Color.white;
        Vector3 origPos = btn.transform.localPosition;

        img.color = Color.red;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            float x = Random.Range(-1f, 1f) * 5f;
            btn.transform.localPosition = new Vector3(origPos.x + x, origPos.y, origPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        btn.transform.localPosition = origPos;
        img.color = origColor;
    }

    public void ConfigureFormDinamis()
    {
        if (MoneyManager.instance == null || JurnalManager.instance == null) return;

        int emasSekarang = MoneyManager.instance.totalEmas;
        int perakSekarang = MoneyManager.instance.totalPerak;

        isEmasWajib = emasSekarang >= JurnalManager.instance.nisabEmasKriteria;
        isPerakWajib = perakSekarang >= JurnalManager.instance.nisabPerakKriteria;

        if (isEmasWajib && isPerakWajib)
        {
            txtHartaku.text = $"Emas : {emasSekarang} Gram\nPerak : {perakSekarang} Gram";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "dari total simpanan emas dan perak sebesar 2.5% pada tahun ini sejumlah :";
        }
        else if (isEmasWajib)
        {
            txtHartaku.text = $"Emas : {emasSekarang} Gram";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "dari total simpanan emas sebesar 2.5% pada tahun ini sejumlah :";
        }
        else if (isPerakWajib)
        {
            txtHartaku.text = $"Perak : {perakSekarang} Gram";
            if (txtDeskripsiZakat != null) 
                txtDeskripsiZakat.text = "dari total simpanan perak sebesar 2.5% pada tahun ini sejumlah :";
        }

        if (containerZakatEmas != null) containerZakatEmas.SetActive(isEmasWajib);
        if (containerZakatPerak != null) containerZakatPerak.SetActive(isPerakWajib);

        if (inputZakatEmas != null) inputZakatEmas.text = "";
        if (inputZakatPerak != null) inputZakatPerak.text = "";
    }

    void BukaFormKuis()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(true);

        ConfigureFormDinamis();
    }

    public void ValidateZakatEmasPerak()
    {
        if (MoneyManager.instance == null) return;

        bool emasValid = true;
        bool perakValid = true;

        float correctEmasAmount = MoneyManager.instance.totalEmas * 0.025f;
        float correctPerakAmount = MoneyManager.instance.totalPerak * 0.025f;

        if (isEmasWajib && inputZakatEmas != null)
        {
            string cleanEmas = inputZakatEmas.text.Replace(",", ".");
            if (float.TryParse(cleanEmas, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float guessEmas))
            {
                emasValid = Mathf.Abs(guessEmas - correctEmasAmount) < 0.1f;
            }
            else emasValid = false;
        }

        if (isPerakWajib && inputZakatPerak != null)
        {
            string cleanPerak = inputZakatPerak.text.Replace(",", ".");
            if (float.TryParse(cleanPerak, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float guessPerak))
            {
                perakValid = Mathf.Abs(guessPerak - correctPerakAmount) < 0.1f;
            }
            else perakValid = false;
        }

        if (emasValid && perakValid)
        {
            if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);
            
            if (JurnalManager.instance != null)
            {
                JurnalManager.instance.isEmasLockedInJurnal = true;
            }
            
            if (isEmasWajib && inputZakatEmas != null)
            {
                string cleanEmas = inputZakatEmas.text.Replace(",", ".");
                if (int.TryParse(cleanEmas, out int amountEmasToPay))
                    MoneyManager.instance.RemoveEmas(amountEmasToPay);
                else
                    MoneyManager.instance.RemoveEmas(Mathf.RoundToInt(MoneyManager.instance.totalEmas * 0.025f));
            }

            if (isPerakWajib && inputZakatPerak != null)
            {
                string cleanPerak = inputZakatPerak.text.Replace(",", ".");
                if (int.TryParse(cleanPerak, out int amountPerakToPay))
                    MoneyManager.instance.RemovePerak(amountPerakToPay);
                else
                    MoneyManager.instance.RemovePerak(Mathf.RoundToInt(MoneyManager.instance.totalPerak * 0.025f));
            }

            if (panelReward != null) 
            {
                panelReward.SetActive(true); 
            }
            if (audioSource != null && rewardBacksound != null)
            {
                audioSource.PlayOneShot(rewardBacksound);
            }

            if (panelFormKuis != null) 
            {
                panelFormKuis.SetActive(false); 
            }
        }
        else
        {
            if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
        }
    }

    void TutupRewardDanMatikanAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isEmasPerakCompleted = true;
            ZakatPanelManager.instance.UpdateCheckmarkVisuals();
            ZakatPanelManager.instance.UpdatePaymentButtonVisual();
        }

        // Langsung panggil perpindahan Level 3 utuh
        if (Level3Manager.instance != null)
        {
            Level3Manager.instance.TutupRewardDanMasukLevel3();
        }

        if (panelReward != null) panelReward.SetActive(false);
        if (UIManager.instance != null) UIManager.instance.ClosePanelMenu(gameObject);
        else gameObject.SetActive(false);
    }
}