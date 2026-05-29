using UnityEngine;

public class NotificationAnimate : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseScale = 1.15f; // Pembesaran maksimal (1.15 berarti naik 15%)
    public float pulseDuration = 0.5f; // Kecepatan kedap-kedip (detik)

    [Header("Shake Settings")]
    public float shakeAngle = 10f; // Derajat rotasi getar (ke kiri & kanan)
    public float shakeDuration = 0.15f; // Kecepatan getaran (detik)
    public float shakeInterval = 3f; // Jeda waktu bergetar kembali (setiap 3 detik)

    private Vector3 originalScale;
    private LTDescr pulseTween;
    private LTDescr shakeTween;

    void Awake()
    {
        // Simpan scale asli objek di awal
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        // Reset scale dan rotasi saat notifikasi muncul kembali
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        // 1. EFEK KEDAP-KEDIP (Scale Pulse) - Berputar selamanya (PingPong)
        pulseTween = LeanTween.scale(gameObject, originalScale * pulseScale, pulseDuration)
            .setEaseInOutSine()
            .setLoopPingPong();

        // 2. EFEK GETAR (Shake) - Dipanggil berulang setiap sekian detik
        InvokeRepeating(nameof(TriggerShake), 0.5f, shakeInterval);
    }

    void TriggerShake()
    {
        if (!gameObject.activeInHierarchy) return;

        // Ambil rotasi awal sebelum getar
        transform.localRotation = Quaternion.identity;

        // Rotasi ke kiri lalu kanan (PingPong) sebanyak 4 kali, lalu kembali normal
        shakeTween = LeanTween.rotateZ(gameObject, shakeAngle, shakeDuration)
            .setEaseLinear()
            .setLoopPingPong(4)
            .setOnComplete(() => {
                transform.localRotation = Quaternion.identity;
            });
    }

    void OnDisable()
    {
        // Batalkan semua tween dan invoke agar memori bersih saat notifikasi mati
        CancelInvoke(nameof(TriggerShake));
        if (pulseTween != null) LeanTween.cancel(pulseTween.uniqueId);
        if (shakeTween != null) LeanTween.cancel(shakeTween.uniqueId);
    }
}