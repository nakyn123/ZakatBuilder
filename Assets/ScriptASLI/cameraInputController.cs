using UnityEngine;
using Unity.Cinemachine;

public class CameraInputController : MonoBehaviour
{
    public CinemachineCamera vcam;
    public TouchLookInput touchInput;
    
    [Range(0.1f, 2f)] public float sensitivity = 0.5f;

    void LateUpdate()
    {   
        // WAJIB: Langsung keluar jika skrip ini sedang dimatikan
        if (!this.enabled || vcam == null || touchInput == null) return;

        if (touchInput.LookInput != Vector2.zero)
        {
            var orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                orbitalFollow.HorizontalAxis.Value += touchInput.LookInput.x * sensitivity * Time.deltaTime * 10f;
                orbitalFollow.VerticalAxis.Value -= touchInput.LookInput.y * sensitivity * Time.deltaTime * 10f;
            }
        }
    }
}