using UnityEngine;

public class SapiWanderAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;          // Kecepatan jalan sapi
    public float rotationSpeed = 2.5f;    // Kecepatan berbelok (Slerp)
    
    [Header("Obstacle Avoidance")]
    public float detectionDistance = 2.5f; // Jarak sensor mendeteksi pagar
    public LayerMask obstacleLayer;       // Layer pagar/Obstacle

    [Header("Player Avoidance (Advanced)")]
    public Transform playerTransform;      // Object Player kamu
    public float safeDistanceFromPlayer = 3.5f; // Jarak aman (Sapi mulai putar balik)

    [Header("Wander Timer")]
    public float minWanderTime = 3f;      
    public float maxWanderTime = 6f;      

    private float currentWanderTimer;
    private Quaternion targetRotation; // Menggunakan Quaternion seperti AnimalWander
    private bool isTurning = false;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        
        if (anim != null)
        {
            anim.SetFloat("OffsetLompat", Random.Range(0f, 1f));
        }

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        GetNewDirection();
    }

    void Update()
    {
        bool needToTurn = false;
        float distanceToPlayer = float.MaxValue;

        // 1. Cek Jarak ke Player (Fitur Jarak Advanced Sapi)
        if (playerTransform != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < safeDistanceFromPlayer)
            {
                needToTurn = true;
            }
        }

        // 2. Sensor Deteksi Pagar (Hanya aktif jika tidak sedang dekat player)
        if (!needToTurn) 
        {
            RaycastHit hit;
            Vector3 forwardDirection = transform.forward;
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, forwardDirection * detectionDistance, isTurning ? Color.red : Color.green);

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forwardDirection, out hit, detectionDistance, obstacleLayer, QueryTriggerInteraction.Collide))
            {
                needToTurn = true;
            }
        }

        // Jika mendeteksi pagar atau player terlalu dekat, cari arah baru
        if (needToTurn)
        {
            if (!isTurning)
            {
                isTurning = true;
                GetNewDirection();
            }
        }

        // =======================================================================
        // ADAPTASI LOGIKA ANIMALWANDERAI (ROTASI & TRANSLATE)
        // =======================================================================
        
        // Hitung sudut sisa rotasi untuk menyuapi parameter Animator
        Vector3 localTarget = transform.InverseTransformDirection(targetRotation * Vector3.forward);
        float turnAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float turnInput = Mathf.Clamp(turnAngle / 45f, -1f, 1f);

        if (anim != null)
        {
            anim.SetFloat("TurnSpeed", turnInput);
        }

        // Putar badan sapi secara halus ke target rotasi baru
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Logika berjalan maju asli dari AnimalWander: Hanya maju jika sudut berbeloknya hampir selesai (< 30 derajat)
        if (Quaternion.Angle(transform.rotation, targetRotation) < 30f)
        {
            isTurning = false;
            
            // Murni menggunakan Translate tanpa Rigidbody
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            if (anim != null) anim.SetFloat("MoveSpeed", 1f); // Animasi jalan penuh
        }
        else
        {
            // Saat belok patah, sapi melambat atau diam di tempat tergantung seberapa dekat dengan player
            if (distanceToPlayer < safeDistanceFromPlayer)
            {
                // Jika belok karena player, kurangi maju drastis (atau set ke 0) agar pantat gak nyabet
                transform.Translate(Vector3.forward * (moveSpeed * 0.1f) * Time.deltaTime);
                if (anim != null) anim.SetFloat("MoveSpeed", 0.1f);
            }
            else
            {
                // Jika belok karena pagar biasa, jalan pelan 0.5f seperti biasa
                transform.Translate(Vector3.forward * (moveSpeed * 0.5f) * Time.deltaTime);
                if (anim != null) anim.SetFloat("MoveSpeed", 0.5f);
            }
        }

        // 4. Timer ganti arah otomatis
        currentWanderTimer -= Time.deltaTime;
        if (currentWanderTimer <= 0)
        {
            GetNewDirection();
        }
    }

    void GetNewDirection()
    {
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < safeDistanceFromPlayer)
        {
            // Logika Advanced Sapi: Paksa arah rotasi membelakangi player
            Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
            
            float randomAngleOffset = Random.Range(-20f, 20f) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(randomAngleOffset);
            float sin = Mathf.Sin(randomAngleOffset);
            
            Vector3 finalLookDir = new Vector3(
                directionAwayFromPlayer.x * cos - directionAwayFromPlayer.z * sin,
                0,
                directionAwayFromPlayer.x * sin + directionAwayFromPlayer.z * cos
            ).normalized;

            // Ubah vektor arah menjadi Quaternion target rotasi
            if (finalLookDir != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(finalLookDir);
            }
        }
        else
        {
            // Logika acak normal bawaan AnimalWander
            float randomAngle = Random.Range(0f, 360f);
            targetRotation = Quaternion.Euler(0, randomAngle, 0);
        }

        currentWanderTimer = Random.Range(minWanderTime, maxWanderTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, safeDistanceFromPlayer);
    }
}