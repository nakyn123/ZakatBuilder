using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TreeSimple : MonoBehaviour {
    [Header("UI Settings")]
    public GameObject globalChopGroup; 
    public Sprite[] slotSprites;

    [Header("Visual Models")]
    public GameObject[] treeModels; 

    [Header("Effects")]
    public GameObject woodParticlePrefab; 
    public GameObject coinModelPrefab; 

    private Button globalButton;
    private Image buttonImage;
    private int hitCount = 0; 

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    void Start() {
        if (globalChopGroup != null) {
            globalButton = globalChopGroup.GetComponentInChildren<Button>();
            buttonImage = globalButton.GetComponent<Image>();
            globalChopGroup.SetActive(false); 
        }

        foreach (GameObject model in treeModels) {
            if (model != null) {
                originalScales[model] = model.transform.localScale;
            }
        }

        UpdateVisualPohon(true); 
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (globalChopGroup != null) {
                globalChopGroup.SetActive(true); 
                UpdateLogo();
                globalButton.onClick.RemoveAllListeners(); 
                globalButton.onClick.AddListener(ActionPotong);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            if (globalChopGroup != null) {
                globalButton.onClick.RemoveAllListeners();
                globalChopGroup.SetActive(false); 
            }
        }
    }

    void ActionPotong() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerMovement moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) {
                // Panggil fungsi Start agar dia loop menebas
                moveScript.StartHarvesting(); 
                
                // Opsional: Tetap hadapkan ke pohon
                Vector3 targetPos = transform.position;
                targetPos.y = player.transform.position.y;
                player.transform.LookAt(targetPos);
            }
        }

        // Logika hitCount dan visual pohon tetap sama seperti kode kamu sebelumnya...
        // [Kode Invoke UpdateVisualPohon dll...]
        
        bool isLastHit = (hitCount == treeModels.Length - 1);

        // Ambil posisi tengah model saat ini agar partikel muncul tepat di batangnya
        Vector3 spawnPos = GetCenterPosition(treeModels[hitCount]);

        if (!isLastHit) {
            GameObject modelSekarang = treeModels[hitCount];
            if (modelSekarang != null) {
                LeanTween.cancel(modelSekarang);
                LeanTween.rotateZ(modelSekarang, 4f, 0.05f).setLoopPingPong(2).setOnComplete(() => {
                    modelSekarang.transform.localRotation = Quaternion.identity;
                });
            }

            if (woodParticlePrefab != null) {
                // Munculkan partikel kayu di tengah model
                Instantiate(woodParticlePrefab, spawnPos, Quaternion.identity);
            }

            hitCount++; 
            Invoke("UpdateLogo", 0.1f);
            Invoke("UpdateVisualPohonTanpaSkip", 0.1f); 
        } else {
            // JIKA TEBASAN TERAKHIR
            Tumbang();
        }
    }

    // Fungsi bantuan untuk mencari titik tengah visual model (agar tidak di kaki pohon)
    Vector3 GetCenterPosition(GameObject model) {
        if (model == null) return transform.position + Vector3.up;
        
        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend != null) {
            return rend.bounds.center;
        }
        return model.transform.position + Vector3.up;
    }

    void UpdateVisualPohonTanpaSkip() { UpdateVisualPohon(false); }

    void UpdateVisualPohon(bool isSilent) {
        for (int i = 0; i < treeModels.Length; i++) {
            GameObject model = treeModels[i];
            if (model == null) continue;

            if (i == hitCount) {
                model.SetActive(true);
                Vector3 targetScale = originalScales[model];

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

    void Tumbang() {
        if(globalChopGroup != null) globalChopGroup.SetActive(false);
        
        GameObject modelTerakhir = treeModels[hitCount];

        if (modelTerakhir != null) {
            // Ambil titik tengah sebelum model diapa-apain
            Vector3 centerPos = GetCenterPosition(modelTerakhir);

            // 1. Animasi skala mengecil (seolah hancur) lalu hilang
            LeanTween.scale(modelTerakhir, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() => {
                modelTerakhir.SetActive(false);
            });

            // 2. Munculkan koin di posisi tengah tadi
            SpawnCoins(centerPos);
        }
    }
    // SPAWN KOIN
    // void SpawnCoins(Vector3 position) {
    //     if (coinModelPrefab == null) return;

    //     int jumlahKoin = 8; // Tambah dikit biar ramai
    //     for (int i = 0; i < jumlahKoin; i++) {
    //         GameObject koin = Instantiate(coinModelPrefab, position, Quaternion.identity);
            
    //         Rigidbody rb = koin.GetComponent<Rigidbody>();
    //         if (rb != null) {
    //             // Efek muncrat koin
    //             Vector3 randomDirection = new Vector3(
    //                 Random.Range(-2f, 2f), 
    //                 Random.Range(2f, 5f), // Muncrat ke atas dulu
    //                 Random.Range(-2f, 2f)
    //             );
                
    //             rb.AddForce(randomDirection, ForceMode.Impulse);
    //         }

    //         CoinMagnet magnet = koin.GetComponent<CoinMagnet>();
    //         if (magnet != null) {
    //             GameObject player = GameObject.FindGameObjectWithTag("Player");
    //             if (player != null) {
    //                 magnet.target = player.transform;
    //             }
    //         }
    //     }
    // }

    void SpawnCoins(Vector3 position) {
        if (coinModelPrefab == null) return;

        // LANGSUNG SPAWN 1 SAJA (Tanpa looping 'for')
        GameObject koin = Instantiate(coinModelPrefab, position, Quaternion.identity);
        
        Rigidbody rb = koin.GetComponent<Rigidbody>();
        if (rb != null) {
            // Kasih efek "loncat" dikit biar nggak kaku
            Vector3 jumpDirection = new Vector3(
                Random.Range(-1f, 1f), 
                3f, // Loncat ke atas
                Random.Range(-1f, 1f)
            );
            
            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }

        // Hubungkan ke magnet biar ditarik ke player
        CoinMagnet magnet = koin.GetComponent<CoinMagnet>();
        if (magnet != null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                magnet.target = player.transform;
            }
        }
    }
}