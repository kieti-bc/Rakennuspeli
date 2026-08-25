using System;
using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
/// <summary>
/// Tämä koodi ohjaa kameraa
/// </summary>
public class CameraController : MonoBehaviour
{
    Vector2 moveVector; // Kuinka nopeasti kamera liikkuu ja minne
    public float moveSpeed;
    public float fovDegreesPerZoom;

    InputAction MoveAction;
    InputAction ZoomAction;

    // Viite rakentamiskoodiin
    BuildingLogic buildingLogic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingLogic = GetComponent<BuildingLogic>();
        // NOTE: NOT reloading domain or scene messes this up 
        // and the actions have to be manually enabled
        InputSystem.actions.Enable();
        MoveAction = InputSystem.actions.FindAction("Player/Move", true);
        ZoomAction = InputSystem.actions.FindAction("Player/Zoom", true);

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(moveVector.x, 0.0f, moveVector.y) * moveSpeed * Time.deltaTime;
        DoMove(MoveAction.ReadValue<Vector2>());
        DoZoom(ZoomAction.ReadValue<float>());
	}

    public void DoMove(Vector2 value)
    {
        Debug.Log($"Move value {value}");
        moveVector = value;
    }

    public void DoZoom(float scrollDelta)
    {
        if (scrollDelta != 0.0f)
        {
            Debug.Log($"Scroll {scrollDelta}");
        }
        // Jos ei olla antamassa käskyjä, zoomaa näkymää
        
            // Laske muutos rullan asennossa
           
            // Laske uusi zoom ja aseta se 
            float newFov = Camera.main.fieldOfView + scrollDelta * fovDegreesPerZoom;
            // Rajoita 25-70 välille
            Camera.main.fieldOfView = Math.Clamp(newFov, 25, 70);
        
    }
}
