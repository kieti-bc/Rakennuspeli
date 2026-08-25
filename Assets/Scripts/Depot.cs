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
        name = "Depot";
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
			// Show vehicle menu
			menuController.HideVehicleMenu();
		}
            
    }
}
