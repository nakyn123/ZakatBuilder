using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ZakatPerdaganganPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Button btnClose;
    public Button btnLanjut;

    [Header("Questions")]
    public Button answerA1; 
    public Button answerB1;
    public Button answerC1;
    
    public Button answerA2;
    public Button answerB2; 
    public Button answerC2;

    public Button answerA3;
    public Button answerB3; 
    public Button answerC3;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Komponen AudioSource
    public AudioClip correctSound;  // Klip suara benar
    public AudioClip wrongSound;

    [Header("Settings")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    [Header("Panel Transition")]
    public GameObject panelKuisBG;
    public GameObject panelFormKuis;
    public GameObject panelZakatCarousel; // Tambahkan ini jika belum ada
    public GameObject panelRewardDagang;

    void Start()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));

        SetupButton(answerA1, true);
        SetupButton(answerB1, false);
        SetupButton(answerC1, false);

        SetupButton(answerA2, false);
        SetupButton(answerB2, true);
        SetupButton(answerC2, false);

        SetupButton(answerA3, false);
        SetupButton(answerB3, true);
        SetupButton(answerC3, false);
        
        if (btnLanjut != null)
        {
            btnLanjut.onClick.AddListener(BukaFormKuis);
            btnLanjut.interactable = false;
        }
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void BukaFormKuis()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false); // Sembunyikan kuis
        if (panelFormKuis != null) panelFormKuis.SetActive(true); // Tampilkan form
    }

    void SetupButton(Button btn, bool isCorrect)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => OnAnswerSelected(btn, isCorrect));
    }

    void OnAnswerSelected(Button clickedBtn, bool isCorrect)
    {
        Image btnImg = clickedBtn.GetComponent<Image>();
        TMP_Text btnText = clickedBtn.GetComponentInChildren<TMP_Text>();

        if (isCorrect)
        {   PlaySound(correctSound);
            btnImg.color = correctColor;
            // if (btnText != null) btnText.color = Color.white;
            // Jika benar, kita biarkan tetap hijau
            CheckAllAnswers();
        }
        else
        {   PlaySound(wrongSound);
            // Jalankan Coroutine untuk efek sementara
            StartCoroutine(WrongAnswerEffect(clickedBtn, btnImg, btnText));
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Coroutine gabungan untuk warna merah, getar, lalu balik ke putih
    IEnumerator WrongAnswerEffect(Button btn, Image img, TMP_Text text)
    {
        // Simpan data awal
        Color originalColor = Color.white; // Asumsi warna dasar tombol adalah putih
        Color originalTextColor = text != null ? text.color : Color.black; 
        Vector3 originalPos = btn.transform.localPosition;

        // Set ke warna salah
        img.color = wrongColor;
        if (text != null) text.color = Color.white;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            btn.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Kembalikan ke posisi, warna, dan teks semula
        btn.transform.localPosition = originalPos;
        img.color = originalColor;
        if (text != null) text.color = originalTextColor;
    }

    void CheckAllAnswers()
    {
        if (answerA1.GetComponent<Image>().color == correctColor &&
            answerB2.GetComponent<Image>().color == correctColor &&
            answerB3.GetComponent<Image>().color == correctColor)
        {
            if (btnLanjut != null) btnLanjut.interactable = true;
        }
    }
    public void MunculkanReward()
    {
        if (panelKuisBG != null) panelKuisBG.SetActive(false);
        if (panelFormKuis != null) panelFormKuis.SetActive(false);
        if (panelZakatCarousel != null) panelZakatCarousel.SetActive(false);
        
        if (panelRewardDagang != null) panelRewardDagang.SetActive(true);
    }
}