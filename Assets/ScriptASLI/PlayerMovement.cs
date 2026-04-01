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

        // --- GABUNGKAN LOGIKA GERAK DAN ANIMASI DI SINI ---
        if (direction.magnitude >= 0.1f) {
            controller.Move(direction * speed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);
            
            anim.SetBool("isWalking", true); // Nyalakan animasi jalan
        } else {
            anim.SetBool("isWalking", false); // Matikan animasi jalan
        }
        
        // ------------------------------------------------

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}