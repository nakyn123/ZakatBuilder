using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public CharacterController controller;
    public float speed = 5f;
    public Joystick joystick; 
    
    // 1. Tambahkan referensi kamera
    public Transform cameraTransform; 
    
    private float gravity = -9.81f;
    private Vector3 velocity;
    public Animator anim;
    public AudioSource walkAudioSource;
    [Header("Sprint System")]
    [Tooltip("Hubungkan script TouchLookInput dari objek kanan ke sini")]
    public TouchLookInput rightTouchInput; 
    public float sprintSpeedMultiplier = 2f;

   // Di PlayerMovement.cs
    public void StartHarvesting() {
        if (anim != null) {
            anim.SetBool("isHarvesting", true);
        }
    }

    public void StopHarvesting() {
        if (anim != null) {
            anim.SetBool("isHarvesting", false);
        }
    }
    void Update() {
        float horizontal = joystick.Horizontal; //
        float vertical = joystick.Vertical; //

        Vector3 camForward = cameraTransform.forward; //
        Vector3 camRight = cameraTransform.right; //
        camForward.y = 0; //
        camRight.y = 0; //
        camForward.Normalize(); //
        camRight.Normalize(); //

        Vector3 direction = (camForward * vertical + camRight * horizontal).normalized; //

        if (controller.isGrounded && velocity.y < 0) { //
            velocity.y = -2f; //
        }

        // --- MANAJEMEN GERAKAN, ANIMASI JALAN CEPAT, DAN AUDIO ---
        if (direction.magnitude >= 0.1f) { 
            float currentMoveSpeed = speed; //

            // Ambil status apakah di Animator sedang memanen/menebang pohon
            bool lagiMenebang = anim != null ? anim.GetBool("isHarvesting") : false; //

            // Jika layar kanan mendeteksi HOLD, joystick kiri digerakkan, DAN TIDAK sedang menebang pohon
            if (rightTouchInput != null && rightTouchInput.IsRunning && !lagiMenebang)
            {
                // 🏃‍♂️ KONDISI LARI (JALAN CEPAT)
                // 1. Set kecepatan gerak fisik karakter di map menjadi 10 (Multiplier 2x dari speed = 5)
                currentMoveSpeed = speed * sprintSpeedMultiplier; //
                
                if (anim != null) {
                    anim.SetBool("isWalking", true);  //
                    
                    // ⚡ INI KUNCINYA: Putaran animasi jalan dipercepat 2 kali lipat biar serasi dengan speed 10
                    anim.speed = 2f; 
                }
            }
            else
            {
                // 🚶‍♂️ KONDISI JALAN NORMAL
                // 2. Set kecepatan jalan normal (Speed = 5)
                if (anim != null) {
                    anim.SetBool("isWalking", true);  //
                    
                    // Putaran animasi jalan diset normal kembali (1)
                    anim.speed = 1f; //
                }
            }

            // Jalankan pergerakan fisik karakter di dunia 3D (bisa bernilai 5 atau 10)
            controller.Move(direction * currentMoveSpeed * Time.deltaTime); //
            transform.rotation = Quaternion.LookRotation(direction); //
            
            // Putar audio langkah kaki
            if (walkAudioSource != null && !walkAudioSource.isPlaying) { 
                walkAudioSource.Play(); //
            }
            
        } else {
            // 🧍‍♂️ KONDISI DIAM / IDLE (Joystick Dilepas)
            if (anim != null) {
                anim.SetBool("isWalking", false); //
                
                // Kembalikan ke kecepatan normal untuk animasi diam/idle
                anim.speed = 1f; //
            }
            
            // Hentikan audio jika karakter berhenti total
            if (walkAudioSource != null && walkAudioSource.isPlaying) { //
                walkAudioSource.Stop(); //
            }
        }

        velocity.y += gravity * Time.deltaTime; //
        controller.Move(velocity * Time.deltaTime); //
    }
}