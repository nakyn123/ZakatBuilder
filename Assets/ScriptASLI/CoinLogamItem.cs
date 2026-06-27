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
    public AudioClip coinSoundEffect; 

    private Vector3 startPosition;
    private bool isTargetedByMagnet = false;
    private bool sudahDiambil = false; // Pengaman agar fungsi tidak terduplikasi dalam satu frame

    void Start()
    {
        startPosition = transform.position;
        if (GetComponent<MiningCoinMagnet>() != null) {
            isTargetedByMagnet = true;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        
        if (!isTargetedByMagnet) {
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    // Fungsi utama penambahan harta, sfx, dan teks
    public void AmbilKoinLogam(Transform playerTransform)
    {
        if (sudahDiambil) return;
        sudahDiambil = true;

        if (coinSoundEffect != null)
        {
            AudioSource.PlayClipAtPoint(coinSoundEffect, playerTransform.position, 1f);
        }

        if (MoneyManager.instance != null)
        {
            if (jenisLogam == JenisLogam.Emas) MoneyManager.instance.totalEmas += jumlahGram;
            else if (jenisLogam == JenisLogam.Perak) MoneyManager.instance.totalPerak += jumlahGram;

            MoneyManager.instance.UpdateEmasPerakUI();
        }

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

        if (floatingTextPrefab != null)
        {
            Vector3 spawnPosition = playerTransform.position + new Vector3(0f, 1.7f, 0f); 
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
            textObj.GetComponent<FloatingText>().SetText("+" + jumlahGram + " gr");
        }

        Destroy(gameObject);
    }

    // 🔥 PENGAMAN CADANGAN: Jika koin membentur fisik player di jalan, langsung serap seketika!
    private void OnTriggerEnter(Collider other) 
    {
        if (sudahDiambil) return;
        if (other.CompareTag("Player")) 
        {
            PlayerMovement playerScript = other.GetComponent<PlayerMovement>();
            if (playerScript != null) {
                playerScript.StopMining(); 
            }
            AmbilKoinLogam(other.transform);
        }
    }
}