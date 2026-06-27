using UnityEngine;
using TMPro; // Diperlukan jika teks peringatan menggunakan TextMeshPro

public class ZoneBarierUmumSensor : MonoBehaviour
{
    [Header("UI Peringatan")]
    [SerializeField] private GameObject txtPeringatanBarierUmum; 

    void Start()
    {
        // Pastikan teks dalam keadaan mati saat game dimulai
        if (txtPeringatanBarierUmum != null)
        {
            txtPeringatanBarierUmum.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ketika Player menabrak collider barier umum
        if (other.CompareTag("Player"))
        {
            HandleSensorMasuk();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Ketika Player keluar dari collider barier umum
        if (other.CompareTag("Player"))
        {
            HandleSensorKeluar();
        }
    }

    // Fungsi untuk menampilkan teks peringatan
    private void HandleSensorMasuk()
    {
        if (txtPeringatanBarierUmum != null)
        {
            txtPeringatanBarierUmum.SetActive(true);
        }
    }

    // Fungsi untuk menyembunyikan teks peringatan
    private void HandleSensorKeluar()
    {
        if (txtPeringatanBarierUmum != null)
        {
            txtPeringatanBarierUmum.SetActive(false);
        }
    }
}