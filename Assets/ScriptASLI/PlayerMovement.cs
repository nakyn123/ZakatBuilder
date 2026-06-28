using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public CharacterController controller;
    public float speed = 5f;
    public Joystick joystick; 
    public Transform cameraTransform; 
    
    private float gravity = -9.81f;
    private Vector3 velocity;
    public Animator anim;
    public AudioSource walkAudioSource;

    [Header("Weapon Settings")]
    public GameObject kapakObject; 
    public GameObject beliungObject;

    [Header("Sprint System")]
    public TouchLookInput rightTouchInput; 
    public float sprintSpeedMultiplier = 2f;

    void Start() {
        if (PlayerPrefs.GetInt("IsRestarted", 0) == 0 && PlayerPrefs.HasKey("Saved_PlayerX")) {
            if (controller != null) controller.enabled = false; 

            float x = PlayerPrefs.GetFloat("Saved_PlayerX");
            float y = PlayerPrefs.GetFloat("Saved_PlayerY");
            float z = PlayerPrefs.GetFloat("Saved_PlayerZ");
            transform.position = new Vector3(x, y, z);

            if (controller != null) controller.enabled = true;
            Debug.Log("<color=green>[PlayerMovement]</color> Berhasil memuat posisi koordinat terakhir player.");
        }
        if (kapakObject != null) kapakObject.SetActive(false);
        if (beliungObject != null) beliungObject.SetActive(false);
    }

    public void SimpanPosisiPlayer() {
        PlayerPrefs.SetFloat("Saved_PlayerX", transform.position.x);
        PlayerPrefs.SetFloat("Saved_PlayerY", transform.position.y);
        PlayerPrefs.SetFloat("Saved_PlayerZ", transform.position.z);
        PlayerPrefs.Save();
        Debug.Log("<color=green>[PlayerMovement]</color> Posisi koordinat player berhasil disimpan.");
    }

    public void StartHarvesting() {
        if (anim != null) {
            anim.SetBool("isHarvesting", true);
            anim.speed = 1.5f; 
        }
        if (kapakObject != null) {
            kapakObject.SetActive(true);
        }
    }

    public void StopHarvesting() {
        if (anim != null) {
            anim.SetBool("isHarvesting", false);
            anim.speed = 1f; 
        }
        if (kapakObject != null) {
            kapakObject.SetActive(false);
        }
    }

    public void StartMining() {
        if (anim != null) {
            anim.SetBool("isMining", true); 
            anim.speed = 2.5f; 
        }
        if (beliungObject != null) {
            beliungObject.SetActive(true); 
        }
    }

    public void StopMining() {
        if (anim != null) {
            anim.SetBool("isMining", false);
            anim.speed = 1f; 
        }
        if (beliungObject != null) {
            beliungObject.SetActive(false); 
        }
    } // 🔥 KUNCI PERBAIKAN: Kurung kurawal penutup fungsi StopMining() yang tadi hilang sudah ditambahkan di sini!

    void Update() {
        // if (anim != null && anim.GetBool("isHarvesting")) 
        // {
        //     if (walkAudioSource != null && walkAudioSource.isPlaying) {
        //         walkAudioSource.Stop();
        //     }
        //     return; 
        // }

        float horizontal = joystick.Horizontal; 
        float vertical = joystick.Vertical; 

        Vector3 camForward = cameraTransform.forward; 
        Vector3 camRight = cameraTransform.right; 
        camForward.y = 0; 
        camRight.y = 0; 
        camForward.Normalize(); 
        camRight.Normalize(); 

        Vector3 direction = (camForward * vertical + camRight * horizontal).normalized; 

        if (controller.isGrounded && velocity.y < 0) { 
            velocity.y = -2f; 
        }

        if (direction.magnitude >= 0.1f) { 
            float currentMoveSpeed = speed; 
            bool lagiMenebang = anim != null ? anim.GetBool("isHarvesting") : false; 

            if (rightTouchInput != null && rightTouchInput.IsRunning && !lagiMenebang)
            {
                currentMoveSpeed = speed * sprintSpeedMultiplier; 
                if (anim != null) {
                    anim.SetBool("isWalking", true);  
                    anim.speed = 2f; 
                }
            }
            else
            {
                if (anim != null) {
                    anim.SetBool("isWalking", true);  
                    anim.speed = 1f; 
                }
            }

            controller.Move(direction * currentMoveSpeed * Time.deltaTime); 
            transform.rotation = Quaternion.LookRotation(direction); 
            
            if (walkAudioSource != null && !walkAudioSource.isPlaying) { 
                walkAudioSource.Play(); 
            }
        } else {
            if (anim != null) {
                anim.SetBool("isWalking", false); 
                anim.speed = 1f; 
            }
            if (walkAudioSource != null && walkAudioSource.isPlaying) { 
                walkAudioSource.Stop(); 
            }
        }

        velocity.y += gravity * Time.deltaTime; 
        controller.Move(velocity * Time.deltaTime); 
    }
}