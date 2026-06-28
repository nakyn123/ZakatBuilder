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
    private bool sudahDiambil = false; 

    // 🔥 BARU: Menyimpan koordinat tempat batu hancur agar text muncul di sana
    private Vector3 spawnLocation; 

    void Start()
    {
        startPosition = transform.position;
        if (GetComponent<MiningCoinMagnet>() != null) {
            isTargetedByMagnet = true;
        }
    }

    // 🔥 BARU: Fungsi pembantu untuk menerima data koordinat dari MiningScript
    public void SetSpawnLocation(Vector3 pos)
    {
        spawnLocation = pos;
    }

    void Update()
    {
        // 🔥 MODIFIKASI: Jika koin baru meloncat (belum diserap sepenuhnya/kinematic), 
        // putar koin jauh lebih cepat (dikali 4) agar memberi efek visual putaran udara yang mantap!
        Rigidbody rb = GetComponent<Rigidbody>();
        float currentRotateSpeed = (rb != null && !rb.isKinematic) ? rotateSpeed * 4f : rotateSpeed;

        transform.Rotate(Vector3.up * currentRotateSpeed * Time.deltaTime);
        
        if (!isTargetedByMagnet) {
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

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

        // 🔥 TAMBAHAN UTAMA: Tambah progress misi tambang babak 2 saat koin diserap tubuh player!
        if (TaskManager.instance != null) {
            int progressTerbaru = TaskManager.instance.totalLogamMinedCount + 1;
            TaskManager.instance.UpdateTambangLogamProgress(progressTerbaru);
        }

        // 🔥 MODIFIKASI UTAMA: Floating Text dipindahkan ke posisi spawnLocation (bekas batu hancur)
        if (floatingTextPrefab != null)
        {
            // Jika data belum terisi, default-nya pakai posisi koin saat ini
            Vector3 textSpawnPos = (spawnLocation != Vector3.zero) ? spawnLocation + new Vector3(0f, 1f, 0f) : transform.position;
            
            GameObject textObj = Instantiate(floatingTextPrefab, textSpawnPos, Quaternion.identity);
            textObj.GetComponent<FloatingText>().SetText("+" + jumlahGram + " gr");
        }

        Destroy(gameObject);
    }

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