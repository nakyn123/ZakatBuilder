using UnityEngine;
using TMPro;

public class NoticeCollider : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject noticeUI; // Seret objek Text Notice ke sini

    void Start()
    {
        // Pastikan teks mati saat start
        if (noticeUI != null) noticeUI.SetActive(false);
    }

    // Terdeteksi saat Player masuk radius
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (noticeUI != null) noticeUI.SetActive(true);
        }
    }

    // Terdeteksi saat Player keluar radius
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (noticeUI != null) noticeUI.SetActive(false);
        }
    }
}