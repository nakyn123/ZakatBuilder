using UnityEngine;
using Unity.Cinemachine;

public class CameraInputController : MonoBehaviour
{
    public CinemachineCamera vcam;
    public TouchLookInput touchInput;
    
    [Range(0.1f, 2f)] public float sensitivity = 0.5f;

    void LateUpdate()
    {   
        // Cek apakah komponen aktif dan referensi sudah diisi
        if (!this.enabled || vcam == null || touchInput == null) return;

        // HANYA proses jika ada input (jari sedang bergerak)
        if (touchInput.LookInput != Vector2.zero)
        {
            var orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();

            if (orbitalFollow != null)
            {
                // Perkalian 0.1f agar sensitivitas lebih enak diatur di Inspector
                orbitalFollow.HorizontalAxis.Value += touchInput.LookInput.x * sensitivity * Time.deltaTime * 10f;
                orbitalFollow.VerticalAxis.Value -= touchInput.LookInput.y * sensitivity * Time.deltaTime * 10f;
            }
        }
    }
}