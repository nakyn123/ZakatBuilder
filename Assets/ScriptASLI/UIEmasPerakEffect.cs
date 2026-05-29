using UnityEngine;

public class UIEmasPerakEffect : MonoBehaviour {
    public enum TypeLogam { Emas, Perak }
    
    private RectTransform target;
    private int amountValue;
    private TypeLogam jenisLogam;
    
    private float speed = 0f;
    private Vector3 velocity; 
    private float friction = 0.92f; 
    private float flyDelay;
    private bool isFlying = false;

    public void InitEffect(RectTransform targetTransform, int value, TypeLogam tipe) {
        target = targetTransform;
        amountValue = value;
        jenisLogam = tipe;
        
        // Efek muncrat ledakan awal di layar
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float force = Random.Range(200f, 400f); 
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * force;

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
            speed += 5000f * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 30f) {
                // TAMBAHKAN KE INDIKATOR YANG TEPAT
                if (MoneyManager.instance != null) {
                    if (jenisLogam == TypeLogam.Emas)
                        MoneyManager.instance.AddEmas(amountValue);
                    else
                        MoneyManager.instance.AddPerak(amountValue);
                }
                Destroy(gameObject);
            }
        }
    }
}