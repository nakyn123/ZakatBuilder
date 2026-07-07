using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TeksPlusKadesAnim : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI txtPlus;
    public Image imgKoin; // 🌟 TAMBAHAN: Untuk memegang gambar ikon emas melayang

    private float kecepatanJatuh = 60f; // Kecepatan gerak turun pixel UI
    private float durasi = 1.0f;        // Durasi melayang sebelum hancur

    public void SetupTeksPlus(string teksHadiah)
    {
        if (txtPlus != null)
        {
            txtPlus.text = teksHadiah;
            // txtPlus.color = Color.green; // Warna hijau subur reward
        }

        StartCoroutine(JalankanAnimasiJatuhMudar());
    }

    IEnumerator JalankanAnimasiJatuhMudar()
    {
        float timer = 0;
        RectTransform rectTransform = GetComponent<RectTransform>();
        
        // Simpan warna awal komponen teks dan gambar
        Color warnaTeksAwal = (txtPlus != null) ? txtPlus.color : Color.green;
        Color warnaKoinAwal = (imgKoin != null) ? imgKoin.color : Color.white;

        // Efek Pop (Membesar sebentar di awal)
        transform.localScale = Vector3.zero;
        float popTimer = 0;
        while (popTimer < 0.1f)
        {
            popTimer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, popTimer / 0.1f);
            yield return null;
        }
        transform.localScale = Vector3.one;

        // Animasi bergerak turun + memudar secara halus
        while (timer < durasi)
        {
            timer += Time.deltaTime;
            float t = timer / durasi;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += Vector2.down * kecepatanJatuh * Time.deltaTime;
            }

            // Efek memudar bersama (Fade Out transparansi alpha ke 0)
            if (txtPlus != null)
            {
                txtPlus.color = new Color(warnaTeksAwal.r, warnaTeksAwal.g, warnaTeksAwal.b, Mathf.Lerp(1, 0, t));
            }
            if (imgKoin != null)
            {
                imgKoin.color = new Color(warnaKoinAwal.r, warnaKoinAwal.g, warnaKoinAwal.b, Mathf.Lerp(1, 0, t));
            }

            yield return null;
        }

        // Hancurkan objek setelah selesai
        Destroy(gameObject);
    }
}