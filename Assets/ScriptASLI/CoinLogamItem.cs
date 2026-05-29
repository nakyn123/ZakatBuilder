using UnityEngine;

public class CoinLogamItem : MonoBehaviour
{
    public enum JenisLogam { Emas, Perak }

    [Header("Settings Logam")]
    public JenisLogam jenisLogam; 
    public int jumlahGram = 10; 

    [Header("Movement Settings")]
    public float rotateSpeed = 100f;
    public float floatSpeed = 2f;    
    public float floatHeight = 0.2f; 

    [Header("Effect Pickup")]
    public GameObject floatingTextPrefab; 
    // 🔥 TAMBAHKAN VARIABEL UTK AUDIO CLIP DI BAWAH INI
    public AudioClip coinSoundEffect; 

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Jalankan Efek Suara di posisi koin sebelum dihancurkan
            if (coinSoundEffect != null)
            {
                // Angka 1f di ujung adalah volume suara (0f sampai 1f)
                AudioSource.PlayClipAtPoint(coinSoundEffect, transform.position, 1f);
            }

            // 2. Tambah Nilai ke MoneyManager
            if (MoneyManager.instance != null)
            {
                if (jenisLogam == JenisLogam.Emas) MoneyManager.instance.totalEmas += jumlahGram;
                else if (jenisLogam == JenisLogam.Perak) MoneyManager.instance.totalPerak += jumlahGram;

                MoneyManager.instance.UpdateEmasPerakUI();
            }

            // 3. Sinkronisasi Teks ke Level2Manager
            if (Level2Manager.instance != null)
            {
                if (jenisLogam == JenisLogam.Emas && Level2Manager.instance.txtEmasUtama != null)
                {
                    Level2Manager.instance.txtEmasUtama.text = MoneyManager.instance.totalEmas + " gr";
                }
                else if (jenisLogam == JenisLogam.Perak && Level2Manager.instance.txtPerakUtama != null)
                {
                    Level2Manager.instance.txtPerakUtama.text = MoneyManager.instance.totalPerak + " gr";
                }
            }

            if (JurnalManager.instance != null)
            {
                JurnalManager.instance.CheckEmasPerakNisab();
            }

            // 4. Munculkan Popup Floating Text
            if (floatingTextPrefab != null)
            {
                Vector3 spawnPosition = transform.position; 
                spawnPosition.y += 0.8f; 
                GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
                textObj.GetComponent<FloatingText>().SetText("+" + jumlahGram + " gr");
            }

            // 5. Hancurkan objek koin
            Destroy(gameObject); //[cite: 2]
        }
    }
}