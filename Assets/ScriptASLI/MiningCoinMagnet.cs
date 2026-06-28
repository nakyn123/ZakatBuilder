using UnityEngine;

public class MiningCoinMagnet : MonoBehaviour {
    public Transform target;
    public float speed = 0.5f;          
    public float acceleration = 8f;  

    private bool sudahDiserap = false;

    void Update() {
        if (target == null || sudahDiserap) return;

        speed += acceleration * Time.deltaTime;
        
        // Target melesat lurus mengarah ke KEPALA player (+ 1.7 unit ke atas)
        Vector3 targetPos = target.position + new Vector3(0f, 1.7f, 0f);
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        float jarakKeKepala = Vector3.Distance(transform.position, targetPos);

        // Jika jarak sudah mulai dekat dengan player (di bawah 1.5 meter)
        if (jarakKeKepala <= 1.5f) {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false; 

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = true; 
                // 🔥 KUNCI PERBAIKAN UNITY 6: Menggunakan linearVelocity dan Vector3.zero yang benar
                rb.linearVelocity = Vector3.zero; 
            }
        }

        // Jika koin sudah sampai tepat di kepala player
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