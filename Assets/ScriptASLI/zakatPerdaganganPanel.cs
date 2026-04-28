using UnityEngine;
using UnityEngine.UI;

public class ZakatPerdaganganPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Button btnClose;
    // Tambahkan variabel lain seperti InputField untuk nominal, dll.

    void Start()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));
    }

    // Tambahkan fungsi logika perhitungan zakat perdagangan di sini
}