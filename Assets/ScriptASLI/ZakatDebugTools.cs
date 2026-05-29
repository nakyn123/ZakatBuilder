using UnityEngine;
using UnityEngine.InputSystem; 

public class ZakatDebugTools : MonoBehaviour
{
    [Header("References untuk Debug")]
    public ZakatPanelManager panelManager;

    [Header("Settings Debug")]
    public int bypassMoneyAmount = 100; // Jumlah perak yang didapatkan

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            SkipToLevel2();
        }
    }

    void SkipToLevel2()
    {
        if (panelManager == null)
        {
            panelManager = FindFirstObjectByType<ZakatPanelManager>();
        }

        if (panelManager != null && MoneyManager.instance != null)
        {
            // 1. Buka akses level perdagangan di Carousel
            panelManager.isPerdaganganUnlocked = true;

            // 2. Tambah data perak di MoneyManager
            MoneyManager.instance.AddPerak(bypassMoneyAmount); 

            // 3. Jalankan fungsi manager terpusat untuk memunculkan map level 2 & UI koin kiri
            if (Level2Manager.instance != null)
            {
                Level2Manager.instance.SwitchToLevel2();
            }

            // 4. Reset gerak carousel agar tidak macet
            panelManager.UpdatePaymentButton();
            panelManager.UpdateItemVisuals();

            Debug.Log($"<color=green>[CHEAT ACTIVE]</color> Sukses loncat ke Level 2! {bypassMoneyAmount} Gram Perak masuk.");
        }
    }
}