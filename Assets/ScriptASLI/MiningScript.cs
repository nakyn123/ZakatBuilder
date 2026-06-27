using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MiningScript : MonoBehaviour {
    public enum MiningType { Emas, Perak }

    [Header("Mining Type Settings")]
    public MiningType jenisLogam; 
    public int totalHadiahGram = 10; 

    [Header("UI Settings")]
    public GameObject globalChopGroup; 
    public Sprite[] slotSprites;       

    [Header("Visual Models (Updated to 4 Models)")]
    [Tooltip("Masukkan 4 model urutan batu: 0 = Utuh, 1 = Retak Ringan, 2 = Retak Parah, 3 = Hancur Kecil")]
    public GameObject[] crystalModels; 

    [Header("Effects & Prefabs")]
    public GameObject stoneParticlePrefab; 
    public GameObject coinLogamPrefab;     

    [Header("Audio Settings")]
    public AudioSource miningAudioSource; 
    public AudioClip mineHitSound; 

    private Button globalButton;
    private Image buttonImage;
    private int hitCount = 0; 
    private bool isDestroyed = false; 
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();

    void Start() {
        if (globalChopGroup != null) {
            globalButton = globalChopGroup.GetComponentInChildren<Button>();
            buttonImage = globalButton.GetComponent<Image>();
        }

        foreach (GameObject model in crystalModels) {
            if (model != null) {
                originalScales[model] = model.transform.localScale;
                originalRotations[model] = model.transform.localRotation;
            }
        }
        UpdateVisualBatu(true);
    }

    void OnTriggerEnter(Collider other) {
        if (isDestroyed) return;
        if (other.CompareTag("Player")) {
            if (globalChopGroup != null) {
                globalChopGroup.SetActive(true); 
                UpdateLogo();
                
                // 🔥 Pengaman null check agar tidak crash jika salah isi slot di Inspector
                if (globalButton != null) {
                    globalButton.onClick.RemoveAllListeners(); 
                    globalButton.onClick.AddListener(ActionTambang);
                } else {
                    Debug.LogError($"<color=red>[MiningScript]</color> Tombol UI tidak ditemukan! Pastikan slot Global Chop Group diisi dengan Panel UI dari Canvas, bukan objek 3D.");
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerMovement moveScript = other.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.StopMining();

            if (globalChopGroup != null) {
                if (globalButton != null) globalButton.onClick.RemoveAllListeners();
                globalChopGroup.SetActive(false); 
            }
        }
    }

    void ActionTambang() {
        if (isDestroyed) return;

        if (miningAudioSource != null && mineHitSound != null) {
            miningAudioSource.PlayOneShot(mineHitSound);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerMovement moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) {
                // Klik pertama mengaktifkan beliung dan status mining
                if (hitCount == 0) {
                    moveScript.StartMining(); 
                }

                // 🔥 KUNCI UTAMA: Paksa animator memutar ulang animasi nambang dari frame 0 di SETIAP KLIK
                // "isMining" adalah nama parameter, tapi di sini kita panggil nama "State" animasinya di Animator Controller.
                // Pastikan ganti "Nambang" di bawah ini dengan nama kotak State Animasi nambangmu di Unity Animator!
                if (moveScript.anim != null) {
                    moveScript.anim.Play("Nambang", 0, 0f); 
                }
            }
        }
        
        bool isLastHit = (hitCount == crystalModels.Length - 1); 
        Vector3 spawnPos = GetCenterPosition(crystalModels[hitCount]);

        if (!isLastHit) {
            GameObject modelSekarang = crystalModels[hitCount];
            if (modelSekarang != null) {
                LeanTween.cancel(modelSekarang);
                LeanTween.rotateX(modelSekarang, 5f, 0.05f).setLoopPingPong(2).setOnComplete(() => {
                    modelSekarang.transform.localRotation = originalRotations[modelSekarang];
                });
            }

            if (stoneParticlePrefab != null) {
                Instantiate(stoneParticlePrefab, spawnPos, Quaternion.identity);
            }

            hitCount++; 
            Invoke("UpdateLogo", 0.1f);
            Invoke("UpdateVisualBatuTanpaSkip", 0.1f); 
        } else {
            Hancur();
        }
    }

    Vector3 GetCenterPosition(GameObject model) {
        if (model == null) return transform.position + Vector3.up;
        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.center;
        return model.transform.position + Vector3.up;
    }

    void UpdateVisualBatuTanpaSkip() { UpdateVisualBatu(false); }

    void UpdateVisualBatu(bool isSilent) {
        for (int i = 0; i < crystalModels.Length; i++) {
            GameObject model = crystalModels[i];
            if (model == null) continue;

            if (i == hitCount) {
                model.SetActive(true);
                Vector3 targetScale = originalScales[model];
                model.transform.localRotation = originalRotations[model];

                if (!isSilent) {
                    model.transform.localScale = Vector3.zero;
                    LeanTween.scale(model, targetScale, 0.15f).setEaseOutBack();
                } else {
                    model.transform.localScale = targetScale;
                }
            } else {
                model.SetActive(false);
            }
        }
    }

    void UpdateLogo() {
        if (buttonImage != null && slotSprites.Length > 0) {
            int index = Mathf.Clamp(hitCount, 0, slotSprites.Length - 1);
            buttonImage.sprite = slotSprites[index];
        }
    }

    void Hancur() {
        isDestroyed = true; 
        if(globalChopGroup != null) globalChopGroup.SetActive(false);
        
        GameObject modelTerakhir = crystalModels[hitCount];
        if (modelTerakhir != null) {
            Vector3 centerPos = GetCenterPosition(modelTerakhir);
            LeanTween.scale(modelTerakhir, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() => {
                modelTerakhir.SetActive(false);
                // 🔥 Menghancurkan total game object utama setelah batu hilang agar bersih dari hierarchy scene
                Destroy(gameObject, 0.1f); 
            });
            SpawnCoinLogam(centerPos); 
        }
        // 🛑 Seluruh logika Coroutine Respawn sudah dibersihkan total dari sini!
    }

    void SpawnCoinLogam(Vector3 position) {
        if (coinLogamPrefab == null) return;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // KOIN 1: Lahir di tengah-tengah, agak di atas kepala dikit (ketinggian 2.3f)
        Vector3 posKoin1 = position + new Vector3(0f, 2.3f, 0f);
        SpawnAnakKoin(posKoin1, new Vector3(Random.Range(-0.2f, 0.2f), 2.5f, Random.Range(-0.2f, 0.2f)), player);

        // KOIN 2: Lahir agak geser kanan dikit (jarak 0.5f), tinggi 2.0f, mantul tipis ke kanan
        Vector3 posKoin2 = position + new Vector3(0.5f, 2.0f, Random.Range(-0.3f, 0.3f));
        SpawnAnakKoin(posKoin2, new Vector3(1.2f, 2.0f, Random.Range(0.2f, 0.8f)), player);

        // KOIN 3: Lahir agak geser kiri dikit (jarak -0.5f), tinggi 1.8f, mantul tipis ke kiri
        Vector3 posKoin3 = position + new Vector3(-0.5f, 1.8f, Random.Range(-0.3f, 0.3f));
        SpawnAnakKoin(posKoin3, new Vector3(-1.2f, 1.5f, Random.Range(-0.8f, -0.2f)), player);
    }

    // Fungsi pembantu spawn koin kecil (tetap sama, jangan diubah)
    void SpawnAnakKoin(Vector3 spawnPos, Vector3 forceDirection, GameObject player) {
        GameObject koinKecil = Instantiate(coinLogamPrefab, spawnPos, Quaternion.identity);
        
        koinKecil.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

        Rigidbody rb = koinKecil.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }

        CoinLogamItem logamItem = koinKecil.GetComponent<CoinLogamItem>();
        if (logamItem != null) {
            if (jenisLogam == MiningType.Emas) logamItem.jenisLogam = CoinLogamItem.JenisLogam.Emas;
            else logamItem.jenisLogam = CoinLogamItem.JenisLogam.Perak;

            logamItem.jumlahGram = Mathf.CeilToInt((float)totalHadiahGram / 3f); 
        }

        MiningCoinMagnet magnetBaru = koinKecil.GetComponent<MiningCoinMagnet>();
        if (magnetBaru == null) {
            magnetBaru = koinKecil.AddComponent<MiningCoinMagnet>();
        }
        
        if (player != null) {
            magnetBaru.target = player.transform;
        }
    }
}