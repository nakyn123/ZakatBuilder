using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TreeSimple : MonoBehaviour {
    public enum TreeType { Kecil, Sedang, Besar }

    [Header("Tree Type Settings")]
    public TreeType jenisPohon; // Pilih di Inspector

    [Header("UI Settings")]
    public GameObject globalChopGroup; 
    public Sprite[] slotSprites;
    public GameObject globalNoChopGroup;
    public GameObject globalNoChopLevel2Group;

    [Header("Visual Models")]
    public GameObject[] treeModels; 

    [Header("Effects")]
    public GameObject woodParticlePrefab; 
    public GameObject coinModelPrefab; 

    [Header("Respawn Settings")]
    public float respawnTime = 5f; 

    [Header("Audio Settings")]
    public AudioSource treeAudioSource; 
    public AudioClip chopSound; // Suara saat menebang (hit)

    private Button globalButton;
    private Image buttonImage;
    private int hitCount = 0; 
    private bool isDestroyed = false; 
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();
    private Vector3 posisiAwalPlayer;
    private Quaternion rotasiAwalPlayer;

    void Start() {
        if (globalChopGroup != null) {
            globalButton = globalChopGroup.GetComponentInChildren<Button>();
            buttonImage = globalButton.GetComponent<Image>();
            globalChopGroup.SetActive(false); 
        }

        if (globalNoChopGroup != null) {
            globalNoChopGroup.SetActive(false);
        }
        if (globalNoChopLevel2Group != null) globalNoChopLevel2Group.SetActive(false);

        foreach (GameObject model in treeModels) {
            if (model != null) {
                originalScales[model] = model.transform.localScale;
                originalRotations[model] = model.transform.localRotation;
            }
        }
        UpdateVisualPohon(true);
    }

    void OnTriggerEnter(Collider other) {
        if (isDestroyed) return;
        if (other.CompareTag("Player")) {
            
            // 1. CEK APAKAH SEKARANG SEDANG DI LEVEL 2
            bool isLevel2 = false;
            if (Level2Manager.instance != null && Level2Manager.instance.environmentLevel2 != null) {
                isLevel2 = Level2Manager.instance.environmentLevel2.activeSelf; 
            }

            // 2. CEK APAKAH SEKARANG SEDANG DI LEVEL 3
            bool isLevel3 = false;
            if (Level3Manager.instance != null && Level3Manager.instance.environmentLevel3 != null) {
                isLevel3 = Level3Manager.instance.environmentLevel3.activeSelf;
            }

            if (isLevel2) {
                // HANYA DI LEVEL 2: Matikan ChopButton & matikan NoChop Lvl 1, HIDUPKAN teks-tebang-lvl2
                if (globalChopGroup != null) globalChopGroup.SetActive(false);
                if (globalNoChopGroup != null) globalNoChopGroup.SetActive(false);
                if (globalNoChopLevel2Group != null) globalNoChopLevel2Group.SetActive(true);
            }
            else if (isLevel3) {
                // 🌟 LOGIKA BARU HANYA DI LEVEL 3 🌟
                if (globalNoChopLevel2Group != null) globalNoChopLevel2Group.SetActive(false);

                // Periksa apakah peternakan sudah memasuki masa Wajib Zakat di buku jurnal
                bool sudahWajibZakatTernak = false;
                if (JurnalManager.instance != null) {
                    sudahWajibZakatTernak = JurnalManager.instance.isTernakNisabReached && JurnalManager.instance.isTernakHaulComplete;
                }

                if (sudahWajibZakatTernak) {
                    // Jika sudah wajib zakat ternak, KUNCI POHON! Munculkan peringatan NoChop Level 1 (atau modifikasi teksnya jika perlu)
                    if (globalChopGroup != null) globalChopGroup.SetActive(false);
                    if (globalNoChopGroup != null) globalNoChopGroup.SetActive(true); 
                }
                else {
                    // Selama belum wajib zakat ternak, pohon BEBAS DI-TEBANG kembali seperti biasa!
                    if (globalNoChopGroup != null) globalNoChopGroup.SetActive(false);
                    if (globalChopGroup != null) {
                        globalChopGroup.SetActive(true);
                        UpdateLogo();
                        globalButton.onClick.RemoveAllListeners();
                        globalButton.onClick.AddListener(ActionPotong);
                    }
                }
            }
            else {
                // JIKA MASIH DI LEVEL 1 (Logika lamamu tetap aman berjalan di sini)
                if (globalNoChopLevel2Group != null) globalNoChopLevel2Group.SetActive(false);

                bool sudahNisabLevel1 = false;
                if (TaskManager.instance != null) {
                    sudahNisabLevel1 = (bool)System.Type.GetType("TaskManager").GetField("isNisabMisiDone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(TaskManager.instance);
                }

                if (sudahNisabLevel1) {
                    if (globalChopGroup != null) globalChopGroup.SetActive(false);
                    if (globalNoChopGroup != null) globalNoChopGroup.SetActive(true);
                } 
                else {
                    if (globalNoChopGroup != null) globalNoChopGroup.SetActive(false);
                    if (globalChopGroup != null) {
                        globalChopGroup.SetActive(true);
                        UpdateLogo();
                        globalButton.onClick.RemoveAllListeners();
                        globalButton.onClick.AddListener(ActionPotong);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerMovement moveScript = other.GetComponent<PlayerMovement>();
            if (moveScript != null) {
                moveScript.StopHarvesting();
            }

            // 🔥 Bersihkan dan matikan seluruh UI Group saat menjauhi area pohon
            if (globalChopGroup != null) {
                globalButton.onClick.RemoveAllListeners();
                globalChopGroup.SetActive(false); 
            }
            if (globalNoChopGroup != null) {
                globalNoChopGroup.SetActive(false);
            }
            if (globalNoChopLevel2Group != null) {
                globalNoChopLevel2Group.SetActive(false);
            }
        }
    }

    void ActionPotong() {
        if (isDestroyed) return;

        if (treeAudioSource != null && chopSound != null) {
            treeAudioSource.PlayOneShot(chopSound);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerMovement moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) {
                moveScript.StartHarvesting(); 
                
                // 🌟 KUNCI: Kunci pergerakan player begitu aktivitas menebang dimulai
                moveScript.isLockedInAction = true; 

                if (moveScript.anim != null) {
                    moveScript.anim.Play("Tebang", 0, 0f); 
                }
            }
        }
        
        bool isLastHit = (hitCount == treeModels.Length - 1);
        Vector3 spawnPos = GetCenterPosition(treeModels[hitCount]);

        if (!isLastHit) {
            GameObject modelSekarang = treeModels[hitCount];
            if (modelSekarang != null) {
                LeanTween.cancel(modelSekarang);
                LeanTween.rotateZ(modelSekarang, 4f, 0.05f).setLoopPingPong(2).setOnComplete(() => {
                    modelSekarang.transform.localRotation = originalRotations[modelSekarang];
                });
            }

            if (woodParticlePrefab != null) {
                Instantiate(woodParticlePrefab, spawnPos, Quaternion.identity);
            }

            hitCount++; 
            Invoke("UpdateLogo", 0.1f);
            Invoke("UpdateVisualPohonTanpaSkip", 0.1f); 
        } else {
            Tumbang();
        }
    }

    void Tumbang() {
        isDestroyed = true; 
        if(globalChopGroup != null) globalChopGroup.SetActive(false);
        
        // 🌟 KUNCI: Lepas kunci pergerakan player SEGERA saat klik terakhir (pohon tumbang)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerMovement moveScript = player.GetComponent<PlayerMovement>();
            if (moveScript != null) {
                moveScript.isLockedInAction = false; // Player bisa jalan lagi!
                moveScript.StopHarvesting();        // Matikan animasi & kapak langsung
            }
        }

        GameObject modelTerakhir = treeModels[hitCount];
        if (modelTerakhir != null) {
            Vector3 centerPos = GetCenterPosition(modelTerakhir);
            LeanTween.scale(modelTerakhir, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() => {
                modelTerakhir.SetActive(false);
            });
            SpawnCoins(centerPos); 
        }

        if (InventoryManager.instance != null && TaskManager.instance != null) {
            int estimasiTotalKayu = InventoryManager.instance.totalWoodCollected + 1;
            TaskManager.instance.UpdateTebangProgress(estimasiTotalKayu);
        }

        StartCoroutine(RespawnRoutine());
    }

    Vector3 GetCenterPosition(GameObject model) {
        if (model == null) return transform.position + Vector3.up;
        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.center;
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

    IEnumerator RespawnRoutine() {
        yield return new WaitForSeconds(respawnTime);
        hitCount = 0;
        isDestroyed = false;
        UpdateVisualPohon(false); 
        UpdateLogo();
    }

    void SpawnCoins(Vector3 position) {
        if (coinModelPrefab == null) return;
        GameObject koin = Instantiate(coinModelPrefab, position, Quaternion.identity);
        Rigidbody rb = koin.GetComponent<Rigidbody>();
        if (rb != null) {
            Vector3 jumpDirection = new Vector3(Random.Range(-1f, 1f), 3f, Random.Range(-1f, 1f));
            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }
        CoinMagnet magnet = koin.GetComponent<CoinMagnet>();
        if (magnet != null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) magnet.target = player.transform;
            magnet.woodType = (int)jenisPohon;
        }
    }
    
}