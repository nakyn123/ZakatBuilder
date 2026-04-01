using UnityEngine;
using UnityEngine.EventSystems;

public class DebugKlik : MonoBehaviour, IPointerDownHandler {
    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("<color=red>FISIK TOMBOL TERDETEKSI!</color>");
    }
}