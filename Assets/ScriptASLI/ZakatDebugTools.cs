using UnityEngine;
using UnityEngine.InputSystem; 

public class ZakatDebugTools : MonoBehaviour
{
    [Header("Settings Debug")]
    public int bypassMoneyAmount = 100; 

    void Update()
    {
        // 🟢 CHEAT LEVEL 2 (Tombol K)
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            SkipToLevel2();
        }

        // 🟡 CHEAT LEVEL 3 (Tombol L)
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            SkipToLevel3WithReward();
        }
    }

    // 🟢 FUNGSI CHEAT LOMPAT KE LEVEL 2
    void SkipToLevel2()
    {
        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isPerdaganganUnlocked = true;
            ZakatPanelManager.instance.UpdatePaymentButton();
            ZakatPanelManager.instance.UpdateItemVisuals();
        }

        if (MoneyManager.instance != null)
        {
            MoneyManager.instance.AddPerak(bypassMoneyAmount); 
        }

        if (Level2Manager.instance != null)
        {
            Level2Manager.instance.SwitchToLevel2();
        }

        Debug.Log($"<color=green>[CHEAT ACTIVE]</color> Sukses loncat ke Level 2! {bypassMoneyAmount} Gram Perak masuk.");
    }

    // 🟡 FUNGSI CHEAT LOMPAT KE LEVEL 3
   // 🟡 FUNGSI CHEAT LOMPAT KE LEVEL 3 (INSTAN BYPASS SEPERTI LEVEL 2)
    void SkipToLevel3WithReward()
    {
        Debug.Log("<color=yellow>[Debug Tools]</color> Cheat L Aktif: Instan Bypass ke Level 3 tanpa panel reward!");

        // 1. Unlocked status backend agar carousel menganggap level 3 sudah terbuka
        if (ZakatPanelManager.instance != null)
        {
            ZakatPanelManager.instance.isEmasPerakUnlocked = true; 
            ZakatPanelManager.instance.isPeternakanUnlocked = true; 
            ZakatPanelManager.instance.UpdatePaymentButton();
            ZakatPanelManager.instance.UpdateItemVisuals();
        }

        // 2. LANGSUNG EKSEKUSI LOGIKA TRANSISI UTUH DI LEVEL 3 MANAGER
        if (Level3Manager.instance != null)
        {
            // Pastikan panel reward dipaksa mati (jaga-jaga jika sedang terbuka)
            if (Level3Manager.instance.panelRewardEmasPerak != null)
            {
                Level3Manager.instance.panelRewardEmasPerak.SetActive(false);
            }

            // Panggil langsung fungsi transisi utama:
            // Ini akan otomatis mengonversi koin mentah ke tas, mematikan map level 2, dan membuka barier peternakan!
            Level3Manager.instance.TutupRewardDanMasukLevel3();
        }
        else
        {
            Debug.LogError("[Debug Tools] Level3Manager.instance tidak ditemukan di scene!");
        }
    }
}