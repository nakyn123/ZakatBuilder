using UnityEngine;

public class MiningCoinMagnet : MonoBehaviour {
    public Transform target;
    public float speed = 0.5f;          
    public float acceleration = 15f;  

    private bool sudahDiserap = false;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (target == null || sudahDiserap) return;

        // 🎯 KUNCI UTAMA: Jika player kabur, matikan gravitasi koin agar tidak jatuh ke tanah
        if (rb != null && !rb.isKinematic) {
            rb.useGravity = false;   // Matikan gravitasi agar koin melayang mengejar player
            rb.linearVelocity = Vector3.zero; // Reset gaya lemparan awal fisika Unity 6
            rb.isKinematic = true;   // Paksa menjadi kinematic agar fokus ke MoveTowards
        }

        // Kecepatan bertambah seiring berjalannya waktu
        speed += acceleration * Time.deltaTime;
        
        // Target melesat lurus mengarah ke KEPALA player (+ 1.7 unit ke atas)
        Vector3 targetPos = target.position + new Vector3(0f, 1.7f, 0f);
        
        // Gerakan koin mengejar posisi target secara real-time ke mana pun player pergi
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        float jarakKeKepala = Vector3.Distance(transform.position, targetPos);

        // Jika jarak sudah mulai dekat dengan player (di bawah 1.5 meter)
        if (jarakKeKepala <= 1.5f) {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false; 
        }

        // Jika koin sudah sampai tepat di kepala player
        if (jarakKeKepala < 0.3f) {
            sudahDiserap = true;
            
            CoinLogamItem coinScript = GetComponent<CoinLogamItem>();
            if (coinScript != null) {
                coinScript.AmbilKoinLogam(target);
            }

            PlayerMovement playerScript = target.GetComponent<PlayerMovement>();
            if (playerScript != null) {
                playerScript.StopMining(); 
            }
        }
    }
}