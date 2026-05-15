using System;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Tämä koodi ohjaa kameraa
/// </summary>
public class CameraController : MonoBehaviour
{
    Vector2 moveVector; // Kuinka nopeasti kamera liikkuu ja minne
    public float moveSpeed;
    // Mikä on zoom arvo ja miten nopeasti hiiren rullaa liikutetaan
    private float lastScroll = 0;
    public float fovDegreesPerZoom;
    
    // Viite rakentamiskoodiin
    BuildingLogic buildingLogic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingLogic = GetComponent<BuildingLogic>();

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(moveVector.x, 0.0f, moveVector.y) * moveSpeed * Time.deltaTime;
    }

    public void OnMove(InputValue value)
    {
        moveVector = value.Get<Vector2>();
    }

    public void OnZoom(InputValue value)
    {
        // Jos ei olla antamassa käskyjä, zoomaa näkymää
        if (buildingLogic.GetActiveInputMode() != BuildingLogic.InputMode.Ordering)
        {
            // Laske muutos rullan asennossa
            float scrollNow = value.Get<float>();
            float scrollDelta = lastScroll - scrollNow;
            // Laske uusi zoom ja aseta se 
            float newFov = Camera.main.fieldOfView + scrollDelta * fovDegreesPerZoom;
            // Rajoita 25-70 välille
            Camera.main.fieldOfView = Math.Clamp(newFov, 25, 70);
        }
    }
}
