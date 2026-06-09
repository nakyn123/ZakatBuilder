using UnityEngine;

public class AnimalWanderAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;          // Kecepatan jalan hewan
    public float rotationSpeed = 5f;      // Kecepatan berbelok
    
    [Header("Obstacle Avoidance")]
    public float detectionDistance = 2.5f; // Jarak sensor mendeteksi pagar
    public LayerMask obstacleLayer;       // Layer pagar (pilih layer Obstacle nanti)
    
    [Header("Wander Timer")]
    public float minWanderTime = 3f;      // Waktu minimal jalan ke satu arah
    public float maxWanderTime = 6f;      // Waktu maksimal jalan ke satu arah

    private float currentWanderTimer;
    private Quaternion targetRotation;
    private bool isTurning = false;

    void Start()
    {
        // TAMBAHKAN KODE INI:
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Menyuapi parameter 'OffsetLompat' di Animator dengan angka acak antara 0.0 sampai 1.0
            anim.SetFloat("OffsetLompat", Random.Range(0f, 1f));
        }

        GetNewDirection();
    }

    void Update()
    {
        // 1. Kirim laser sensor ke arah depan hewan
        RaycastHit hit;
        Vector3 forwardDirection = transform.forward;

        // Visualisasi laser di Scene View (hijau = aman, merah = deteksi pagar)
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, forwardDirection * detectionDistance, isTurning ? Color.red : Color.green);

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forwardDirection, out hit, detectionDistance, obstacleLayer, QueryTriggerInteraction.Collide))
        {
            // Jika mendeteksi pagar di depannya, langsung paksa cari arah baru
            if (!isTurning)
            {
                isTurning = true;
                GetNewDirection();
            }
        }

        // 2. Logika rotasi (Berbelok halus menuju arah target)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 3. Logika berjalan maju (Hanya maju jika sudut berbeloknya sudah hampir selesai)
        if (Quaternion.Angle(transform.rotation, targetRotation) < 30f)
        {
            isTurning = false;
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        // 4. Hitung mundur waktu ganti arah otomatis (supaya jalannya acak walaupun gak nabrak)
        currentWanderTimer -= Time.deltaTime;
        if (currentWanderTimer <= 0)
        {
            GetNewDirection();
        }
    }

    void GetNewDirection()
    {
        // Tentukan sudut acak baru (0 sampai 360 derajat)
        float randomAngle = Random.Range(0f, 360f);
        targetRotation = Quaternion.Euler(0, randomAngle, 0);

        // Reset ulang timer acaknya
        currentWanderTimer = Random.Range(minWanderTime, maxWanderTime);
    }
}