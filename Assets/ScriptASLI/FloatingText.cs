using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1f;
    public TextMeshPro textMesh; // Tarik komponen TextMeshPro (3D) di prefab ke sini

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Bergerak ke atas perlahan
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        
        // Opsional: Membuat teks selalu menghadap ke kamera pemain (Billboarding)
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void SetText(string amount)
    {
        if (textMesh != null) textMesh.text = amount;
    }
}