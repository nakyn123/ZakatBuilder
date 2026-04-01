using UnityEngine;

public class UICoinEffect : MonoBehaviour {
    private RectTransform target;
    private int value;
    
    private float speed = 0f;
    private Vector3 velocity; 
    private float friction = 0.92f; 
    private float flyDelay;
    private bool isFlying = false;

    public void Init(RectTransform targetTransform, int moneyValue) {
        target = targetTransform;
        value = moneyValue;
        
        // --- EFEK LEDAKAN AWAL ---
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float force = Random.Range(200f, 500f); // Kita kurangi dikit biar gak terlalu jauh muncratnya
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * force;

        // --- DICEPETIN: Jeda sebelum terbang dikurangi (sebelumnya 0.4 - 0.8) ---
        flyDelay = Random.Range(0.15f, 0.3f); 
        
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.15f);
    }

    void Update() {
        if (target == null) return;

        if (!isFlying) {
            transform.position += velocity * Time.deltaTime;
            velocity *= friction;

            flyDelay -= Time.deltaTime;
            if (flyDelay <= 0) isFlying = true;
        } else {
            // --- DICEPETIN: Akselerasi ditambah (sebelumnya 2500) ---
            speed += 5000f * Time.deltaTime; 
            
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // Jarak deteksi diperbesar biar pas deket langsung "nyedot"
            if (Vector3.Distance(transform.position, target.position) < 30f) {
                if(MoneyManager.instance != null) MoneyManager.instance.AddMoney(value);
                Destroy(gameObject);
            }
        }
    }
}