using UnityEngine;

public class KlikObjekPakan : MonoBehaviour
{
    // Hubungkan ke objek PakanSapi yang memiliki skrip ManajemenMakanan3D
    [SerializeField] private ManajemenMakanan3D manajemenMakanan;

    private void OnMouseDown()
    {
        if (manajemenMakanan != null)
        {
            // Panggil fungsi isi ulang saat objek ini diklik/disentuh
            manajemenMakanan.IsiUlangMakanan();
        }
    }
}