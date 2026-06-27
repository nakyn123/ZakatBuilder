using UnityEngine;

public class CoinMagnet : MonoBehaviour {
    public Transform target;
    public float speed = 0f;
    public float acceleration = 18f;
    
    // TAMBAHKAN INI: Untuk menyimpan tipe kayu dari koin ini
    [HideInInspector] public int woodType; 

    void Update() {
        if (target == null) return;

        speed += acceleration * Time.deltaTime;
        Vector3 targetPos = target.position + new Vector3(0, 0.8f, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f) {
            if (InventoryManager.instance != null) {
                InventoryManager.instance.AddWood(1, woodType);
            }

            PlayerMovement playerScript = target.GetComponent<PlayerMovement>();
            if (playerScript != null) {
                // 🔥 SINKRONISASI AKHIR: Matikan animasi apa pun yang sedang aktif saat koin masuk ke tubuh
                playerScript.StopHarvesting();
                playerScript.StopMining(); // Matikan beliung dan tegakkan badan jika sedang menambang!
            }
            Destroy(gameObject);
        }
    }
}