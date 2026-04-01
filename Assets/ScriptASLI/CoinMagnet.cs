using UnityEngine;

public class CoinMagnet : MonoBehaviour {
    public Transform target;
    public float speed = 0f;
    public float acceleration = 18f; // Semakin tinggi makin cepat nariknya

    void Update() {
        if (target == null) return;

        // Kecepatan makin lama makin tinggi (akselerasi)
        speed += acceleration * Time.deltaTime;

        // Gerakkan koin ke arah Player (sedikit ke atas pinggang player)
        Vector3 targetPos = target.position + new Vector3(0, 0.8f, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f) {
            // --- INI YANG DITAMBAHKAN ---
            if (InventoryManager.instance != null) {
                InventoryManager.instance.AddWood(1);
            }
            // ----------------------------
            PlayerMovement playerScript = target.GetComponent<PlayerMovement>();
            if (playerScript != null) {
                playerScript.StopHarvesting();
            }
            Destroy(gameObject);
        }
    }
}