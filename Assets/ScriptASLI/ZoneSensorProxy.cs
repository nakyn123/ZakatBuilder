using UnityEngine;

public class ZoneSensorProxy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Level3Manager.instance != null)
        {
            Level3Manager.instance.HandleSensorWilayahMasuk();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && Level3Manager.instance != null)
        {
            Level3Manager.instance.HandleSensorWilayahKeluar();
        }
    }
}