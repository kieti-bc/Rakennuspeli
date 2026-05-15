using UnityEngine;
/// <summary>
/// Tämä rakennus rakentaa ajoneuvoja
/// </summary>
public class Depot : Building
{
    private MenuController menuController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();
        menuController = MenuController.GetController();
        menuController.vehicleButtonPressed += OnVehicleButton;
        name = "Depot";
    }

    void OnVehicleButton(GameObject prefab)
    {
       // TODO build vehicle 
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        if (selected)
        {
            // Show vehicle menu
            menuController.ShowVehicleMenu();
        }
        else
        {
            menuController.HideVehicleMenu();
        }
    }
}
