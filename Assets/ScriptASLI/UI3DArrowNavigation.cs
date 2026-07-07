using UnityEngine;

public class UI3DArrowNavigation : MonoBehaviour
{
    public Transform playerTransform;       // Referensi Player utama di dunia 3D
    public Transform targetDestination;     // Target (Cube Transparan Kantor Zakat)
    public Transform mainCameraTransform;   // Kamera utama game (Cinemachine)

    [Header("Settings")]
    public float rotationSpeed = 8f;
    public float floatSpeed = 4f;
    public float floatAmplitude = 10f; // Naik turun lokal untuk fbx

    private Vector3 startLocalPosition;

    void Start()
    {
        startLocalPosition = transform.localPosition;
        
        // Cari main camera otomatis jika belum diisi
        if (mainCameraTransform == null && Camera.main != null)
            mainCameraTransform = Camera.main.transform;

        gameObject.SetActive(false); // Mati di awal game
    }

    void Update()
{
    if (targetDestination == null || playerTransform == null || mainCameraTransform == null) return;

    // 1. Ambil posisi horizontal Player dan Target (Abaikan tinggi Y)
    Vector3 playerPos = playerTransform.position;
    Vector3 targetPos = targetDestination.position;
    playerPos.y = 0;
    targetPos.y = 0;

    // Arah dari player menuju ke cube tujuan
    Vector3 dirToTarget = (targetPos - playerPos).normalized;

    // 2. Ambil arah horizontal kamera utama game
    Vector3 camForward = mainCameraTransform.forward;
    camForward.y = 0;
    camForward.Normalize();

    // 3. Hitung sudut rotasi yang dibutuhkan (Formula Kompas)
    float camYaw = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
    float targetAngle = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;
    
    // Sudut relatif tujuan terhadap arah hadap kamera saat ini
    // 🌟 TAMBAHKAN + 180f DI SINI UNTUK MEMBALIKKAN ARAH PANAH YANG TERBALIK
    float relativeAngle = targetAngle - camYaw + 180f;

    // 4. TERAPKAN HANYA PADA SUMBU Y (Kunci X dan Z bawaan model fbx kamu)
    float targetYRotation = relativeAngle;
    
    // Gunakan Lerp Angle agar transisi putaran terasa halus
    float currentY = transform.localEulerAngles.y;
    float smoothedY = Mathf.LerpAngle(currentY, targetYRotation, rotationSpeed * Time.deltaTime);

    // Terapkan rotasi akhir dengan sumbu X dan Z yang dikunci sesuai Inspector awalmu
    transform.localEulerAngles = new Vector3(-64.054f, smoothedY, -90f);

    // 5. Efek melayang lembut agar interaktif di UI
    Vector3 newPos = startLocalPosition;
    newPos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    transform.localPosition = newPos;
}

    public void SetTarget(Transform newTarget, Transform player)
    {
        targetDestination = newTarget;
        playerTransform = player;
        gameObject.SetActive(true);
    }

    public void HideArrow()
    {
        targetDestination = null;
        gameObject.SetActive(false);
    }
}