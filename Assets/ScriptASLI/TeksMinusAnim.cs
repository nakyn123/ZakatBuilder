using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TeksMinusAnim : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI txtMinus;
    public Image imgKoin;

    private float kecepatanJatuh = 80f; // Kecepatan gerak turun pixel UI
    private float durasi = 1.0f;        // Durasi sebelum menghilang

    public void SetupTeksMinus(string teksHarga)
    {
        if (txtMinus != null)
        {
            txtMinus.text = teksHarga;
        }

        StartCoroutine(JalankanAnimasiJatuhMudar());
    }

    IEnumerator JalankanAnimasiJatuhMudar()
    {
        float timer = 0;
        
        // Ambil warna awal untuk efek Fade Out
        Color warnaTeksAwal = (txtMinus != null) ? txtMinus.color : Color.red;
        Color warnaKoinAwal = (imgKoin != null) ? imgKoin.color : Color.white;

        // Sedikit efek pop (membesar sebentar di awal)
        transform.localScale = Vector3.zero;
        float popTimer = 0;
        while (popTimer < 0.1f)
        {
            popTimer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, popTimer / 0.1f);
            yield return null;
        }
        transform.localScale = Vector3.one;

        // Animasi bergerak turun + memudar
        while (timer < durasi)
        {
            timer += Time.deltaTime;
            float t = timer / durasi;

            // Gerakkan objek ke bawah perlahan
            transform.localPosition += Vector3.down * kecepatanJatuh * Time.deltaTime;

            // Efek memudar (Fade Out transparansi alpha ke 0)
            if (txtMinus != null)
            {
                txtMinus.color = new Color(warnaTeksAwal.r, warnaTeksAwal.g, warnaTeksAwal.b, Mathf.Lerp(1, 0, t));
            }
            if (imgKoin != null)
            {
                imgKoin.color = new Color(warnaKoinAwal.r, warnaKoinAwal.g, warnaKoinAwal.b, Mathf.Lerp(1, 0, t));
            }

            yield return null;
        }

        // Hancurkan objek setelah animasi selesai
        Destroy(gameObject);
    }
}